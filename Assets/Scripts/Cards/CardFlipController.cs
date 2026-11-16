using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class CardFlipController : MonoBehaviour
{
    [Header("UI设置")]
    public Button OpenCardButton; // 开牌按钮

    [Header("动画设置")]
    public float flipDuration = 0.8f; // 翻牌动画持续时间
    public float delayBetweenFlips = 0.2f; // 卡牌之间的翻牌延迟

    [Header("音效")]
    private AudioSource cardAudioSource;
    public AudioClip flipCardAudio;
    public Button PropButton;

    // 卡牌花色枚举
    public enum Suit { Diamonds, Flowers, Hearts, Spades }
    // 卡牌点数枚举
    public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }
    public struct CardData
    {
        public Suit suit;
        public Rank rank;
        public CardDragHandler cardObject;
        
        public override string ToString()
        {
            return $"{rank} of {suit}";
        }
    }
    public CardComparisonManager cardComparisonManager; 

    private bool hasFlipped = false; // 新增：标记是否已经翻过牌
    public bool isPropUsed = true; // 新增：道具是否被使用，初始为true

    private BettingManager bettingManager;
    void Start()
    {
        bettingManager = FindObjectOfType<BettingManager>();
        if (bettingManager == null)
        {
            Debug.LogError("BettingManager not found!");
        }
        // 绑定按钮点击事件
        OpenCardButton.onClick.AddListener(FlipAllCards);
    }

    void Awake()
    {
        cardAudioSource = GetComponent<AudioSource>();
        if (cardAudioSource == null)
        {
            cardAudioSource = gameObject.AddComponent<AudioSource>();
            cardAudioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        // 每帧检查卡牌完整性
        UpdateButtonState();
    }

    public void ResetFlipState()
    {
        hasFlipped = false;
        OpenCardButton.interactable = true; // 确保按钮可用
    }

    // 更新按钮状态
    private void UpdateButtonState()
    {
        // 如果已经翻过牌，按钮保持禁用状态
        if (hasFlipped)
        {
            OpenCardButton.interactable = false;
            return;
        }
        // 如果道具没被使用，禁用开牌按钮
        if (!isPropUsed)
        {
            OpenCardButton.interactable = false;
            return;
        }
        // 检查所有池子是否都有卡牌
        bool allPoolsHaveCards = CheckAllPoolsHaveCards();

        // 设置按钮交互状态
        OpenCardButton.interactable = allPoolsHaveCards;
    }

    // 检查所有池子是否都有卡牌
    private bool CheckAllPoolsHaveCards()
    {
        // 检查所有Enemy_Pool
        for (int i = 1; i <= 7; i++)
        {
            GameObject pool = GameObject.Find($"Enemy_Pool{i}");
            if (pool != null && !HasCardInPool(pool))
            {
                return false;
            }
        }

        // 检查所有Player_Pool
        for (int i = 1; i <= 7; i++)
        {
            GameObject pool = GameObject.Find($"Player_Pool{i}");
            if (pool != null && !HasCardInPool(pool))
            {
                return false;
            }
        }

        return true;
    }

    // 检查单个池子是否有卡牌
    private bool HasCardInPool(GameObject pool)
    {
        foreach (Transform child in pool.transform)
        {
            if (child.name.StartsWith("Card(Clone)"))
            {
                return true;
            }
        }
        return false;
    }

    // 翻动所有卡牌
    public void FlipAllCards()
    {
        // 禁用按钮防止重复点击
        OpenCardButton.interactable = false;

        // 标记已经翻过牌
        hasFlipped = true;

        StartCoroutine(FlipCardsCoroutine());
        PropButton.interactable = false;
    }

    IEnumerator FlipCardsCoroutine()
    {
        // 获取所有池子
        GameObject[] enemyPools = new GameObject[7];
        GameObject[] playerPools = new GameObject[7];

        for (int i = 1; i <= 7; i++)
        {
            enemyPools[i - 1] = GameObject.Find($"Enemy_Pool{i}");
            playerPools[i - 1] = GameObject.Find($"Player_Pool{i}");
        }

        // 先遍历敌方池中的卡牌，优先翻有 CardJHandler 组件的卡牌
        List<GameObject> priorityEnemyCards = new List<GameObject>();
        for (int i = 0; i < 7; i++)
        {
            GameObject enemyCard = FindCardInPool(enemyPools[i]);
            if (enemyCard != null && enemyCard.GetComponent<CardJHandler>() != null)
            {
                priorityEnemyCards.Add(enemyCard);
            }
        }

        // 先翻有 CardJHandler 组件的敌方卡牌
        foreach (var card in priorityEnemyCards)
        {
            Transform back = card.transform.Find("Back");
            Transform front = card.transform.Find("Front");
            if (back != null && front != null && back.gameObject.activeSelf && !front.gameObject.activeSelf)
            {
                StartCoroutine(FlipSingleCard(card, back, front));
                // yield return new WaitForSeconds(delayBetweenFlips); // 等待翻完敌人卡牌再翻玩家卡牌
                yield return new WaitForSeconds(1f);
            }
        }

        // 将有 CardJHandler 组件的敌方卡牌转换为A
        foreach (var card in priorityEnemyCards)
        {
            Transform back = card.transform.Find("Back");
            Transform front = card.transform.Find("Front");
            if (back != null && front != null && front.gameObject.activeSelf && !back.gameObject.activeSelf)
            {
                StartCoroutine(ChangeSingleCard(card, back, front));
                // yield return new WaitForSeconds(delayBetweenFlips); // 等待翻完敌人卡牌再翻玩家卡牌
                yield return new WaitForSeconds(1f);
            }
        }

        // 交替翻牌，先敌人后玩家
        for (int i = 0; i < 7; i++)
        {
            GameObject enemyCard = FindCardInPool(enemyPools[i]);
            GameObject playerCard = FindCardInPool(playerPools[i]);

            // 先翻敌人卡牌
            if (enemyCard != null)
            {
                Transform back = enemyCard.transform.Find("Back");
                Transform front = enemyCard.transform.Find("Front");
                if (back != null && front != null && back.gameObject.activeSelf && !front.gameObject.activeSelf)
                {
                    StartCoroutine(FlipSingleCard(enemyCard, back, front));
                    yield return new WaitForSeconds(delayBetweenFlips); // 等待翻完敌人卡牌再翻玩家卡牌
                }
            }

            // 再翻玩家卡牌
            if (playerCard != null)
            {
                Transform back = playerCard.transform.Find("Back");
                Transform front = playerCard.transform.Find("Front");
                if (back != null && front != null && back.gameObject.activeSelf && !front.gameObject.activeSelf)
                {
                    StartCoroutine(FlipSingleCard(playerCard, back, front));
                    yield return new WaitForSeconds(delayBetweenFlips); // 等待翻完玩家卡牌再翻下一张敌人卡牌
                }
            }
        }
        cardComparisonManager.StartComparison();
        // yield break;
    }

    // 在池中查找卡牌对象
    private GameObject FindCardInPool(GameObject pool)
    {
        foreach (Transform child in pool.transform)
        {
            if (child.name.StartsWith("Card(Clone)"))
            {
                return child.gameObject;
            }
        }
        return null;
    }

    // 翻动单张卡牌
    IEnumerator FlipSingleCard(GameObject card, Transform back, Transform front)
    {
        if (front.gameObject.activeSelf)
        {
            yield break;  // 跳过这张卡牌的翻牌
        }

        // 初始状态：显示背面，隐藏正面
        back.gameObject.SetActive(true);
        front.gameObject.SetActive(false);

        // 创建旋转序列
        Sequence flipSequence = DOTween.Sequence();

        // 第一阶段：旋转90度（到侧面）
        flipSequence.Append(card.transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2)
            .SetEase(Ease.OutQuad)).OnStart(() => {
                cardAudioSource.PlayOneShot(flipCardAudio);
            });

        // 第二阶段：切换正反面并完成旋转
        flipSequence.AppendCallback(() =>
        {
            // 切换正反面显示
            back.gameObject.SetActive(false);
            front.gameObject.SetActive(true);

            // 设置到180度位置
            card.transform.eulerAngles = new Vector3(0, 270, 0);
        });

        // 第三阶段：完成旋转到0度
        flipSequence.Append(card.transform.DORotate(Vector3.zero, flipDuration / 2)
            .SetEase(Ease.InQuad));

        yield return flipSequence.WaitForCompletion();
    }

    // 翻动单张卡牌的动画
    IEnumerator ChangeSingleCard(GameObject card, Transform back, Transform front)
    {
        EnemyCardSelectionHandler selectionHandler = card.GetComponent<EnemyCardSelectionHandler>();

        if (selectionHandler != null)
        {
            // 使用 EnemyCardSelectionHandler 中记录的原始缩放值
            card.transform.localScale = selectionHandler.GetOriginalScale();
        }

        // 初始状态：显示正面，隐藏背面（重要调整）
        back.gameObject.SetActive(false);
        front.gameObject.SetActive(true);

        Sequence flipSequence = DOTween.Sequence();

        // ===== 第一阶段：正面翻转到背面 =====
        // 0° → 90°（前半段）
        flipSequence.Append(card.transform.DORotate(new Vector3(0, 90, 0), flipDuration / 4)
            .SetEase(Ease.OutQuad)).OnStart(() => {
                cardAudioSource.PlayOneShot(flipCardAudio);
            });

        // 90°时切换显示：隐藏正面，显示背面
        flipSequence.AppendCallback(() =>
        {
            front.gameObject.SetActive(false);
            back.gameObject.SetActive(true);
        });

        // 90° → 180°（后半段）
        flipSequence.Append(card.transform.DORotate(new Vector3(0, 180, 0), flipDuration / 4)
            .SetEase(Ease.InQuad));

        // ===== 关键点：背面朝上时更新点数 =====
        flipSequence.AppendCallback(() =>
        {
            // 从卡牌对象获取rank和suit
            Rank rank = Rank.Ace;  // 强制将rank更新为 A (Ace)
            Suit suit = GetSuitFromCard(card.GetComponent<CardDragHandler>());  // 获取花色

            // 输出当前的点数和花色
            Debug.Log($"Before Increment: {rank} of {suit}");

            // 更新卡牌的视觉
            UpdateCardVisual(card, rank, suit);

            Debug.Log($"After Increment: {rank} of {suit}");
        });

        // ===== 第二阶段：背面翻回正面 =====
        // 180° → 270°
        flipSequence.Append(card.transform.DORotate(new Vector3(0, 270, 0), flipDuration / 4)
            .SetEase(Ease.OutQuad));

        // 270°时切换显示：隐藏背面，显示新点数正面
        flipSequence.AppendCallback(() =>
        {
            back.gameObject.SetActive(false);
            front.gameObject.SetActive(true);
        });

        // 270° → 360°（0°）
        flipSequence.Append(card.transform.DORotate(Vector3.zero, flipDuration / 4)
            .SetEase(Ease.InQuad));

        yield return flipSequence.WaitForCompletion();
    }

    // 更新卡牌显示
    void UpdateCardVisual(GameObject card, Rank rank, Suit suit)
    {
        // 找到卡牌上的相关 Transform
        Transform type = card.transform.Find("Front/Type");
        Transform suit1 = card.transform.Find("Front/Suit1");
        Transform suit2 = card.transform.Find("Front/Suit2");

        string rankString;
        switch (rank)
        {
            case Rank.King:
                rankString = "K";
                break;
            case Rank.Queen:
                rankString = "Q";
                break;
            case Rank.Jack:
                rankString = "J";
                break;
            case Rank.Ace:
                rankString = "A";
                break;
            default:
                // 对于 2 到 10 的数字，直接使用数字字符串
                rankString = ((int)rank).ToString();
                break;
        }

        // 根据传入的 rank 和 suit 加载类型和花色的精灵
        Sprite typeSprite = Resources.Load<Sprite>("CardElements/Types/" + rankString); // 根据 rank 加载类型
        Sprite suitSprite = Resources.Load<Sprite>("CardElements/Suits/" + suit.ToString()); // 根据 suit 加载花色

        // 更新卡牌的类型精灵
        if (type != null)
        {
            SpriteRenderer typeRenderer = type.GetComponent<SpriteRenderer>();
            if (typeRenderer != null)
            {
                typeRenderer.sprite = typeSprite;
            }

            // 设置红色或黑色花色
            if (suit == Suit.Hearts || suit == Suit.Diamonds)
            {
                typeRenderer.color = new Color(0.8f, 0.1f, 0.1f); // 红色花色（红桃、方块）
            }
            else
            {
                typeRenderer.color = Color.black; // 黑色花色（黑桃、梅花）
            }
        }

        // 更新卡牌的花色精灵
        if (suit1 != null) suit1.GetComponent<SpriteRenderer>().sprite = suitSprite;
        if (suit2 != null) suit2.GetComponent<SpriteRenderer>().sprite = suitSprite;
    }
    #region 辅助方法
    // 从卡槽获取卡牌数据
    private CardData GetCardData(CardDropZone pool)
    {    
        if (pool.currentCard == null || pool.currentCard.gameObject == null)
        {
            // 尝试从子对象查找卡牌
            var cardInPool = pool.GetComponentInChildren<CardDragHandler>();
            if (cardInPool != null)
            {
                pool.currentCard = cardInPool;
            }
            else
            {
                Debug.LogError($"卡槽 {pool.name} 中未找到有效卡牌引用");
                return new CardData();
            }
        }
    
        return new CardData
        {
            suit = GetSuitFromCard(pool.currentCard),
            rank = GetRankFromCard(pool.currentCard),
            cardObject = pool.currentCard
        };
    }

    // 从卡牌对象获取点数
    private Rank GetRankFromCard(CardDragHandler card)
    {
        Transform front = card.transform.Find("Front");
        if (front != null)
        {
            Transform typeObj = front.Find("Type");
            if (typeObj != null)
            {
                SpriteRenderer spriteRenderer = typeObj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    string rankStr = spriteRenderer.sprite.name;
                    return ParseRank(rankStr);
                }
            }
        }
        return Rank.Two; // 默认值
    }

    // 从卡牌对象获取花色
    private Suit GetSuitFromCard(CardDragHandler card)
    {
        Transform front = card.transform.Find("Front");
        if (front != null)
        {
            Transform suit1Obj = front.Find("Suit1");
            if (suit1Obj != null)
            {
                SpriteRenderer spriteRenderer = suit1Obj.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    string suitStr = spriteRenderer.sprite.name;
                    return ParseSuit(suitStr);
                }
            }
        }
        return Suit.Diamonds; // 默认值
    }

    // 解析点数（兼容数字和英文）
    private Rank ParseRank(string rankStr)
    {
        string lowerStr = rankStr.ToLower();

        if (int.TryParse(lowerStr, out int rankValue))
        {
            return (Rank)rankValue; // 直接转换数字
        }

        switch (lowerStr)
        {
            case "a": case "ace": return Rank.Ace;
            case "k": case "king": return Rank.King;
            case "q": case "queen": return Rank.Queen;
            case "j": case "jack": return Rank.Jack;
            // 处理数字别名
            case "11": return Rank.Jack;
            case "12": return Rank.Queen;
            case "13": return Rank.King;
            case "1": return Rank.Ace;
            default:
                Debug.LogWarning($"无法识别的点数: {rankStr}");
                return Rank.Two;
        }
    }

    // 解析花色（兼容大小写和别名）
    private Suit ParseSuit(string suitStr)
    {
        string lowerStr = suitStr.ToLower();

        switch (lowerStr)
        {
            case "diamonds": case "diamond": return Suit.Diamonds;
            case "flowers": case "clubs": case "club": return Suit.Flowers;
            case "hearts": case "heart": return Suit.Hearts;
            case "spades": case "spade": return Suit.Spades;
            default:
                Debug.LogWarning($"无法识别的花色: {suitStr}");
                return Suit.Diamonds;
        }
    }
    #endregion
}
