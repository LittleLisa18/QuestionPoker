using System;
using System.Collections.Generic;
using System.Linq;

namespace CardGame
{
    /// <summary>
    /// 纯C#卡牌游戏计分器 - 不依赖Unity，专门用于比较已分组的牌并计算得分
    /// </summary>
    public static class CardGameScorer
    {
        public class PlayerCardGroups
        {
            public List<Card> Group1 { get; set; } = new List<Card>(); // 1张牌
            public List<Card> Group2 { get; set; } = new List<Card>(); // 1张牌
            public List<Card> Group3 { get; set; } = new List<Card>(); // 2张牌
            public List<Card> Group4 { get; set; } = new List<Card>(); // 3张牌
            public string PlayerName { get; set; } = "";
        }
        
        public class GameResult
        {
            public int Player1Score { get; set; } = 0;
            public int Player2Score { get; set; } = 0;
            public string Winner { get; set; } = "";
            public List<string> RoundResults { get; set; } = new List<string>();
            public List<GroupComparisonDetail> GroupDetails { get; set; } = new List<GroupComparisonDetail>();
        }
        
        public class GroupComparisonDetail
        {
            public int GroupNumber { get; set; }
            public string Rule { get; set; }
            public List<Card> Player1Cards { get; set; }
            public List<Card> Player2Cards { get; set; }
            public int ComparisonResult { get; set; } // 1=玩家1胜, 2=玩家2胜, 0=平局
            public int Points { get; set; }
            public string AdditionalInfo { get; set; } // 额外信息，如点数和、手牌类型等
        }
        
       /// <summary>
       /// 比较Group1大小
       /// </summary>
       public static int CompareGroup1_Logic(List<Card> group1, List<Card> group2)
       {
            return HandComparison.CompareGroup1(group1, group2);
       }

       /// <summary>
       /// 比较Group2大小
       /// </summary>
       public static int CompareGroup2_Logic(List<Card> group1, List<Card> group2)
       {
            return HandComparison.CompareGroup2(group1, group2);
       }

       /// <summary>
       /// 比较Group3大小
       /// </summary>
       public static int CompareGroup3_Logic(List<Card> group1, List<Card> group2)
       {
            return HandComparison.CompareGroup3(group1, group2);
       }

       /// <summary>
       /// 比较Group4大小
       /// </summary>
       public static int CompareGroup4_Logic(List<Card> group1, List<Card> group2)
       {
            return HandComparison.CompareGroup4(group1, group2);
       }

        /// <summary>
        /// 比较两个玩家的牌组并计算得分
        /// </summary>
        public static GameResult CompareAndScore(PlayerCardGroups player1Groups, PlayerCardGroups player2Groups)
        {
            GameResult result = new GameResult();
            
            Console.WriteLine("================================");
            Console.WriteLine("开始比较计分");
            Console.WriteLine($"玩家1: {player1Groups.PlayerName}");
            Console.WriteLine($"玩家2: {player2Groups.PlayerName}");
            Console.WriteLine("================================");
            
            // 第一组比较 (1张牌，大牌胜，得1分)
            var detail1 = CompareGroup1(player1Groups.Group1, player2Groups.Group1, player1Groups.PlayerName, player2Groups.PlayerName);
            result.GroupDetails.Add(detail1);
            if (detail1.ComparisonResult == 1) result.Player1Score += detail1.Points;
            else if (detail1.ComparisonResult == 2) result.Player2Score += detail1.Points;
            
            // 第二组比较 (1张牌，小牌胜，得1分)
            var detail2 = CompareGroup2(player1Groups.Group2, player2Groups.Group2, player1Groups.PlayerName, player2Groups.PlayerName);
            result.GroupDetails.Add(detail2);
            if (detail2.ComparisonResult == 1) result.Player1Score += detail2.Points;
            else if (detail2.ComparisonResult == 2) result.Player2Score += detail2.Points;
            
            // 第三组比较 (2张牌，点数和大的胜，得2分)
            var detail3 = CompareGroup3(player1Groups.Group3, player2Groups.Group3, player1Groups.PlayerName, player2Groups.PlayerName);
            result.GroupDetails.Add(detail3);
            if (detail3.ComparisonResult == 1) result.Player1Score += detail3.Points;
            else if (detail3.ComparisonResult == 2) result.Player2Score += detail3.Points;
            
            // 第四组比较 (3张牌，牌型大的胜，得3分)
            var detail4 = CompareGroup4(player1Groups.Group4, player2Groups.Group4, player1Groups.PlayerName, player2Groups.PlayerName);
            result.GroupDetails.Add(detail4);
            if (detail4.ComparisonResult == 1) result.Player1Score += detail4.Points;
            else if (detail4.ComparisonResult == 2) result.Player2Score += detail4.Points;
            
            // 生成回合结果字符串
            foreach (var detail in result.GroupDetails)
            {
                string resultStr = detail.ComparisonResult == 1 ? $"{player1Groups.PlayerName}胜！(+{detail.Points}分)" :
                                  detail.ComparisonResult == 2 ? $"{player2Groups.PlayerName}胜！(+{detail.Points}分)" : "平局！";
                result.RoundResults.Add($"第{detail.GroupNumber}组: {resultStr}");
            }
            
            // 判断最终胜者
            if (result.Player1Score > result.Player2Score)
                result.Winner = player1Groups.PlayerName;
            else if (result.Player2Score > result.Player1Score)
                result.Winner = player2Groups.PlayerName;
            else
                result.Winner = "平局";
            
            Console.WriteLine("================================");
            Console.WriteLine($"最终得分 - {player1Groups.PlayerName}: {result.Player1Score}, {player2Groups.PlayerName}: {result.Player2Score}");
            Console.WriteLine($"获胜者: {result.Winner}");
            Console.WriteLine("================================");
            
            return result;
        }
        
