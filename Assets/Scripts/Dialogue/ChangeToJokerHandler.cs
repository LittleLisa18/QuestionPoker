using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class ChangeToJokerHandler : MonoBehaviour
{
    public CardDealer cardDealer;

    [Header("UI组件")]
    public TMP_Text dialogueText;      // 显示对话的Text组件
    public GameObject dialogueBox; // 对话框父物体
    public Image jokerImage;

    [Header("对话内容")]
    [TextArea(3, 10)]
    public List<string> dialogues = new List<string>(); // 对话列表
    public float typingSpeed = 0.05f; // 打字效果速度

    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private Coroutine typingCoroutine;

    
    void Start()
    {
        StartDialogue(dialogues);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && dialogueActive)
        {
            if (isTyping)
            {
                // 如果正在打字，立即完成当前句子
                StopCoroutine(typingCoroutine);
                dialogueText.text = dialogues[currentDialogueIndex];
                isTyping = false;
            }
            else
            {
                // 显示下一句对话
                ShowNextDialogue();
            }
        }
    }

    // 开始对话
    public void StartDialogue(List<string> newDialogues)
    {
        dialogues = newDialogues;
        currentDialogueIndex = 0;
        dialogueActive = true;
        dialogueBox.SetActive(true);
        jokerImage.gameObject.SetActive(true);
        ShowDialogue(dialogues[currentDialogueIndex]);
    }

    // 显示指定对话
    private void ShowDialogue(string dialogue)
    {
        typingCoroutine = StartCoroutine(TypeDialogue(dialogue));
    }

    // 打字机效果协程
    private IEnumerator TypeDialogue(string dialogue)
    {
        isTyping = true;
        dialogueText.text = "";
        
        // 使用正则表达式匹配富文本标签
        string pattern = @"(<.*?>)";
        string[] parts = Regex.Split(dialogue, pattern);
        
        foreach (string part in parts)
        {
            if (string.IsNullOrEmpty(part)) continue;
            
            // 如果是富文本标签，一次性添加
            if (part.StartsWith("<") && part.EndsWith(">"))
            {
                dialogueText.text += part;
                yield return null; // 等待一帧确保标签被渲染
            }
            else
            {
                // 普通文本逐个字符显示
                foreach (char letter in part.ToCharArray())
                {
                    dialogueText.text += letter;
                    yield return new WaitForSeconds(typingSpeed);
                }
            }
        }
        
        isTyping = false;
    }

    // 显示下一句对话
    private void ShowNextDialogue()
    {
        currentDialogueIndex++;
        Debug.Log("currentDialogueIndex: " + currentDialogueIndex);
        
        if (currentDialogueIndex < dialogues.Count)
        {
            ShowDialogue(dialogues[currentDialogueIndex]);
        }
        else
        {
            // 对话结束
            EndDialogue();
        }
    }

    // 结束对话
    private void EndDialogue()
    {
        dialogueActive = false;
        dialogueBox.SetActive(false);
        jokerImage.gameObject.SetActive(false);
        Debug.Log("对话结束");

        cardDealer.InitializeGame();
    }
}
