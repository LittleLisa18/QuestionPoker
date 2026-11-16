using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class QuestionAndAnswerController : MonoBehaviour
{
    [System.Serializable]
    public struct QuestionAnswer
    {
        public string question;
        public string answer;
    }

    [Header("UI References")]
    public Button question1Button;
    public Button question2Button;
    public Button collectButton;
    public Text question1Text;
    public Text question2Text;
    public Text qaBossText;
    public Text ruleText;

    private List<QuestionAnswer> correctQAs = new List<QuestionAnswer>();
    private List<QuestionAnswer> wrongQAs = new List<QuestionAnswer>();
    private int currentRound = 0;
    private int totalRounds = 5;
    private QuestionAnswer currentQuestion1;
    private QuestionAnswer currentQuestion2;
    private bool isQ1Correct;
    private bool isQ2Correct;
    private bool isFirstSession = true;
    private IEnumerator delayedRoundStart;
    private int correctClueCount = 0;

    [Header("Clue")]
    public GameObject[] Ques;
    public GameObject[] Ans;
    public Text[] QuesText;
    public Text[] AnswerText;
    public Image[] T;
    public Image[] F;
    public Text CorrectClueCountText;

    [Header("打字机效果设置")]
    public float typingSpeed = 0.05f; // 默认打字速度
    public float punctuationDelay = 0.3f; // 标点符号额外延迟

    public void StartQASession()
    {
        if (isFirstSession)
        {
            // 首次运行显示开场白
            StartCoroutine(FirstSessionCoroutine());
            isFirstSession = false;
        }
        else
        {
            // 后续轮次直接开始
            NextRound();
        }
    }

    private void InitClue()
    {
        for (int i = 0; i < Ques.Length; i++)
        {
            Ques[i].SetActive(false);
        }
        CorrectClueCountText.text = "正确线索数：0";
    }

    IEnumerator FirstSessionCoroutine()
    {
        qaBossText.text = "......又一个迷茫的赌徒\n" +
            "你连自己押注的是什么都不知道......\n" +
            "就敢把过去摆上赌桌？\n" +
            "但规则总是仁慈的——\n" +
            "我允许你......质问\n" +
            "每轮从命运中抽取两个问题，向我寻求真相";
            
        yield return StartCoroutine(TypewriterEffect(qaBossText.text));
        
        // yield return new WaitForSeconds(2f); 

        // 延迟后显示剩余文本
        qaBossText.text += "\n但记住——\n问题既是筹码，答案既是陷阱";
        StartCoroutine(TypewriterEffect("\n但记住——\n问题既是筹码，答案既是陷阱"));
        
        yield return new WaitForSeconds(4f);
        
        // 首次轮次开始
        StartNewRound();
    }

    void Start()
    {
        // 初始化正确问题列表
        correctQAs.Add(new QuestionAnswer {
            question = "你是否曾背叛过最信任你的人？",
            answer = "我背叛了国王……也背叛了自己的誓言。但我的剑，始终忠于我的心。"
        });
        correctQAs.Add(new QuestionAnswer {
            question = "你的武器是否被后世传颂？",
            answer = "它被称为'湖上骑士之剑'，但它的光芒……早已被我的罪孽玷污。"
        });
        correctQAs.Add(new QuestionAnswer {
            question = "你是否因爱情而失去一切？",
            answer = "我赢得了她的心，却输掉了王国……和我的灵魂。"
        });
        correctQAs.Add(new QuestionAnswer {
            question = "你是否曾追寻过神圣之物？",
            answer = "我见过……但我不配触碰它。只有我的儿子，达到了我未能企及的纯洁。"
        });
        correctQAs.Add(new QuestionAnswer {
            question = "你是否仍在寻求救赎？",
            answer = "我余生都在修道院中祈祷……但有些罪，连上帝都不会宽恕。"
        });
        correctQAs.Add(new QuestionAnswer {
            question = "你是否属于一个传奇的骑士团体？",
            answer = "我曾是'XX'(哎我不告诉你)的象征……直到我亲手毁了它。"
        });
        correctQAs.Add(new QuestionAnswer {
            question = "你是否有无法击败的对手？",
            answer = "我击败了所有敌人……除了我自己。"
        });
        correctQAs.Add(new QuestionAnswer {
            question = "你是否与'湖'有特殊的联系？",
            answer = "我被称作'湖上骑士'……但湖水再清澈，也洗不净我的罪。"
        });

        // 初始化错误问题列表
        wrongQAs.Add(new QuestionAnswer {
            question = "你是否出生在特洛伊？",
            answer = "......不，这与你要找的人无关。"
        });
        wrongQAs.Add(new QuestionAnswer {
            question = "你是否曾为查理曼大帝效力？",
            answer = "......不是我。你问错人了。"
        });
        wrongQAs.Add(new QuestionAnswer {
            question = "你是否曾在百年战争中驰骋？",
            answer = "......这不是我的年代。"
        });
        wrongQAs.Add(new QuestionAnswer {
            question = "你是否以弓箭闻名？",
            answer = "......我从不以弓为武。"
        });
        wrongQAs.Add(new QuestionAnswer {
            question = "你的荣耀是否因守护家园而传颂？",
            answer = "......我的荣耀与家园无关。"
        });
        wrongQAs.Add(new QuestionAnswer {
            question = "你是否因与阿格琉斯决斗而闻名？",
            answer = "......你说的是别人，不是我。"
        });
        wrongQAs.Add(new QuestionAnswer {
            question = "你是否死于敌国的围攻？",
            answer = "......我的结局不是在围城之下。"
        });
        wrongQAs.Add(new QuestionAnswer {
            question = "你的故事是否被记载于《伊利亚特》？",
            answer = "......我的名字不在那部史诗里。"
        });

        // 添加按钮监听
        question1Button.onClick.AddListener(OnQuestion1ButtonClicked);
        question2Button.onClick.AddListener(OnQuestion2ButtonClicked);

        // 按钮不显示
        question1Button.gameObject.SetActive(false);
        question2Button.gameObject.SetActive(false);
        collectButton.interactable = false; 

        InitClue();

        // 初始化规则文本			
        ruleText.text = "规则：\n" + "1）玩家每轮从两个问题中选择一个来进行提问\n" +
        "2）两个问题中一个是正确方向的问题，另外一个则是错误方向的问题\n" +
        "3）玩家提问的正确方向与否将会影响正确线索数量的收集\n" +
        "4）正确的线索数量越多，在第二阶段玩家所面对的敌人将会越“虚弱”\n" +
        "5）如果感到迷茫，记得查看资料板并理清敌人的身份";
    }

    void StartNewRound()
    {
        if (currentRound >= totalRounds)
        {
            // 结束游戏
            // 质问结束消息
            question1Button.interactable = false;
            question2Button.interactable = false;
            return;
        }

        // 随机选择正确问题和错误问题
        int correctIndex = Random.Range(0, correctQAs.Count);
        int wrongIndex = Random.Range(0, wrongQAs.Count);

        QuestionAnswer correctQA = correctQAs[correctIndex];
        QuestionAnswer wrongQA = wrongQAs[wrongIndex];

        // 从列表中移除已选问题
        correctQAs.RemoveAt(correctIndex);
        wrongQAs.RemoveAt(wrongIndex);

        // 随机分配问题到按钮
        if (Random.Range(0, 2) == 0)
        {
            currentQuestion1 = correctQA;
            currentQuestion2 = wrongQA;
            isQ1Correct = true;
            isQ2Correct = false;
        }
        else
        {
            currentQuestion1 = wrongQA;
            currentQuestion2 = correctQA;
            isQ1Correct = false;
            isQ2Correct = true;
        }

        // 更新UI
        question1Text.text = currentQuestion1.question;
        question2Text.text = currentQuestion2.question;

        // 启用按钮
        question1Button.gameObject.SetActive(true);
        question2Button.gameObject.SetActive(true);
        question1Button.interactable = true;
        question2Button.interactable = true;

        // 清空BOSS文本
        qaBossText.text = "请选择一个问题向我提问吧...\n（按下想选的问题前面的问题框即可）";
    }

    public void OnQuestion1ButtonClicked()
    {
        ShowAnswer(currentQuestion1.question, currentQuestion1.answer, isQ1Correct);
        collectButton.interactable = true;
    }

    public void OnQuestion2ButtonClicked()
    {
        ShowAnswer(currentQuestion2.question, currentQuestion2.answer, isQ2Correct);
        collectButton.interactable = true;
    }

    void ShowAnswer(string question, string answer, bool isCorrect)
    {
        // 在质问面板显示答案
        qaBossText.text = answer;
        question1Button.interactable = false;
        question2Button.interactable = false;

        // 在线索面板显示答案
        Ques[currentRound].SetActive(true);
        Ans[currentRound].SetActive(true);
        QuesText[currentRound].text = question;
        AnswerText[currentRound].text = answer;
        T[currentRound].gameObject.SetActive(isCorrect);
        F[currentRound].gameObject.SetActive(!isCorrect);

        // 计算正确线索数量
        if (isCorrect)
        {
            correctClueCount++;
        }

        CorrectClueCountText.text = "正确线索数：" + correctClueCount;

    }

    void NextRound()
    {
        collectButton.interactable = false; 
        currentRound++;
        StartNewRound();
    }

    public int GetCorrectClueCount()
    {
        return correctClueCount;
    }

     // 打字机效果协程
    private IEnumerator TypewriterEffect(string fullText)
    {
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
                qaBossText.text = currentText.ToString();
                continue;
            }
            
            // 结束标签处理
            if (insideTag && currentChar == '>')
            {
                insideTag = false;
                currentText.Append(currentChar);
                qaBossText.text = currentText.ToString();
                continue;
            }
            
            // 普通字符处理
            if (!insideTag)
            {
                currentText.Append(currentChar);
                qaBossText.text = currentText.ToString();
                
                // 根据字符类型设置延迟
                float delay = typingSpeed;
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
        
        // // 打字完成后等待3秒再开始第一轮
        yield return new WaitForSeconds(3f);
        // StartNewRound();
    }

    // 提取富文本标签
    private string ExtractTag(string text, int startIndex)
    {
        int endIndex = text.IndexOf('>', startIndex);
        if (endIndex == -1) return "";
        return text.Substring(startIndex, endIndex - startIndex + 1);
    }

    // 判断是否是标点符号
    private bool IsPunctuation(char c)
    {
        return c == ',' || c == '!' || c == '?' || c == ';' || c == ':' || c == '—';
    }
}
