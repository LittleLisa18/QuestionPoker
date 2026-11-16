using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StoryTellingHandler : MonoBehaviour
{
    [Header("UI组件")]
    public TMP_Text dialogueText;      // 显示对话的Text组件
    public GameObject dialogueBox; // 对话框父物体
    public Image dialogueBoxImage;
    public GameObject ClueArea;
    public TMP_Text clueText;
    public Button collectButton;

    [Header("对话内容")]
    [TextArea(3, 10)]
    public List<string> dialogues = new List<string>(); // 对话列表
    public float typingSpeed = 0.05f; // 打字效果速度

    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private Coroutine typingCoroutine;
    public bool isDone = false;

    public void InitializeStoryTelling(){
        dialogueBox.SetActive(true);
        dialogueBoxImage.gameObject.SetActive(true);
        StartDialogue(dialogues);
        collectButton.gameObject.SetActive(false);
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
        
        foreach (char letter in dialogue.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
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
        dialogueBoxImage.gameObject.SetActive(false);
        ClueArea.SetActive(true);
        clueText.text = dialogues[0];
        isDone = true;
        collectButton.gameObject.SetActive(true);
        collectButton.interactable = true;
        Debug.Log("对话结束");
    }
}
