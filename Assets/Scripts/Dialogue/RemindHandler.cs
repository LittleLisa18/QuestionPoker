using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RemindHandler : MonoBehaviour
{
    public TMP_Text remindText; // 拖拽你的Text组件到这里
    public float fadeDuration = 1f; // 渐隐/渐显持续时间
    public float displayDuration = 3f; // 文字停留时间
    public GameObject remindPanel;
    [TextArea(3, 10)]
    public List<string> messages = new List<string>(); // 初始化List

    private int currentIndex = 0;
    private bool isPlaying = false;

    public void InitializeRemind()
    {
        if (remindText == null)
        {
            remindText = GetComponent<TMP_Text>();
        }
        
        // 显示提示面板
        remindPanel.SetActive(true);

        // 初始状态透明
        remindText.color = new Color(remindText.color.r, remindText.color.g, remindText.color.b, 0);
        
        // 只有有消息时才播放
        if (messages.Count > 0)
        {
            StartCoroutine(TextFadeSequence());
        }
    }

    // 外部添加消息的方法
    public void AddMessage(string message)
    {
        messages.Add(message);
        if (!isPlaying && messages.Count == 1) // 如果是第一条消息，开始播放
        {
            StartCoroutine(TextFadeSequence());
        }
    }

    IEnumerator TextFadeSequence()
    {
        isPlaying = true;
        
        while (currentIndex < messages.Count)
        {
            remindText.text = messages[currentIndex];
            
            // 渐显
            yield return StartCoroutine(FadeText(0f, 1f, fadeDuration));
            
            // 停留
            yield return new WaitForSeconds(displayDuration);
            
            // 如果不是最后一条消息，渐隐
            if (currentIndex < messages.Count - 1)
            {
                yield return StartCoroutine(FadeText(1f, 0f, fadeDuration));
            }
            
            currentIndex++;
        }

        // 最后一条消息处理
        yield return StartCoroutine(FadeText(1f, 0f, fadeDuration));
        
        // 全部播放完成后可以执行其他操作
        isPlaying = false;
        remindPanel.SetActive(false);
    }

    IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        float time = 0f;
        Color color = remindText.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            remindText.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        remindText.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}
