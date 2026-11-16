using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class IntroductionHandler : MonoBehaviour
{
    public CardDealer cardDealer;

    [Header("UI组件")]
    public TMP_Text dialogueText;      // 显示对话的Text组件
    public GameObject dialogueBox; // 对话框父物体
    public Image dialogueBoxImage;
    public List<string> objectNames = new List<string>();
    public Button ProtectButton;

    [Header("对话内容")]
    [TextArea(3, 10)]
    public List<string> dialogues = new List<string>(); // 对话列表
    public float typingSpeed = 0.05f; // 打字效果速度

    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private Coroutine typingCoroutine;
    private Dictionary<string, GameObject> objectReferences = new Dictionary<string, GameObject>();

    
    void Start()
    {
        // 提前存储所有对象的引用
        foreach (string name in objectNames)
        {
            GameObject obj = GameObject.Find(name); 
            if (obj != null)
            {
                objectReferences[name] = obj;
            }
            else
            {
                Debug.LogError($"初始化时找不到对象: {name}");
            }
        }
        SetAllInActive();
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
        dialogueBoxImage.gameObject.SetActive(true);
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
            SetCorrespondingButtonActive(currentDialogueIndex);
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
        SetOtherActive();
        Debug.Log("对话结束");

        cardDealer.InitializeGame();
        gameObject.SetActive(false);
    }

    public void SetCorrespondingButtonActive(int index)
    {
        Debug.Log("SetCorrespondingButtonActive: " + index);
        switch (index)
        {
            case 0:
                break;
            case 1:
                SetActive(objectNames[0]);
                break;
            case 2:
                SetActive(objectNames[1]);
                break;
            case 3:
                SetActive(objectNames[2]);
                break;
            case 4:
                SetActive(objectNames[3]);
                break;
            case 5:
                SetActive(objectNames[4]);
                break;
            case 6:
                SetActive(objectNames[5]);
                break;
            case 7:
                SetActive(objectNames[6]);
                SetActive(objectNames[7]);
                SetActive(objectNames[8]);
                break;
            case 8:
                SetActive(objectNames[9]);
                break;
            case 9:
                break;
            default:
                break;
        }
    }

    public void SetInActive(string name)
    {
        GameObject.Find(name).SetActive(false);
    }

    public void SetActive(string name)
    {
        if (objectReferences.TryGetValue(name, out GameObject obj))
        {
            obj.SetActive(true);
        }
        else
        {
            Debug.LogError($"找不到缓存的对象: {name}");
        }
    }

    public void SetOtherActive()
    {
        SetActive("RoundInfoBg");
    }

    public void SetAllInActive()
    {
        foreach (string name in objectNames)
        {
            SetInActive(name);
        }
    }
}
