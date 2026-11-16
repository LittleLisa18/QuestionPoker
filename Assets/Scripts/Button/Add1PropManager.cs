using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
public class Add1PropManager : MonoBehaviour
{
    public static Add1PropManager Instance;
    [Header("UI设置")]
    public Button ConfirmPropButton;         // 回收按钮

    [Header("动画参数")]
    public float flipDuration = 0.8f;       // 翻牌动画持续时间
    public float delayBetweenFlips = 0.2f;  // 卡牌之间的翻牌延迟
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
    void Start()
    {
        if (Instance == null) Instance = this; // 单例初始化
        // 绑定按钮点击事件
        ConfirmPropButton.onClick.AddListener(RecycleSelectedCards);
        ConfirmPropButton.interactable = false; // 初始时按钮不可点击
    }

    // 更新确认按钮的状态
    public void PlayerUpdateConfirmButtonState()
    {
        if (PlayerCardSelectionHandler.selectedCardCount > 0)  // 至少选中一张卡
        {
            ConfirmPropButton.interactable = true;
        }
        else
        {
            ConfirmPropButton.interactable = false;
        }
    }

    // 回收选中的卡牌
    public void RecycleSelectedCards()
    {
        // 重置选中的卡牌数量
        PlayerCardSelectionHandler.selectedCardCount = 0;

        // 隐藏回收按钮
        ConfirmPropButton.gameObject.SetActive(false);

        // 禁用所有卡牌的选择功能
        var allCards = FindObjectsOfType<PlayerCardSelectionHandler>();
        foreach (var card in allCards)
        {
            card.enabled = false;  // 禁用选择功能
        }

        StartCoroutine(RecycleCardsCoroutine());

        // 启用所有按钮
        ButtonDisabler buttonDisabler = FindObjectOfType<ButtonDisabler>();
        if (buttonDisabler != null)
        {
            buttonDisabler.EnableAllButtons();  // 启用所有按钮
        }

        // 恢复ToolCardManager的Update方法
        ToolCardManager toolCardManager = FindObjectOfType<ToolCardManager>();
        if (toolCardManager != null)
        {
            toolCardManager.shouldUpdate = true;  // 恢复Update执行
        }
        // 恢复CardFlipController的Update方法
        CardFlipController cardFlipController = FindObjectOfType<CardFlipController>();
        if (cardFlipController != null)
        {
            cardFlipController.isPropUsed = true;  // 恢复Update执行
        }
    }

    IEnumerator RecycleCardsCoroutine()
    {
        ConfirmPropButton.interactable = false;

        // 获取所有选中的卡牌
        var selectedCards = FindObjectsOfType<PlayerCardSelectionHandler>()
            .Where(c => c.isSelected)
            .ToList();

        // 执行翻牌动画
        foreach (var card in selectedCards)
        {
            if (card != null)
            {
                // 查找卡牌的前后两面
                Transform back = card.transform.Find("Back");
                Transform front = card.transform.Find("Front");

                if (back != null && front != null)
                {
                    // 执行翻牌动画
                    StartCoroutine(FlipSingleCard(card.gameObject, back, front));
                }
            }

            // 等待延迟后再翻下一张
            yield return new WaitForSeconds(delayBetweenFlips);
        }
        var allCards = FindObjectsOfType<PlayerCardSelectionHandler>();
        foreach (var card in allCards)
        {
            card.ResetCardState(); // 新增重置方法
        }
        // 恢复按钮状态
        ConfirmPropButton.interactable = true;
    }

    // 翻动单张卡牌的动画
    IEnumerator FlipSingleCard(GameObject card, Transform back, Transform front)
    {
        PlayerCardSelectionHandler selectionHandler = card.GetComponent<PlayerCardSelectionHandler>();

        if (selectionHandler != null)
        {
            // 使用 PlayerCardSelectionHandler 中记录的原始缩放值
            card.transform.localScale = selectionHandler.GetOriginalScale();
        }
        // 初始状态：显示正面，隐藏背面（重要调整）
        back.gameObject.SetActive(false);
        front.gameObject.SetActive(true);

        Sequence flipSequence = DOTween.Sequence();

        // ===== 第一阶段：正面翻转到背面 =====
        // 0° → 90°（前半段）
        flipSequence.Append(card.transform.DORotate(new Vector3(0, 90, 0), flipDuration / 4)
            .SetEase(Ease.OutQuad));

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
            Rank rank = GetRankFromCard(card.GetComponent<CardDragHandler>());  // 获取点数
            Suit suit = GetSuitFromCard(card.GetComponent<CardDragHandler>());  // 获取花色

            // 输出当前的点数和花色
            Debug.Log($"Before Increment: {rank} of {suit}");

            // 增加rank
            string[] ranks = { "Ace", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King" };

            int currentIndex = System.Array.IndexOf(ranks, rank.ToString());
            int nextIndex = (currentIndex + 1) % ranks.Length;
            rank = (Rank)System.Enum.Parse(typeof(Rank), ranks[nextIndex]);

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