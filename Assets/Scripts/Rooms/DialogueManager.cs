using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public GameObject demonDialogueBox;
    public GameObject playerDialogueBox;
    public TMP_Text demonText;
    public TMP_Text playerText;

    [Header("Dialogue Settings")]
    public float typingSpeed = 0.05f;
    public float punctuationPause = 0.2f;

    private DialogueLine[] dialogueSequence;
    private int currentLine = 0;
    private bool isDialogueActive = false;
    private bool hasDialogueFinished = false;

    [System.Serializable]
    private class DialogueLine
    {
        public string text;
        public bool isDemonSpeaking;
    }

    void Start()
    {
        dialogueSequence = new DialogueLine[] {
            new DialogueLine { text = "这是哪里？我……我是谁?", isDemonSpeaking = false },
            new DialogueLine { text = "记忆是多余的筹码\n在这里你只需要记得一件事——\n你已入局", isDemonSpeaking = true }, 
            new DialogueLine { text = "什么局？\n你到底是谁？", isDemonSpeaking = false },
            new DialogueLine { text = "你可以叫我J。\n不过现在，你更应该关心的是——\n你的赌注是什么？", isDemonSpeaking = true },
            new DialogueLine { text = "赌注？我什么都没答应！", isDemonSpeaking = false },
            new DialogueLine { text = "哦？可你的灵魂......\n已经押上桌了。\n那么，开始吧。\n第一局——赌你的过去", isDemonSpeaking = true }
        };

        demonDialogueBox.SetActive(false);
        playerDialogueBox.SetActive(false);
        hasDialogueFinished = false;
    }

    public void StartDialogue()
    {
        if (hasDialogueFinished) return;
        isDialogueActive = true;
        startButton?.gameObject.SetActive(false);
        currentLine = 0;
        StartCoroutine(DisplayDialogue());
    }

    IEnumerator DisplayDialogue()
    {
        while (currentLine < dialogueSequence.Length)
        {
            DialogueLine line = dialogueSequence[currentLine];

            demonDialogueBox.SetActive(line.isDemonSpeaking);
            playerDialogueBox.SetActive(!line.isDemonSpeaking);

            TMP_Text activeText = line.isDemonSpeaking ? demonText : playerText;
            activeText.text = "";

            foreach (char letter in line.text.ToCharArray())
            {
                activeText.text += letter;
                float delay = IsPunctuation(letter) ? punctuationPause : typingSpeed;
                yield return new WaitForSeconds(delay);
            }

            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            currentLine++;
        }

        EndDialogue();
    }

    private bool IsPunctuation(char c)
    {
        return c.ToString() == "！" || c.ToString() == "。" || c.ToString() == "？" ||
               c.ToString() == "." || c.ToString() == "!" || c.ToString() == "?";
    }

    void EndDialogue()
    {
        demonDialogueBox.SetActive(false);
        playerDialogueBox.SetActive(false);
        isDialogueActive = false;
        hasDialogueFinished = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isDialogueActive && !hasDialogueFinished)
        {
            StartDialogue();
        }
    }
}