using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;  
public class CardComparisonManager : MonoBehaviour
{
    [Header("UI设置")]
    public Button openCardButton;
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject tiePanel;
    public Text[] groupScoreTexts = new Text[4];  // 4组分数显示

    [Header("卡槽设置")]
    public CardDropZone[] enemyPools = new CardDropZone[7];
    public CardDropZone[] playerPools = new CardDropZone[7];

    [Header("分数规则")]
    public int[] winScores = { 1, 1, 2, 3 };
    public int[] loseScores = { -1, -1, -2, -3 };
    public int[] tieScores = { 0, 0, 0, 0 };

    public int maxScore = 100;

    // 卡牌花色枚举
    public enum Suit { Diamonds, Flowers, Hearts, Spades }

    // 卡牌点数枚举
    public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    // 牌型枚举
    public enum HandType { HighCard, Pair, Straight, Flush, StraightFlush, ThreeOfAKind }

    // 卡牌信息结构
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
    public enum MatchResult { Victory, Defeat, Tie }
    public MatchResult matchResult;
    public BettingManager bettingManager;
    public PlayerBettingSystem playerBettingSystem;
    public VfxController vfxController;
    public TooltipManager tooltipManager;
    public CoinFlyToWallet coinFlyToWallet1;
    public CoinFlyToWallet coinFlyToWallet2;
    public CoinFlyToWallet coinFlyToWallet3;
    public CoinFlyToWallet coinFlyToWallet4;

    public bool IsJoker = false;

    // void Start()
    // {
    //     openCardButton.onClick.AddListener(StartComparison);
    // }

    // 开始比牌
    public void StartComparison()
    {
        StartCoroutine(CompareAllHandsCoroutine());
    }

