using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class EnemyCardManager : MonoBehaviour
{
    [Header("Card Setup")]
    public CardDropZone[] enemyPiles = new CardDropZone[7];  // 7个敌人卡堆
    public CardDropZone[] enemyPools = new CardDropZone[7];  // 7个敌人卡槽

    [Header("Animation Settings")]
    public float moveDuration = 0.5f;
    public float delayBetweenCards = 0.2f;
    public bool isJoker = false;

    [Header("Manager")]
    public BettingManager bettingManager;
    public CardJHandler cardJHandler;

    // 卡牌花色枚举
    public enum Suit { Diamonds, Flowers, Hearts, Spades }

    // 卡牌点数枚举（Ace=14, King=13, Queen=12, Jack=11）
    public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    // 牌型枚举
    public enum HandType { HighCard, Pair, Straight, Flush, ThreeOfAKind, StraightFlush }

    // 总得分
    private double totalScore = 0;
    private List<List<CardInfo>> optimizedGroups = new List<List<CardInfo>>();

    // 卡牌数据结构
    private class CardInfo
    {
        public CardDragHandler card;
        public Rank rank;
        public Suit suit;
        public bool isJAsAce = false;

        public override string ToString()
        {
            return $"{rank} of {suit}";
        }

        // 实际牌值计算（J牌作为A时返回14）
        public Rank ActualRank 
        {
            get 
            {
                if (rank == Rank.Jack && isJAsAce)
                    return Rank.Ace; // A的牌值
                return rank;
            }
        }

        public CardInfo Clone()
        {
            return new CardInfo { card = card, rank = rank, suit = suit, isJAsAce = isJAsAce };
        }
    }

    public void DistributeEnemyCards()
    {
        if (enemyPiles.Length == 0 || enemyPools.Length == 0)
        {
            Debug.LogError("Card zones not set up!");
            return;
        }
        StartCoroutine(DistributeEnemyCardsCoroutine());
    }

    IEnumerator DistributeEnemyCardsCoroutine()
    {
        yield return new WaitForSeconds(1f); // 等待1秒
        
        // 收集所有敌人卡牌信息
        List<CardInfo> cardInfos = new List<CardInfo>();
        for (int i = 0; i < enemyPiles.Length; i++)
        {
            if (enemyPiles[i].currentCard != null)
            {
                CardDragHandler card = enemyPiles[i].currentCard;
                Rank rank = GetRankFromCard(card);
                Suit suit = GetSuitFromCard(card);
                cardInfos.Add(new CardInfo { card = card, rank = rank, suit = suit });
                Debug.Log($"收集卡牌 {i}: {rank} of {suit}");
            }
        }
        
        if (isJoker)
        {
            optimizedGroups = OptimizeCardGroupsStage1(cardInfos);
        }
        else
        {
            // 优化分组
            optimizedGroups = (bettingManager.currentRound < bettingManager.maxRounds) 
                ? OptimizeCardGroupsStage1(cardInfos) 
                : OptimizeCardGroupsStage2(cardInfos);
        }
        
        // 按分组顺序排列卡牌
        List<CardDragHandler> orderedCards = new List<CardDragHandler>();
        orderedCards.Add(optimizedGroups[0][0].card);
        orderedCards.Add(optimizedGroups[1][0].card);
        orderedCards.Add(optimizedGroups[2][0].card);
        orderedCards.Add(optimizedGroups[2][1].card);
        orderedCards.Add(optimizedGroups[3][0].card);
        orderedCards.Add(optimizedGroups[3][1].card);
        orderedCards.Add(optimizedGroups[3][2].card);

        // 创建卡牌信息映射表
        Dictionary<CardDragHandler, CardInfo> cardInfoMap = new Dictionary<CardDragHandler, CardInfo>();
        foreach (var group in optimizedGroups)
        {
            foreach (var cardInfo in group)
            {
                cardInfoMap[cardInfo.card] = cardInfo;
            }
        }
        
        // 分发卡牌
        for (int i = 0; i < orderedCards.Count; i++)
        {
            CardDragHandler card = orderedCards[i];
            CardDropZone targetPool = enemyPools[i];
            
            // 获取卡牌信息
            CardInfo cardInfo = cardInfoMap[card];
            
            // 移动卡牌
            Vector3 startPosition = card.transform.position;
            card.transform.DOMove(targetPool.transform.position, moveDuration)
                .SetEase(Ease.OutCubic)
                .OnStart(() => card.canvasGroup.alpha = 0.8f)
                .OnComplete(() => {
                    card.canvasGroup.alpha = 1f;
                    targetPool.AcceptCard(card, card.originalDropZone);
                    
                    // 检查是否为J牌作为A使用
                    if (cardInfo.isJAsAce)
                    {
                        // 动态添加CardJHandler组件
                        if (card.GetComponent<CardJHandler>() == null)
                        {
                            CardJHandler handler = card.gameObject.AddComponent<CardJHandler>();
                            Debug.Log($"为卡牌 {cardInfo} 添加了CardJHandler组件");
                        }
                    }
                });
            
            yield return new WaitForSeconds(delayBetweenCards);
        }
        
        bettingManager.StartMonsterBettingRound();
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
        Debug.Log($"解析花色: 原始名称={suitStr}");

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

    public double GetBestScore()
    {
        return totalScore;
    }

    // 优化卡牌分组
    private List<List<CardInfo>> OptimizeCardGroupsStage1(List<CardInfo> sevenCards)
    {
        if (sevenCards.Count != 7)
            throw new ArgumentException("必须提供7张卡牌");

        List<List<CardInfo>> bestGroups = null;
        double bestScore = double.MinValue;
        int evaluationCount = 0;

        // 生成所有可能的Group1选择
        foreach (var group1Card in sevenCards)
        {
            var group1 = new List<CardInfo> { group1Card };
            var remaining1 = sevenCards.Except(group1).ToList();

            // 生成所有可能的Group2选择
            foreach (var group2Card in remaining1)
            {
                var group2 = new List<CardInfo> { group2Card };
                var remaining2 = remaining1.Except(group2).ToList();

                // 生成所有可能的Group3组合
                for (int i = 0; i < remaining2.Count; i++)
                {
                    for (int j = i + 1; j < remaining2.Count; j++)
                    {
                        var group3 = new List<CardInfo> { remaining2[i], remaining2[j] };
                        var group4 = remaining2.Except(group3).ToList();

                        // 构建分组列表
                        var groups = new List<List<CardInfo>> { group1, group2, group3, group4 };

                        // 评估分组得分
                        double score = EvaluateGroupScoreStage1(groups);
                        evaluationCount++;

                        // 更新最佳分组
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestGroups = groups;

                            Debug.Log("EnemyCardManager bestScore: " + bestScore);
                        }
                    }
                }
            }
        }
        Debug.Log("EnemyCardManager bestScore: " + bestScore);

        totalScore = bestScore;
        return bestGroups;
    }

    // 评估卡牌分组得分
    private double EvaluateGroupScoreStage1(List<List<CardInfo>> groups)
    {
        double score = 0;

        // Group1评分（大牌胜）
        var group1Card = groups[0][0];
        double group1Score = (int)group1Card.rank * 0.5 + (int)group1Card.suit * 0.1;
        score += group1Score;

        // Group2评分（小牌胜）
        var group2Card = groups[1][0];
        double group2Score = (14 - (int)group2Card.rank) * 0.5 + (3 - (int)group2Card.suit) * 0.1;
        score += group2Score;

        // Group3评分（考虑高牌）
        var group3Cards = groups[2].OrderByDescending(c => (int)c.rank).ThenByDescending(c => (int)c.suit).ToList(); // 降序排序
        int sumGroup3 = CalculateBestSumForTwoCardsStage1(group3Cards[0], group3Cards[1]);
        if (sumGroup3 > 18) sumGroup3 = 0;
        
        // 高牌加成（点数相同时比较最高牌）
        double group3HighCardBonus = (int)group3Cards[0].rank * 0.01; // 最高牌的小额加成
        double group3Score = sumGroup3 * 0.7 + group3HighCardBonus;
        score += group3Score;

        // Group4评分（牌型+高牌）
        var group4Cards = groups[3].OrderByDescending(c => (int)c.rank).ThenByDescending(c => (int)c.suit).ToList();
        HandType handType = EvaluateHandStage1(group4Cards);
        double group4BaseScore = (int)handType * 2.0;
        
        // 高牌比较
        double group4HighCardScore = (int)group4Cards[0].rank * 0.01;
        
        // 额外奖励特殊组合
        double bonus = handType switch
        {
            HandType.ThreeOfAKind => 5.0,
            HandType.StraightFlush => 4.0,
            HandType.Flush => 3.0,
            HandType.Straight => 2.0,
            HandType.Pair => 1.0,
            _ => 0
        };
        
        score += group4BaseScore + group4HighCardScore + bonus;

        // 最终得分四舍五入到3位小数（避免浮点误差）
        return Math.Round(score, 3);
    }

    // 评估三张卡牌的牌型
    private HandType EvaluateHandStage1(List<CardInfo> threeCards)
    {
        // 按点数排序
        threeCards = threeCards.OrderBy(card => (int)card.rank).ThenBy(card => (int)card.suit).ToList();

        bool isFlush = threeCards[0].suit == threeCards[1].suit &&
                       threeCards[1].suit == threeCards[2].suit;

        bool isStraight = false;
        int rankDiff1 = (int)threeCards[1].rank - (int)threeCards[0].rank;
        int rankDiff2 = (int)threeCards[2].rank - (int)threeCards[1].rank;

        // 检查三条
        if (threeCards[0].rank == threeCards[1].rank && threeCards[1].rank == threeCards[2].rank)
            return HandType.ThreeOfAKind;

        if (isFlush && isStraight)
            return HandType.StraightFlush;

        if (isFlush)
            return HandType.Flush;

        if (isStraight)
            return HandType.Straight;

        // 检查对子
        if (threeCards[0].rank == threeCards[1].rank ||
            threeCards[1].rank == threeCards[2].rank)
            return HandType.Pair;

        return HandType.HighCard;
    }

    // 计算两张牌的最优和（考虑A的灵活取值）
    private int CalculateBestSumForTwoCardsStage1(CardInfo card1, CardInfo card2)
    {
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
            CardInfo aCard = card1IsA ? card1 : card2;
            CardInfo otherCard = card1IsA ? card2 : card1;
            
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
                return sumWithA11; // 优先取11
            }
            else 
            {
                return sumWithA1; // 只能取1
            }
        }
        // 情况3：两张都是A牌
        else
        {
            return 12; 
        }
    }

    // 优化卡牌分组
    private List<List<CardInfo>> OptimizeCardGroupsStage2(List<CardInfo> sevenCards)
    {
        if (sevenCards.Count != 7)
            throw new ArgumentException("必须提供7张卡牌");

        List<List<CardInfo>> bestGroups = null;
        double bestScore = double.MinValue;

        // 1. 识别所有J牌
        var jokers = sevenCards.Where(c => c.rank == Rank.Jack).ToList();
        int jokerCount = jokers.Count;
        int identityCombinations = 1 << jokerCount; // 2^jokerCount种身份组合

         // 2. 遍历所有J牌身份组合
        for (int comb = 0; comb < identityCombinations; comb++)
        {
            // 克隆卡牌（避免污染原始数据）
            List<CardInfo> clonedCards = sevenCards.Select(c => c.Clone()).ToList();
            var clonedJokers = clonedCards.Where(c => c.rank == Rank.Jack).ToList();

            // 设置每张J牌的身份
            for (int j = 0; j < jokerCount; j++)
            {
                clonedJokers[j].isJAsAce = ((comb >> j) & 1) == 1; // 位运算设置身份
            }

            // 3. 遍历所有分组组合（使用克隆后的卡牌）
            foreach (var group1Card in clonedCards)
            {
                var group1 = new List<CardInfo> { group1Card };
                var remaining1 = clonedCards.Except(group1).ToList();

                foreach (var group2Card in remaining1)
                {
                    var group2 = new List<CardInfo> { group2Card };
                    var remaining2 = remaining1.Except(group2).ToList();

                    for (int i = 0; i < remaining2.Count; i++)
                    {
                        for (int j = i + 1; j < remaining2.Count; j++)
                        {
                            var group3 = new List<CardInfo> { remaining2[i], remaining2[j] };
                            var group4 = remaining2.Except(group3).ToList();

                            var groups = new List<List<CardInfo>> { group1, group2, group3, group4 };
                            double score = EvaluateGroupScoreStage2(groups);

                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestGroups = groups;
                            }
                        }
                    }
                }
            }
        }
        Debug.Log("EnemyCardManager bestScore: " + bestScore);

        totalScore = bestScore;
        return bestGroups;
    }

    // 评估卡牌分组得分
    private double EvaluateGroupScoreStage2(List<List<CardInfo>> groups)
    {
        double score = 0;

        // Group1评分（大牌胜 + 高牌加成）
        var group1Card = groups[0][0];
        double group1Score = (int)group1Card.ActualRank * 0.5 + (int)group1Card.suit * 0.1;
        // 添加牌面值的小数部分作为高牌区分
        score += group1Score; 

        // Group2评分（小牌胜 + 高牌加成）
        var group2Card = groups[1][0];
        double group2Score = (14 - (int)group2Card.ActualRank) * 0.5 + (3 - (int)group2Card.suit * 0.1);
        // 添加逆向牌面值的小数部分
        score += group2Score;

        // Group3评分（带高牌比较的点和）
        var group3Cards = groups[2].OrderByDescending(c => (int)c.ActualRank).ThenByDescending(c => (int)c.suit).ToList();
        int sumGroup3 = CalculateBestSumForTwoCardsStage2(group3Cards[0], group3Cards[1]);
        if (sumGroup3 > 18) sumGroup3 = 0;
        
        // 高牌加成系统（确保点数相同时能比较）
        double group3HighCardBonus = (int)group3Cards[0].ActualRank * 0.01; // 最高牌
        
        score += sumGroup3 * 0.7 + group3HighCardBonus;

        // Group4评分（牌型+层级式高牌比较）
        var group4Cards = groups[3].OrderByDescending(c => (int)c.ActualRank).ThenByDescending(c => (int)c.suit).ToList();
        HandType handType = EvaluateHandStage2(group4Cards);
        double group4BaseScore = (int)handType * 2.0;
        
        // 层级式高牌比较系统
        double group4HighCardScore = (int)group4Cards[0].ActualRank * 0.01; // 最高牌

        // 牌型奖励（使用switch表达式简化）
        double bonus = handType switch
        {
            HandType.ThreeOfAKind => 5.0,
            HandType.StraightFlush => 4.0,
            HandType.Flush => 3.0,
            HandType.Straight => 2.0,
            HandType.Pair => 1.0,
            _ => 0
        };
        
        score += group4BaseScore + group4HighCardScore + bonus;

        // 最终得分四舍五入到3位小数（避免浮点误差）
        return Math.Round(score, 3);
    }

    // 评估三张卡牌的牌型
    private HandType EvaluateHandStage2(List<CardInfo> threeCards)
    {
        // 按点数排序
        threeCards = threeCards.OrderBy(card => (int)card.ActualRank).ThenBy(card => (int)card.suit).ToList();

        bool isFlush = threeCards[0].suit == threeCards[1].suit &&
                       threeCards[1].suit == threeCards[2].suit;

        bool isStraight = false;
        int rankDiff1 = (int)threeCards[1].ActualRank - (int)threeCards[0].ActualRank;
        int rankDiff2 = (int)threeCards[2].ActualRank - (int)threeCards[1].ActualRank;

        // 检查三条
        if (threeCards[0].ActualRank == threeCards[1].ActualRank && threeCards[1].ActualRank == threeCards[2].ActualRank)
            return HandType.ThreeOfAKind;

        if (isFlush && isStraight)
            return HandType.StraightFlush;

        if (isFlush)
            return HandType.Flush;

        if (isStraight)
            return HandType.Straight;

        // 检查对子
        if (threeCards[0].ActualRank == threeCards[1].ActualRank ||
            threeCards[1].ActualRank == threeCards[2].ActualRank)
            return HandType.Pair;

        return HandType.HighCard;
    }

    // 计算两张牌的最优和（考虑A的灵活取值）
    private int CalculateBestSumForTwoCardsStage2(CardInfo card1, CardInfo card2)
    {
        // 处理可能的A牌（需要考虑1或11的情况）
        bool card1IsA = (int)card1.ActualRank == 14; 
        bool card2IsA = (int)card2.ActualRank == 14;
        
        // 情况1：没有A牌
        if (!card1IsA && !card2IsA)
        {
            // J、Q、K固定为10
            int rank1 = (int)card1.ActualRank;
            int rank2 = (int)card2.ActualRank;
            
            if (rank1 >= 11 && rank1 <= 13) rank1 = 10; // J,Q,K
            if (rank2 >= 11 && rank2 <= 13) rank2 = 10; // J,Q,K
            
            return rank1 + rank2;
        }
        // 情况2：只有一张A牌
        else if (card1IsA || card2IsA)
        {
            // 确定哪张是A
            CardInfo aCard = card1IsA ? card1 : card2;
            CardInfo otherCard = card1IsA ? card2 : card1;
            
            // 其他牌的点数（J,Q,K固定为10）
            int otherRank = (int)otherCard.ActualRank;
            if (otherRank >= 11 && otherRank <= 13) otherRank = 10;
            
            // A可以取1或11
            // 我们需要选择使得总和最接近18但不超18的值
            int sumWithA11 = otherRank + 11;
            int sumWithA1 = otherRank + 1;
            
            // 选择不超过18的最大可能值
            if (sumWithA11 <= 18) 
            {
                return sumWithA11; // 优先取11
            }
            else 
            {
                return sumWithA1; // 只能取1
            }
        }
        // 情况3：两张都是A牌
        else
        {
            return 12; 
        }
    }
}