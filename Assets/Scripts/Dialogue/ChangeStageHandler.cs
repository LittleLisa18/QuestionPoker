using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChangeStageHandler : MonoBehaviour
{
    [Header("UI组件")]
    public TMP_Text dialogueText;      // 显示对话的Text组件
    public GameObject dialogueBox;     // 对话框父物体
    public Image IntroImage;           // 旁白图像
    public Image BossImage;            // 兰斯洛特图像

    [Header("选择按钮")]
    public Button J1Button;
    public Button J2Button;
    public Button J3Button;
    public Button J4Button;
    public Button collectButton;

    [Header("对话内容")]
    [TextArea(3, 10)]
    public List<string> dialogues = new List<string>(); // 对话列表
    [TextArea(3, 10)]
    public List<string> correctDialogues = new List<string>(); // 正确对话列表
    [TextArea(3, 10)]
    public List<string> wrongDialogues = new List<string>(); // 错误对话列表
    public float typingSpeed = 0.05f; // 打字效果速度
    public List<bool> isBossSpeaking;
    public List<bool> isIntroSpeaking;
    public bool isDone = false;

    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private Coroutine typingCoroutine;
    private bool isCorrectChoice = false;
    private bool waitingForChoice = false;

    public void InitializeChangeStage()
    {
        // 初始化按钮状态
        SetButtonsActive(false);
        
        // 添加按钮监听
        J1Button.onClick.AddListener(() => OnChoiceSelected(false));
        J2Button.onClick.AddListener(() => OnChoiceSelected(true));
        J3Button.onClick.AddListener(() => OnChoiceSelected(false));
        J4Button.onClick.AddListener(() => OnChoiceSelected(false));

        // 初始化UI
        dialogueBox.SetActive(true);
        IntroImage.gameObject.SetActive(true);
        BossImage.gameObject.SetActive(false);
        collectButton.gameObject.SetActive(false);
        // 开始对话
        StartDialogue(dialogues);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && dialogueActive && !waitingForChoice)
        {
            if (isTyping)
            {
                // 如果正在打字，立即完成当前句子
                StopCoroutine(typingCoroutine);
                dialogueText.text = GetCurrentDialogueText();
                isTyping = false;
                
                // 如果是第二句，显示按钮
                if (currentDialogueIndex == 1)
                {
                    ShowChoiceButtons();
                }
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
        ShowDialogue(dialogues[currentDialogueIndex]);
    }

    // 显示指定对话
    private void ShowDialogue(string dialogue)
    {
        // 更新说话者图像
        UpdateSpeakerImage();
        
        // 开始打字效果
        typingCoroutine = StartCoroutine(TypeDialogue(dialogue));
    }

    // 更新说话者图像
    private void UpdateSpeakerImage()
    {
        if (currentDialogueIndex < isBossSpeaking.Count)
        {
            BossImage.gameObject.SetActive(isBossSpeaking[currentDialogueIndex]);
            IntroImage.gameObject.SetActive(!isBossSpeaking[currentDialogueIndex]);
        }
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
        
        // 如果是第二句，显示按钮
        if (currentDialogueIndex == 1)
        {
            ShowChoiceButtons();
        }
    }

    // 显示选择按钮
    private void ShowChoiceButtons()
    {
        SetButtonsActive(true);
        waitingForChoice = true;
    }

    // 设置按钮激活状态
    private void SetButtonsActive(bool active)
    {
        J1Button.gameObject.SetActive(active);
        J2Button.gameObject.SetActive(active);
        J3Button.gameObject.SetActive(active);
        J4Button.gameObject.SetActive(active);
        
        J1Button.interactable = active;
        J2Button.interactable = active;
        J3Button.interactable = active;
        J4Button.interactable = active;
    }

    // 显示下一句对话
    private void ShowNextDialogue()
    {
        currentDialogueIndex++;
        
        if (currentDialogueIndex < 9)
        {
            ShowDialogue(GetCurrentDialogueText());
        }
        else
        {
            EndDialogue();
        }
    }

    // 获取当前对话文本
    private string GetCurrentDialogueText()
    {
        if (currentDialogueIndex <= 1)
        {
            return dialogues[currentDialogueIndex];
        }
        else
        {
            return isCorrectChoice ? correctDialogues[currentDialogueIndex - 2] 
                                 : wrongDialogues[currentDialogueIndex - 2];
        }
    }

    // 选择按钮被点击
    private void OnChoiceSelected(bool isCorrect)
    {
        isCorrectChoice = isCorrect;
        waitingForChoice = false;
        SetButtonsActive(false);
        
        // 继续对话
        currentDialogueIndex++;
        ShowDialogue(GetCurrentDialogueText());
    }

    // 结束对话
    private void EndDialogue()
    {
        dialogueActive = false;
        dialogueBox.SetActive(false);
        IntroImage.gameObject.SetActive(false);
        BossImage.gameObject.SetActive(false);
        collectButton.gameObject.SetActive(true);
        isDone = true;
        Debug.Log("对话结束");
    }

    public bool GetIsCorrectAnswer()
    {
        return isCorrectChoice;
    }
}