    IEnumerator CompareAllHandsCoroutine()
    {
        if (IsJoker)
        {
            // 计数初始化
            tooltipManager.diamonds_count = 0;
            tooltipManager.hearts_count = 0;

            Debug.Log("开始计算卡牌的红心和钻石数量");

            // 计算 playerPools 和 enemyPools 中的红心和钻石数量
            foreach (CardDropZone pool in playerPools)
            {
                if (pool.currentCard != null)
                {
                    CardData cardData = GetCardData(pool);
                    Debug.Log($"玩家池卡槽 {pool.name} 中的卡牌：{cardData.rank} of {cardData.suit}");

                    if (cardData.suit == Suit.Diamonds)
                    {
                        tooltipManager.diamonds_count++;
                        Debug.Log("钻石计数增加");
                    }
                    else if (cardData.suit == Suit.Hearts)
                    {
                        tooltipManager.hearts_count++;
                        Debug.Log("红心计数增加");
                    }
                }
            }

            foreach (CardDropZone pool in enemyPools)
            {
                if (pool.currentCard != null)
                {
                    CardData cardData = GetCardData(pool);
                    Debug.Log($"敌人池卡槽 {pool.name} 中的卡牌：{cardData.rank} of {cardData.suit}");

                    if (cardData.suit == Suit.Diamonds)
                    {
                        tooltipManager.diamonds_count++;
                        Debug.Log("钻石计数增加");
                    }
                    else if (cardData.suit == Suit.Hearts)
                    {
                        tooltipManager.hearts_count++;
                        Debug.Log("红心计数增加");
                    }
                }
            }

            // 输出最终的计数
            Debug.Log($"计算完成: 红心数量 = {tooltipManager.hearts_count}, 钻石数量 = {tooltipManager.diamonds_count}");

            // 更新UI文本
            tooltipManager.heartsCountText.text = tooltipManager.hearts_count.ToString();
            tooltipManager.diamondsCountText.text = tooltipManager.diamonds_count.ToString();
        }

        // yield return new WaitForSeconds(5f);
        int totalScore = 0;

        // 第一组比较 (Pool0)
        CardData playerCard1 = GetCardData(playerPools[0]);
        CardData enemyCard1 = GetCardData(enemyPools[0]);
        int result1 = CompareGroup1(playerCard1, enemyCard1);
        int score1 = GetScoreForResult(result1, 0);
        totalScore += score1;
        UpdateScoreDisplay(0, score1);
        yield return new WaitForSeconds(0.5f);

        // 第二组比较 (Pool1)
        CardData playerCard2 = GetCardData(playerPools[1]);
        CardData enemyCard2 = GetCardData(enemyPools[1]);
        int result2 = CompareGroup2(playerCard2, enemyCard2);
        int score2 = GetScoreForResult(result2, 1);
        totalScore += score2;
        UpdateScoreDisplay(1, score2);
        yield return new WaitForSeconds(0.5f);

        // 第三组比较 (Pool2-3)
        List<CardData> playerGroup3 = new List<CardData>
        {
            GetCardData(playerPools[2]),
            GetCardData(playerPools[3])
        };

        List<CardData> enemyGroup3 = new List<CardData>
        {
            GetCardData(enemyPools[2]),
            GetCardData(enemyPools[3])
        };

        int result3 = CompareGroup3(playerGroup3, enemyGroup3);
        int score3 = GetScoreForResult(result3, 2);
        totalScore += score3;
        UpdateScoreDisplay(2, score3);
        yield return new WaitForSeconds(0.5f);

        // 第四组比较 (Pool4-6)
        List<CardData> playerGroup4 = new List<CardData>
        {
            GetCardData(playerPools[4]),
            GetCardData(playerPools[5]),
            GetCardData(playerPools[6])
        };

        List<CardData> enemyGroup4 = new List<CardData>
        {
            GetCardData(enemyPools[4]),
            GetCardData(enemyPools[5]),
            GetCardData(enemyPools[6])
        };

        int result4 = CompareGroup4(playerGroup4, enemyGroup4);
        int score4 = GetScoreForResult(result4, 3);
        totalScore += score4;
        UpdateScoreDisplay(3, score4);
        yield return new WaitForSeconds(0.5f);


        if (totalScore > 0)
        {
            matchResult = MatchResult.Victory;
        }
        else if (totalScore < 0)
        {
            matchResult = MatchResult.Defeat;
        }
        else
        {
            matchResult = MatchResult.Tie;
        }
        // 在比牌结束后调用 BettingManager 的 OpenCards 方法
        // bettingManager.OpenCards();
        StartCoroutine(bettingManager.OpenCardsCoroutine());

        if (totalScore > 0)
        {
            winPanel.SetActive(true);
            RectTransform rt = winPanel.GetComponent<RectTransform>();
            rt.SetAsLastSibling();

            StartCoroutine(HidePanelAfterDelay(winPanel, 3f));

            //特效
            coinFlyToWallet1.StartCoinFlyEffect();
            coinFlyToWallet2.StartCoinFlyEffect();
            coinFlyToWallet3.StartCoinFlyEffect();
            coinFlyToWallet4.StartCoinFlyEffect();

            vfxController.PlayBgFx();
            StartCoroutine(HideEffectAfterDelay(3f));
        }
        else if (totalScore < 0)
        {
            losePanel.SetActive(true);
            RectTransform rt = losePanel.GetComponent<RectTransform>();
            rt.SetAsLastSibling();

            StartCoroutine(HidePanelAfterDelay(losePanel, 3f));
        }
        else
        {
            tiePanel.SetActive(true);
            RectTransform rt = tiePanel.GetComponent<RectTransform>();
            rt.SetAsLastSibling();

            StartCoroutine(HidePanelAfterDelay(tiePanel, 3f));
        }
        yield break;
    }
    public void CloseAllPanelsAndEffects()
    {
        // 关闭所有面板
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        tiePanel.SetActive(false);

        // 停止特效
        vfxController.StopBgFx();
        coinFlyToWallet1.StopCoinFlyEffect();
        coinFlyToWallet2.StopCoinFlyEffect();
        coinFlyToWallet3.StopCoinFlyEffect();
        coinFlyToWallet4.StopCoinFlyEffect();
    }