        /// <summary>
        /// 比较第一组牌（单牌，大的胜）
        /// </summary>
        private static GroupComparisonDetail CompareGroup1(List<Card> group1_player1, List<Card> group1_player2, 
            string player1Name, string player2Name)
        {
            var detail = new GroupComparisonDetail
            {
                GroupNumber = 1,
                Rule = "大牌胜",
                Player1Cards = new List<Card>(group1_player1),
                Player2Cards = new List<Card>(group1_player2),
                Points = 1
            };
            
            if (group1_player1.Count != 1 || group1_player2.Count != 1)
            {
                Console.WriteLine("第一组比较需要每组1张牌！");
                detail.ComparisonResult = 0;
                return detail;
            }
            
            detail.ComparisonResult = CompareGroup1_Logic(group1_player1, group1_player2);
            
            PrintGroupResult(detail, player1Name, player2Name);
            return detail;
        }
        
        /// <summary>
        /// 比较第二组牌（单牌，小的胜）
        /// </summary>
        private static GroupComparisonDetail CompareGroup2(List<Card> group2_player1, List<Card> group2_player2,
            string player1Name, string player2Name)
        {
            var detail = new GroupComparisonDetail
            {
                GroupNumber = 2,
                Rule = "小牌胜",
                Player1Cards = new List<Card>(group2_player1),
                Player2Cards = new List<Card>(group2_player2),
                Points = 1
            };
            
            if (group2_player1.Count != 1 || group2_player2.Count != 1)
            {
                Console.WriteLine("第二组比较需要每组1张牌！");
                detail.ComparisonResult = 0;
                return detail;
            }
            
            detail.ComparisonResult = CompareGroup2_Logic(group2_player1, group2_player2);
            
            PrintGroupResult(detail, player1Name, player2Name);
            return detail;
        }
        
        /// <summary>
        /// 比较第三组牌（两张牌，点数和不超过21且更接近21的胜）
        /// </summary>
        private static GroupComparisonDetail CompareGroup3(List<Card> group3_player1, List<Card> group3_player2,
            string player1Name, string player2Name)
        {
            var detail = new GroupComparisonDetail
            {
                GroupNumber = 3,
                Rule = "点数和大的胜",
                Player1Cards = new List<Card>(group3_player1),
                Player2Cards = new List<Card>(group3_player2),
                Points = 2
            };
            
            if (group3_player1.Count != 2 || group3_player2.Count != 2)
            {
                Console.WriteLine("第三组比较需要每组2张牌！");
                detail.ComparisonResult = 0;
                return detail;
            }
            
            detail.ComparisonResult = CompareGroup3_Logic(group3_player1, group3_player2);

            int sum1 = HandComparison.CalculateCardSum(group3_player1[0], group3_player1[1]);
            int sum2 = HandComparison.CalculateCardSum(group3_player2[0], group3_player2[1]);
            detail.AdditionalInfo = $"点数和: {player1Name}={sum1}, {player2Name}={sum2}";
            
            PrintGroupResult(detail, player1Name, player2Name);
            return detail;
        }
        
        /// <summary>
        /// 比较第四组牌（三张牌，按手牌类型比较）
        /// </summary>
        private static GroupComparisonDetail CompareGroup4(List<Card> group4_player1, List<Card> group4_player2,
            string player1Name, string player2Name)
        {
            var detail = new GroupComparisonDetail
            {
                GroupNumber = 4,
                Rule = "牌型大的胜",
                Player1Cards = new List<Card>(group4_player1),
                Player2Cards = new List<Card>(group4_player2),
                Points = 3
            };
            
            if (group4_player1.Count != 3 || group4_player2.Count != 3)
            {
                Console.WriteLine("第四组比较需要每组3张牌！");
                detail.ComparisonResult = 0;
                return detail;
            }
            
            HandType hand1 = HandComparison.EvaluateHand(group4_player1);
            HandType hand2 = HandComparison.EvaluateHand(group4_player2);
            
            detail.AdditionalInfo = $"手牌类型: {player1Name}={hand1}, {player2Name}={hand2}";
            detail.ComparisonResult = CompareGroup4_Logic(group4_player1, group4_player2);
            
            PrintGroupResult(detail, player1Name, player2Name);
            return detail;
        }
        
