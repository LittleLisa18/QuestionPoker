using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

[RequireComponent(typeof(Text))]
public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float defaultTypeInterval = 0.05f; // 默认打字速度
    [SerializeField] private float punctuationDelay = 0.3f;     // 标点符号额外延迟
    
    private Text targetTextComponent;
    private string fullText;
    private Coroutine typingCoroutine;
    private bool isTyping;

    void Awake()
    {
        targetTextComponent = GetComponent<Text>();
        if (targetTextComponent == null)
        {
            Debug.LogError("Text component not found!");
            return;
        }
        
        // 保存完整文本并清空显示
        fullText = targetTextComponent.text;
        targetTextComponent.text = "";
    }

    void Start()
    {
        StartTyping();
    }

    public void StartTyping()
    {
        if (isTyping) return;
        
        StopAllCoroutines();
        typingCoroutine = StartCoroutine(TypingRoutine());
    }

    public void SkipTyping()
    {
        if (!isTyping) return;
        
        StopCoroutine(typingCoroutine);
        targetTextComponent.text = fullText;
        isTyping = false;
    }

    private IEnumerator TypingRoutine()
    {
        isTyping = true;
        StringBuilder currentText = new StringBuilder();
        bool insideTag = false; // 标记是否在富文本标签内

        for (int i = 0; i < fullText.Length; i++)
        {
            char currentChar = fullText[i];
            
            // 处理富文本标签
            if (currentChar == '<')
            {
                insideTag = true;
                string tagContent = ExtractTag(fullText, i);
                currentText.Append(tagContent);
                i += tagContent.Length - 1; // 跳过整个标签
                targetTextComponent.text = currentText.ToString();
                continue;
            }
            
            // 结束标签处理
            if (insideTag && currentChar == '>')
            {
                insideTag = false;
                currentText.Append(currentChar);
                targetTextComponent.text = currentText.ToString();
                continue;
            }
            
            // 普通字符处理
            if (!insideTag)
            {
                currentText.Append(currentChar);
                targetTextComponent.text = currentText.ToString();
                
                // 根据字符类型设置延迟
                float delay = defaultTypeInterval;
                if (IsPunctuation(currentChar))
                {
                    delay += punctuationDelay;
                }
                
                yield return new WaitForSeconds(delay);
            }
            else
            {
                currentText.Append(currentChar);
            }
        }
        
        isTyping = false;
    }

    private string ExtractTag(string text, int startIndex)
    {
        int endIndex = text.IndexOf('>', startIndex);
        if (endIndex == -1) return "";
        return text.Substring(startIndex, endIndex - startIndex + 1);
    }

    private bool IsPunctuation(char c)
    {
        return c == '.' || c == ',' || c == '!' || c == '?' || c == ';' || c == ':';
    }
}