    // 协程：等待指定时间后隐藏面板
    IEnumerator HidePanelAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
    }

    // 协程：等待指定时间
    IEnumerator HideEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        vfxController.StopBgFx();
        coinFlyToWallet1.StopCoinFlyEffect();
        coinFlyToWallet2.StopCoinFlyEffect();
        coinFlyToWallet3.StopCoinFlyEffect();
        coinFlyToWallet4.StopCoinFlyEffect();
    }

    // 更新分数显示
    private void UpdateScoreDisplay(int groupIndex, int score)
    {
        if (groupScoreTexts[groupIndex] != null)
        {
            groupScoreTexts[groupIndex].text = score > 0 ? $"+{score}" : score.ToString();
            groupScoreTexts[groupIndex].gameObject.SetActive(true);
        }
    }

    // 根据比较结果获取分数
    private int GetScoreForResult(int result, int groupIndex)
    {
        if (result == 1) return winScores[groupIndex]; // 玩家赢
        if (result == 2) return loseScores[groupIndex]; // 敌人赢
        return tieScores[groupIndex]; // 平局
    }

    #region 比牌逻辑
    // 第一组比较 (单张牌，大牌胜)
    public int CompareGroup1(CardData playerCard, CardData enemyCard)
    {
        return CompareSingleCard(playerCard, enemyCard);
    }

    // 第二组比较 (单张牌，小牌胜)
    public int CompareGroup2(CardData playerCard, CardData enemyCard)
    {
        int result = CompareSingleCard(playerCard, enemyCard);
        return result == 1 ? 2 : (result == 2 ? 1 : 0);
    }

    // 比较两张单牌
    private int CompareSingleCard(CardData card1, CardData card2)
    {
        // 先比较点数
        int rankResult = CompareRank(card1.rank, card2.rank);
        if (rankResult != 0) return rankResult;
        else return CompareSuit(card1.suit, card2.suit);
    }

    // 比较点数
    private int CompareRank(Rank rank1, Rank rank2)
    {
        if ((int)rank1 > (int)rank2) return 1;
        if ((int)rank1 < (int)rank2) return 2;
        return 0;
    }

    // 比较花色 (黑桃>红心>梅花>方块)
    private int CompareSuit(Suit suit1, Suit suit2)
    {
        if ((int)suit1 > (int)suit2) return 1;
        if ((int)suit1 < (int)suit2) return 2;
        return 0;
    }

    // 第三组比较 
    // 18点，超过18点算作0，小于等于18点的越接近于18点胜
    // J、Q、K的点数固定为10，A的点数可以灵活取1或11（以最接近18为准）
    public int CompareGroup3(List<CardData> playerCards, List<CardData> enemyCards)
    {
        if (playerCards.Count != 2 || enemyCards.Count != 2) return 0;

        List<CardData> playerSortedCards = SortCards(playerCards);
        List<CardData> enemySortedCards = SortCards(enemyCards);
        
        // 计算玩家最优两张牌组合
        int playerSum = CalculateBestSum(playerSortedCards);
        
        // 计算对手最优两张牌组合
        int enemySum = CalculateBestSum(enemySortedCards);

        if (playerSum > 18) {playerSum = 0;}
        if (enemySum > 18) {enemySum = 0;}

        if (playerSum > enemySum) return 1;
        else if (playerSum < enemySum) return 2;
        else return CompareHighCardGroup3(playerSortedCards, enemySortedCards);
    }

    // 计算两张牌的最优和（考虑A的灵活取值）
    private int CalculateBestSum(List<CardData> cards)
    {
        // 获取前两张牌（已排序）
        CardData card1 = cards[0];
        CardData card2 = cards[1];
        
        // 处理可能的A牌（需要考虑1或11的情况）
        bool card1IsA = (int)card1.rank == 14; // A的rank是14
        bool card2IsA = (int)card2.rank == 14;
        
        // 情况1：没有A牌
        if (!card1IsA && !card2IsA)
        {
            // J、Q、K固定为10
            int rank1 = (int)card1.rank;
            int rank2 = (int)card2.rank;
            
            if (rank1 >= 11 && rank1 <= 13) rank1 = 10; // J,Q,K
            if (rank2 >= 11 && rank2 <= 13) rank2 = 10; // J,Q,K
            
            return rank1 + rank2;
        }
        // 情况2：只有一张A牌
        else if (card1IsA || card2IsA)
        {
            // 确定哪张是A
            CardData aCard = card1IsA ? card1 : card2;
            CardData otherCard = card1IsA ? card2 : card1;
            
            // 其他牌的点数（J,Q,K固定为10）
            int otherRank = (int)otherCard.rank;
            if (otherRank >= 11 && otherRank <= 13) otherRank = 10;
            
            // A可以取1或11
            // 我们需要选择使得总和最接近18但不超18的值
            int sumWithA11 = otherRank + 11;
            int sumWithA1 = otherRank + 1;
            
            // 选择不超过18的最大可能值
            if (sumWithA11 <= 18) 
            {
                return sumWithA11; 
            }
            else 
            {
                return sumWithA1; 
            }
        }
        // 情况3：两张都是A牌
        else
        {
            return 12; 
        }
    }


    // 第四组比较 (三张牌牌型)
    public int CompareGroup4(List<CardData> playerCards, List<CardData> enemyCards)
    {
        if (playerCards.Count < 3 || enemyCards.Count < 3) return 0;

        List<CardData> playerSortedCards = SortCards(playerCards);
        List<CardData> enemySortedCards = SortCards(enemyCards);
        
        HandType playerType = EvaluateHand(playerSortedCards);
        HandType enemyType = EvaluateHand(enemySortedCards);
        
        if (playerType > enemyType) return 1;
        else if (playerType < enemyType) return 2;
        else return CompareSameHandType(playerSortedCards, enemySortedCards, playerType);
    }

    // 评估三张卡牌的牌型
    private HandType EvaluateHand(List<CardData> cards)
    {
        Debug.Log($"Card1: {cards[0].suit}{cards[0].rank} Card2: {cards[1].suit}{cards[1].rank} Card3: {cards[2].suit}{cards[2].rank}");
        
        if (IsThreeOfAKind(cards)) return HandType.ThreeOfAKind;
        else if (IsStraightFlush(cards)) return HandType.StraightFlush;
        else if (IsFlush(cards)) return HandType.Flush;
        else if (IsStraight(cards)) return HandType.Straight;
        else if (IsPair(cards)) return HandType.Pair;
        else return HandType.HighCard;
    }

    // 检查是否为顺子
    private bool IsStraight(List<CardData> cards)
    {
        int rankDiff1 = (int)cards[1].rank - (int)cards[0].rank;
        int rankDiff2 = (int)cards[2].rank - (int)cards[1].rank;
        return rankDiff1 == 1 && rankDiff2 == 1;
    }

    // 检查是否为同花
    private bool IsFlush(List<CardData> cards)
    {
        return cards[0].suit == cards[1].suit && cards[1].suit == cards[2].suit;
    }

    // 检查是否为三条
    private bool IsThreeOfAKind(List<CardData> cards)
    {
        return cards[0].rank == cards[1].rank && cards[1].rank == cards[2].rank;
    }

    // 检查是否为对子
    private bool IsPair(List<CardData> cards)
    {
        return cards[0].rank == cards[1].rank || cards[1].rank == cards[2].rank || cards[0].rank == cards[2].rank;
    }

    // 检查是否为同花顺
    private bool IsStraightFlush(List<CardData> cards)
    {
        return IsStraight(cards) && IsFlush(cards);
    }

    // 检查是否为高牌
    private bool IsHighCard(List<CardData> cards)
    {
        return true;
    }

    private int CompareSameHandType(List<CardData> playerCards, List<CardData> enemyCards, HandType handType)
    {
        switch (handType)
        {
            case HandType.ThreeOfAKind:
                Debug.Log("Both ThreeOfAKind");
                return CompareHighCardGroup4(playerCards, enemyCards);
            case HandType.StraightFlush:
                Debug.Log("Both StraightFlush");
                return CompareHighCardGroup4(playerCards, enemyCards);
            case HandType.Flush:
                Debug.Log("Both Flush");
                return CompareHighCardGroup4(playerCards, enemyCards);
            case HandType.Straight:
                Debug.Log("Both Straight");
                return CompareHighCardGroup4(playerCards, enemyCards);
            case HandType.Pair:
                Debug.Log("Both Pair");
                return ComparePair(playerCards, enemyCards);
            case HandType.HighCard:
                Debug.Log("Both HighCard");
                return CompareHighCardGroup4(playerCards, enemyCards);
            default:
                return 0;
        }
    }

    private int CompareHighCardGroup4(List<CardData> playerCards, List<CardData> enemyCards)
    {
        Debug.Log($"CompareHighCard: {playerCards[2].suit}{playerCards[2].rank} {enemyCards[2].suit}{enemyCards[2].rank}");
        return CompareSingleCard(playerCards[2], enemyCards[2]);
    }

    private int CompareHighCardGroup3(List<CardData> playerCards, List<CardData> enemyCards)
    {
        Debug.Log($"CompareHighCard: {playerCards[1].suit}{playerCards[1].rank} {enemyCards[1].suit}{enemyCards[1].rank}");
        return CompareSingleCard(playerCards[1], enemyCards[1]);
    }

    private int ComparePair(List<CardData> playerCards, List<CardData> enemyCards)
    {
        Rank playerPairRank = FindPairRank(playerCards);
        CardData playerRemainCard = FindRemainCard(playerCards, playerPairRank);
        Rank enemyPairRank = FindPairRank(enemyCards);
        CardData enemyRemainCard = FindRemainCard(enemyCards, enemyPairRank);

        if (playerPairRank > enemyPairRank) return 1;
        else if (playerPairRank < enemyPairRank) return 2;
        else return CompareSingleCard(playerRemainCard, enemyRemainCard);
    }

    private Rank FindPairRank(List<CardData> cards)
    {
        if (cards[0].rank == cards[1].rank) return cards[0].rank;
        else if (cards[1].rank == cards[2].rank) return cards[1].rank;
        else return cards[0].rank;
    }

    private CardData FindRemainCard(List<CardData> cards, Rank pairRank)
    {
        CardData remainCard = new CardData();
        foreach (CardData card in cards)
        {
            if (card.rank != pairRank)
            {
                remainCard = card;
                break;
            }
        }
        return remainCard;
    }

    private List<CardData> SortCards(List<CardData> cards)
    {
        return cards.OrderBy(c => (int)c.rank).ThenBy(c => (int)c.suit).ToList();
    }
    #endregion

    #region 辅助方法
    // 从卡槽获取卡牌数据
    private CardData GetCardData(CardDropZone pool)
    {
        // if (pool.currentCard == null)
        // {
        //     Debug.LogWarning($"卡槽 {pool.name} 中没有卡牌");
        //     return new CardData();
        // }
        
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
