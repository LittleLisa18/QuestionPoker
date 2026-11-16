using System;

namespace CardGame
{
    /// <summary>
    /// 花色枚举 (按优先级排序: 黑桃 > 红心 > 梅花 > 方块)
    /// </summary>
    public enum Suit
    {
        Diamonds = 1,   // 方块
        Clubs = 2,      // 梅花  
        Hearts = 3,     // 红心
        Spades = 4      // 黑桃
    }

    /// <summary>
    /// 牌面枚举 (按大小排序: A最大=14)
    /// </summary>
    public enum Rank
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }

    /// <summary>
    /// 手牌类型枚举 (按大小排序)
    /// </summary>
    public enum HandType
    {
        HighCard = 0,        // 高牌
        Pair = 1,            // 对子
        Straight = 2,        // 顺子
        Flush = 3,           // 同花
        StraightFlush = 4,   // 同花顺
        ThreeOfAKind = 5     // 三条
    }

    /// <summary>
    /// 卡牌类
    /// </summary>
    [System.Serializable]
    public struct Card
    {
        public Suit suit;
        public Rank rank;

        public Card(Suit suit, Rank rank)
        {
            this.suit = suit;
            this.rank = rank;
        }

        /// <summary>
        /// 获取花色的中文名称
        /// </summary>
        public string GetSuitName()
        {
            switch (suit)
            {
                case Suit.Spades: return "黑桃";
                case Suit.Hearts: return "红心";
                case Suit.Clubs: return "梅花";
                case Suit.Diamonds: return "方块";
                default: return "未知";
            }
        }

        /// <summary>
        /// 获取牌面的显示名称
        /// </summary>
        public string GetRankName()
        {
            switch (rank)
            {
                case Rank.Ace: return "A";
                case Rank.King: return "K";
                case Rank.Queen: return "Q";
                case Rank.Jack: return "J";
                default: return ((int)rank).ToString();
            }
        }

        /// <summary>
        /// 获取卡牌的完整显示名称
        /// </summary>
        public override string ToString()
        {
            return $"{GetSuitName()}{GetRankName()}";
        }

        /// <summary>
        /// 比较两张卡牌是否相等
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is Card other)
            {
                return suit == other.suit && rank == other.rank;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return suit.GetHashCode() ^ rank.GetHashCode();
        }

        public static bool operator ==(Card left, Card right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Card left, Card right)
        {
            return !left.Equals(right);
        }
    }
}