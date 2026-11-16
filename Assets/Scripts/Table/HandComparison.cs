using System;
using System.Collections.Generic;
using System.Linq;

namespace CardGame
{
    /// <summary>
    /// 手牌比较逻辑类 - 包含所有卡牌比较的核心算法
    /// </summary>
    public static class HandComparison
    {
        #region 单牌比较

        public static int CompareGroup1(List<Card> group1, List<Card> group2)
        {
            return CompareSingleCard(group1[0], group2[0]);
        }

        public static int CompareGroup2(List<Card> group1, List<Card> group2)
        { 
            if (CompareSingleCard(group1[0], group2[0]) == 1)
                return 2;
            else if (CompareSingleCard(group1[0], group2[0]) == 2)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// 比较两张单牌的大小
        /// 返回值: 1=card1胜, 2=card2胜, 0=平局
        /// </summary>
        public static int CompareSingleCard(Card card1, Card card2)
        {
            // 先比较牌面大小
            int rankResult = CompareRank(card1.rank, card2.rank);
            if (rankResult != 0)
                return rankResult;
            
            // 牌面相同则比较花色
            return CompareSuit(card1.suit, card2.suit);
        }

        /// <summary>
        /// 比较牌面大小
        /// 返回值: 1=rank1胜, 2=rank2胜, 0=平局
        /// </summary>
        public static int CompareRank(Rank rank1, Rank rank2)
        {
            if ((int)rank1 > (int)rank2)
                return 1;
            else if ((int)rank1 < (int)rank2)
                return 2;
            else
                return 0;
        }

        /// <summary>
        /// 比较花色大小 (黑桃>红心>梅花>方块)
        /// 返回值: 1=suit1胜, 2=suit2胜, 0=平局
        /// </summary>
        public static int CompareSuit(Suit suit1, Suit suit2)
        {
            if ((int)suit1 > (int)suit2)
                return 1;
            else if ((int)suit1 < (int)suit2)
                return 2;
            else
                return 0;
        }

        #endregion

        #region 第三组比较 - 两张牌点数和

        /// <summary>
        /// 计算两张牌的点数和
        /// </summary>
        public static int CalculateCardSum(Card card1, Card card2)
        {
            return (int)card1.rank + (int)card2.rank;
        }

        /// <summary>
        /// 比较第三组（两张牌点数和）
        /// </summary>
        public static int CompareGroup3(List<Card> group1, List<Card> group2)
        {
            if (group1.Count != 2 || group2.Count != 2)
            {
                Console.WriteLine("第三组比较需要每组2张牌！");
                return 0;
            }

            int sum1 = CalculateCardSum(group1[0], group1[1]);
            int sum2 = CalculateCardSum(group2[0], group2[1]);

            if (sum1 > sum2)
                return 1;
            else if (sum1 < sum2)
                return 2;
            else
                return 0;
        }

        #endregion

        #region 第四组比较 - 三张牌手牌类型

        /// <summary>
        /// 评估三张牌的手牌类型
        /// </summary>
        public static HandType EvaluateHand(List<Card> cards)
        {
            if (cards.Count != 3)
            {
                Console.WriteLine("手牌评估需要3张牌！");
                return HandType.HighCard;
            }

            if (IsThreeOfAKind(cards))
                return HandType.ThreeOfAKind;
            else if (IsStraightFlush(cards))
                return HandType.StraightFlush;
            else if (IsFlush(cards))
                return HandType.Flush;
            else if (IsStraight(cards))
                return HandType.Straight;
            else if (IsPair(cards))
                return HandType.Pair;
            else
                return HandType.HighCard;
        }

        /// <summary>
        /// 比较第四组（三张牌手牌类型）
        /// </summary>
        public static int CompareGroup4(List<Card> hand1, List<Card> hand2)
        {
            HandType type1 = EvaluateHand(hand1);
            HandType type2 = EvaluateHand(hand2);

            Console.WriteLine($"手牌类型: 手牌1={type1}, 手牌2={type2}");

            int value1 = (int)type1;
            int value2 = (int)type2;

            if (value1 > value2)
                return 1;
            else if (value1 < value2)
                return 2;
            else
            {
                // 相同类型时的细节比较
                // return CompareHandsOfSameType(hand1, hand2, type1);
                return 0;
            }
        }

        // /// <summary>
        // /// 比较相同类型的手牌
        // /// </summary>
        // private static int CompareHandsOfSameType(List<Card> hand1, List<Card> hand2, HandType handType)
        // {
        //     switch (handType)
        //     {
        //         case HandType.ThreeOfAKind:
        //             return CompareThreeOfAKind(hand1, hand2);
        //         case HandType.StraightFlush:
        //             return CompareStraightFlush(hand1, hand2);
        //         case HandType.Flush:
        //             return CompareFlush(hand1, hand2);
        //         case HandType.Straight:
        //             return CompareStraight(hand1, hand2);
        //         case HandType.Pair:
        //             return ComparePair(hand1, hand2);
        //         case HandType.HighCard:
        //             return CompareHighCard(hand1, hand2);
        //         default:
        //             return 0;
        //     }
        // }

        #endregion

        #region 手牌类型判断

        /// <summary>
        /// 检查是否为三条
        /// </summary>
        private static bool IsThreeOfAKind(List<Card> cards)
        {
            return cards[0].rank == cards[1].rank && cards[1].rank == cards[2].rank;
        }

        /// <summary>
        /// 检查是否为同花
        /// </summary>
        private static bool IsFlush(List<Card> cards)
        {
            return cards[0].suit == cards[1].suit && cards[1].suit == cards[2].suit;
        }

        /// <summary>
        /// 检查是否为顺子
        /// </summary>
        private static bool IsStraight(List<Card> cards)
        {
            var sortedCards = cards.OrderBy(c => (int)c.rank).ToList();
            
            // 检查常规顺子 (例如: 7-8-9)
            if ((int)sortedCards[1].rank == (int)sortedCards[0].rank + 1 && 
                (int)sortedCards[2].rank == (int)sortedCards[1].rank + 1)
                return true;
                
            // 检查A-2-3顺子 (A当1使用)
            if (sortedCards[0].rank == Rank.Two && 
                sortedCards[1].rank == Rank.Three && 
                sortedCards[2].rank == Rank.Ace)
                return true;
                
            // 检查Q-K-A顺子
            if (sortedCards[0].rank == Rank.Queen && 
                sortedCards[1].rank == Rank.King && 
                sortedCards[2].rank == Rank.Ace)
                return true;
                
            return false;
        }

        /// <summary>
        /// 检查是否为同花顺
        /// </summary>
        private static bool IsStraightFlush(List<Card> cards)
        {
            return IsStraight(cards) && IsFlush(cards);
        }

        /// <summary>
        /// 检查是否为对子
        /// </summary>
        private static bool IsPair(List<Card> cards)
        {
            return cards[0].rank == cards[1].rank || 
                   cards[0].rank == cards[2].rank || 
                   cards[1].rank == cards[2].rank;
        }

        #endregion

        // #region 相同类型手牌比较

        // /// <summary>
        // /// 比较三条
        // /// </summary>
        // private static int CompareThreeOfAKind(List<Card> hand1, List<Card> hand2)
        // {
        //     // 三条比较直接比较牌面大小
        //     Rank rank1 = hand1[0].rank;
        //     Rank rank2 = hand2[0].rank;
        //     return CompareRank(rank1, rank2);
        // }

        // /// <summary>
        // /// 比较同花顺
        // /// </summary>
        // private static int CompareStraightFlush(List<Card> hand1, List<Card> hand2)
        // {
        //     // 同花顺比较最高牌
        //     return CompareStraight(hand1, hand2);
        // }

        // /// <summary>
        // /// 比较同花
        // /// </summary>
        // private static int CompareFlush(List<Card> hand1, List<Card> hand2)
        // {
        //     // 同花比较最高牌，然后第二高牌，最后最低牌
        //     var sorted1 = hand1.OrderByDescending(c => (int)c.rank).ToList();
        //     var sorted2 = hand2.OrderByDescending(c => (int)c.rank).ToList();
            
        //     for (int i = 0; i < 3; i++)
        //     {
        //         int result = CompareRank(sorted1[i].rank, sorted2[i].rank);
        //         if (result != 0)
        //             return result;
        //     }
            
        //     // 如果所有牌面都相同，比较花色
        //     return CompareSuit(sorted1[0].suit, sorted2[0].suit);
        // }

        // /// <summary>
        // /// 比较顺子
        // /// </summary>
        // private static int CompareStraight(List<Card> hand1, List<Card> hand2)
        // {
        //     // 获取顺子的最高牌
        //     Rank high1 = GetStraightHighCard(hand1);
        //     Rank high2 = GetStraightHighCard(hand2);
            
        //     return CompareRank(high1, high2);
        // }

        // /// <summary>
        // /// 获取顺子的最高牌
        // /// </summary>
        // private static Rank GetStraightHighCard(List<Card> cards)
        // {
        //     var sorted = cards.OrderBy(c => (int)c.rank).ToList();
            
        //     // A-2-3顺子，最高牌是3
        //     if (sorted[0].rank == Rank.Two && sorted[1].rank == Rank.Three && sorted[2].rank == Rank.Ace)
        //         return Rank.Three;
            
        //     // 其他顺子，最高牌就是最大的那张
        //     return sorted[2].rank;
        // }

        // /// <summary>
        // /// 比较对子
        // /// </summary>
        // private static int ComparePair(List<Card> hand1, List<Card> hand2)
        // {
        //     // 先比较对子的牌面
        //     Rank pair1 = GetPairRank(hand1);
        //     Rank pair2 = GetPairRank(hand2);
            
        //     int pairResult = CompareRank(pair1, pair2);
        //     if (pairResult != 0)
        //         return pairResult;
            
        //     // 对子相同，比较单张
        //     Rank kicker1 = GetKickerRank(hand1, pair1);
        //     Rank kicker2 = GetKickerRank(hand2, pair2);
            
        //     return CompareRank(kicker1, kicker2);
        // }

        // /// <summary>
        // /// 获取对子的牌面
        // /// </summary>
        // private static Rank GetPairRank(List<Card> cards)
        // {
        //     if (cards[0].rank == cards[1].rank)
        //         return cards[0].rank;
        //     else if (cards[0].rank == cards[2].rank)
        //         return cards[0].rank;
        //     else
        //         return cards[1].rank;
        // }

        // /// <summary>
        // /// 获取踢脚牌的牌面
        // /// </summary>
        // private static Rank GetKickerRank(List<Card> cards, Rank pairRank)
        // {
        //     return cards.First(c => c.rank != pairRank).rank;
        // }

        // /// <summary>
        // /// 比较高牌
        // /// </summary>
        // private static int CompareHighCard(List<Card> hand1, List<Card> hand2)
        // {
        //     // 高牌比较从最高牌开始逐一比较
        //     var sorted1 = hand1.OrderByDescending(c => (int)c.rank).ToList();
        //     var sorted2 = hand2.OrderByDescending(c => (int)c.rank).ToList();
            
        //     for (int i = 0; i < 3; i++)
        //     {
        //         int result = CompareRank(sorted1[i].rank, sorted2[i].rank);
        //         if (result != 0)
        //             return result;
        //     }
            
        //     // 如果所有牌面都相同，比较最高牌的花色
        //     return CompareSuit(sorted1[0].suit, sorted2[0].suit);
        // }

        // #endregion

        #region 工具方法

        /// <summary>
        /// 将手牌按牌面大小排序
        /// </summary>
        public static List<Card> SortCardsByRank(List<Card> cards, bool ascending = true)
        {
            if (ascending)
                return cards.OrderBy(c => (int)c.rank).ToList();
            else
                return cards.OrderByDescending(c => (int)c.rank).ToList();
        }

        /// <summary>
        /// 获取手牌类型的中文名称
        /// </summary>
        public static string GetHandTypeName(HandType handType)
        {
            switch (handType)
            {
                case HandType.HighCard: return "高牌";
                case HandType.Pair: return "对子";
                case HandType.Straight: return "顺子";
                case HandType.Flush: return "同花";
                case HandType.StraightFlush: return "同花顺";
                case HandType.ThreeOfAKind: return "三条";
                default: return "未知";
            }
        }

        #endregion
    }
}