        /// <summary>
        /// 输出组比较结果
        /// </summary>
        private static void PrintGroupResult(GroupComparisonDetail detail, string player1Name, string player2Name)
        {
            string group1Str = string.Join(", ", detail.Player1Cards.Select(c => c.ToString()));
            string group2Str = string.Join(", ", detail.Player2Cards.Select(c => c.ToString()));
            
            Console.WriteLine($"Group {detail.GroupNumber}: {detail.Rule}");
            Console.WriteLine($"{player1Name}: [{group1Str}]");
            Console.WriteLine($"{player2Name}: [{group2Str}]");
            
            if (!string.IsNullOrEmpty(detail.AdditionalInfo))
            {
                Console.WriteLine(detail.AdditionalInfo);
            }
            
            if (detail.ComparisonResult == 1)
            {
                Console.WriteLine($"{player1Name}胜！(+{detail.Points}分)");
            }
            else if (detail.ComparisonResult == 2)
            {
                Console.WriteLine($"{player2Name}胜！(+{detail.Points}分)");
            }
            else
            {
                Console.WriteLine("平局！");
            }
            
            Console.WriteLine("--------------------------------");
        }
        
        #region 便捷方法
        
        /// <summary>
        /// 创建玩家牌组
        /// </summary>
        public static PlayerCardGroups CreatePlayerGroups(string playerName, List<Card> group1, List<Card> group2, 
            List<Card> group3, List<Card> group4)
        {
            return new PlayerCardGroups
            {
                PlayerName = playerName,
                Group1 = new List<Card>(group1),
                Group2 = new List<Card>(group2),
                Group3 = new List<Card>(group3),
                Group4 = new List<Card>(group4)
            };
        }
        
        /// <summary>
        /// 创建单张牌组
        /// </summary>
        public static List<Card> CreateSingleCardGroup(Suit suit, Rank rank)
        {
            return new List<Card> { new Card(suit, rank) };
        }
        
        /// <summary>
        /// 创建两张牌组
        /// </summary>
        public static List<Card> CreateTwoCardGroup(Suit suit1, Rank rank1, Suit suit2, Rank rank2)
        {
            return new List<Card> { new Card(suit1, rank1), new Card(suit2, rank2) };
        }
        
        /// <summary>
        /// 创建三张牌组
        /// </summary>
        public static List<Card> CreateThreeCardGroup(Suit suit1, Rank rank1, Suit suit2, Rank rank2, Suit suit3, Rank rank3)
        {
            return new List<Card> { new Card(suit1, rank1), new Card(suit2, rank2), new Card(suit3, rank3) };
        }
        
        #endregion
        
        /// <summary>
        /// 使用示例
        /// </summary>
        public static class Example
        {
            public static void RunExample()
            {
                // 创建玩家1的牌组
                var player1Groups = CardGameScorer.CreatePlayerGroups("玩家1",
                    CardGameScorer.CreateSingleCardGroup(Suit.Spades, Rank.Ace),        // 第1组：黑桃A
                    CardGameScorer.CreateSingleCardGroup(Suit.Hearts, Rank.Two),        // 第2组：红心2
                    CardGameScorer.CreateTwoCardGroup(Suit.Diamonds, Rank.King, Suit.Clubs, Rank.Queen), // 第3组：方块K+梅花Q
                    CardGameScorer.CreateThreeCardGroup(Suit.Spades, Rank.Jack, Suit.Hearts, Rank.Jack, Suit.Diamonds, Rank.Jack) // 第4组：三条J
                );
                
                // 创建玩家2的牌组
                var player2Groups = CardGameScorer.CreatePlayerGroups("玩家2",
                    CardGameScorer.CreateSingleCardGroup(Suit.Hearts, Rank.King),       // 第1组：红心K
                    CardGameScorer.CreateSingleCardGroup(Suit.Spades, Rank.Five),       // 第2组：黑桃5
                    CardGameScorer.CreateTwoCardGroup(Suit.Clubs, Rank.Ace, Suit.Diamonds, Rank.Ten), // 第3组：梅花A+方块10
                    CardGameScorer.CreateThreeCardGroup(Suit.Hearts, Rank.Seven, Suit.Diamonds, Rank.Eight, Suit.Spades, Rank.Nine) // 第4组：顺子7-8-9
                );
                
                // 进行比较计分
                var result = CardGameScorer.CompareAndScore(player1Groups, player2Groups);
                
                // 输出详细结果
                Console.WriteLine("=== 游戏结果详情 ===");
                foreach (string roundResult in result.RoundResults)
                {
                    Console.WriteLine(roundResult);
                }
                Console.WriteLine($"最终得分 - {player1Groups.PlayerName}: {result.Player1Score}, {player2Groups.PlayerName}: {result.Player2Score}");
                Console.WriteLine($"获胜者: {result.Winner}");
            }
        }
    }
    
    /// <summary>
    /// 程序入口
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // 运行示例
            CardGameScorer.Example.RunExample();
            
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }
    }
}