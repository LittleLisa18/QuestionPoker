using System;
using System.Collections.Generic;
using System.Linq;

namespace CardGame
{
    /// <summary>
    /// 智能玩家类 - 自动将7张卡牌分组以最大化得分潜力
    /// </summary>
    public static class SmartCardPlayer
    {
        /// <summary>
        /// 优化分组以最大化得分潜力
        /// </summary>
        public static CardGameScorer.PlayerCardGroups OptimizeCardGroups(List<Card> sevenCards, string playerName)
        {
            if (sevenCards.Count != 7)
            {
                throw new ArgumentException("必须提供7张卡牌");
            }

            // 存储最佳分组和得分
            CardGameScorer.PlayerCardGroups bestGroups = null;
            double bestScore = double.MinValue;

            // 生成所有可能的Group1选择
            foreach (var group1Card in sevenCards)
            {
                var group1 = new List<Card> { group1Card };
                var remaining1 = sevenCards.Except(group1).ToList();
                
                // 生成所有可能的Group2选择
                foreach (var group2Card in remaining1)
                {
                    var group2 = new List<Card> { group2Card };
                    var remaining2 = remaining1.Except(group2).ToList();
                    
                    // 生成所有可能的Group3组合
                    for (int i = 0; i < remaining2.Count; i++)
                    {
                        for (int j = i + 1; j < remaining2.Count; j++)
                        {
                            var group3 = new List<Card> { remaining2[i], remaining2[j] };
                            var group4 = remaining2.Except(group3).ToList();
                            
                            // 创建分组
                            var groups = new CardGameScorer.PlayerCardGroups
                            {
                                PlayerName = playerName,
                                Group1 = group1,
                                Group2 = group2,
                                Group3 = group3,
                                Group4 = group4
                            };
                            
                            // 评估分组得分潜力
                            double score = EvaluateGroupScore(groups);
                            
                            // 更新最佳分组
                            if (score > bestScore)
                            {
                                bestScore = score;
                                bestGroups = groups;
                            }
                        }
                    }
                }
            }

            return bestGroups;
        }

        /// <summary>
        /// 评估分组得分潜力
        /// </summary>
        private static double EvaluateGroupScore(CardGameScorer.PlayerCardGroups groups)
        {
            double score = 0;
            
            // Group1评分（大牌胜）
            score += (int)groups.Group1[0].rank * 0.5;
            score += (int)groups.Group1[0].suit * 0.1;
            
            // Group2评分（小牌胜）
            score += (14 - (int)groups.Group2[0].rank) * 0.5;
            score += (4 - (int)groups.Group2[0].suit) * 0.1;
            
            // Group3评分（点数和大的胜）
            score += HandComparison.CalculateCardSum(groups.Group3[0], groups.Group3[1]) * 0.7;
            
            // Group4评分（牌型大的胜）
            var handType = HandComparison.EvaluateHand(groups.Group4);
            score += (int)handType * 2.0; // 牌型权重更高
            
            // 额外奖励特殊组合
            if (handType == HandType.ThreeOfAKind)
            {
                score += 5.0; // 三条额外奖励
            }
            else if (handType == HandType.StraightFlush)
            {
                score += 4.0; // 同花顺额外奖励
            }
            else if (handType == HandType.Flush)
            {
                score += 3.0; // 同花额外奖励
            }
            else if (handType == HandType.Straight)
            {
                score += 2.0; // 顺子额外奖励
            }
            else if (handType == HandType.Pair)
            {
                score += 1.0; // 对子额外奖励
            }
            
            return score;
        }

        /// <summary>
        /// 示例：智能玩家分组
        /// </summary>
        public static void RunSmartPlayerExample()
        {
            // 创建智能玩家的7张牌
            var smartPlayerCards = new List<Card>
            {
                new Card(Suit.Spades, Rank.Ace),
                new Card(Suit.Hearts, Rank.Two),
                new Card(Suit.Diamonds, Rank.King),
                new Card(Suit.Clubs, Rank.Queen),
                new Card(Suit.Spades, Rank.Jack),
                new Card(Suit.Hearts, Rank.Jack),
                new Card(Suit.Diamonds, Rank.Jack)
            };
            
            // 智能分组
            var smartGroups = OptimizeCardGroups(smartPlayerCards, "智能玩家");
            
            // 创建对手玩家（使用固定分组）
            var opponentGroups = new CardGameScorer.PlayerCardGroups
            {
                PlayerName = "对手",
                Group1 = CardGameScorer.CreateSingleCardGroup(Suit.Hearts, Rank.King),
                Group2 = CardGameScorer.CreateSingleCardGroup(Suit.Spades, Rank.Five),
                Group3 = CardGameScorer.CreateTwoCardGroup(Suit.Clubs, Rank.Ace, Suit.Diamonds, Rank.Ten),
                Group4 = CardGameScorer.CreateThreeCardGroup(Suit.Hearts, Rank.Seven, 
                    Suit.Diamonds, Rank.Eight, Suit.Spades, Rank.Nine)
            };
            
            // 进行比较
            var result = CardGameScorer.CompareAndScore(smartGroups, opponentGroups);
            
            // 输出结果
            Console.WriteLine("===== 智能玩家分组结果 =====");
            Console.WriteLine($"Group1: {string.Join(", ", smartGroups.Group1)}");
            Console.WriteLine($"Group2: {string.Join(", ", smartGroups.Group2)}");
            Console.WriteLine($"Group3: {string.Join(", ", smartGroups.Group3)}");
            Console.WriteLine($"Group4: {string.Join(", ", smartGroups.Group4)}");
            Console.WriteLine();
            
            Console.WriteLine("===== 对战结果 =====");
            Console.WriteLine($"最终得分: {result.Player1Score} vs {result.Player2Score}");
            Console.WriteLine($"获胜者: {result.Winner}");
        }
         // /// <summary>
        // /// 将7张卡牌分成4组以获得最高得分潜力
        // /// </summary>
        // /// <param name="sevenCards">玩家的7张卡牌</param>
        // /// <param name="playerName">玩家名称</param>
        // /// <returns>分组后的玩家牌组</returns>
        // public static CardGameScorer.PlayerCardGroups DivideCardsIntoTroups(List<Card> sevenCards, string playerName)
        // {
        //     if (sevenCards.Count != 7)
        //     {
        //         throw new ArgumentException("必须提供7张卡牌");
        //     }

        //     // 第一步：找出最佳Group1（大牌胜）
        //     var bestGroup1 = FindBestGroup1(sevenCards);
        //     var remaining1 = sevenCards.Except(bestGroup1).ToList();
            
        //     // 第二步：找出最佳Group2（小牌胜）
        //     var bestGroup2 = FindBestGroup2(remaining1);
        //     var remaining2 = remaining1.Except(bestGroup2).ToList();
            
        //     // 第三步：找出最佳Group3（点数和大的胜）
        //     var bestGroup3 = FindBestGroup3(remaining2);
        //     var remaining3 = remaining2.Except(bestGroup3).ToList();
            
        //     // 第四步：剩余3张作为Group4（牌型大的胜）
        //     var bestGroup4 = FindBestGroup4(remaining3);

        //     return new CardGameScorer.PlayerCardGroups
        //     {
        //         PlayerName = playerName,
        //         Group1 = bestGroup1,
        //         Group2 = bestGroup2,
        //         Group3 = bestGroup3,
        //         Group4 = bestGroup4
        //     };
        // }

        // /// <summary>
        // /// 寻找最佳Group1（大牌胜）
        // /// </summary>
        // private static List<Card> FindBestGroup1(List<Card> cards)
        // {
        //     // 选择最大牌（先按牌面，再按花色）
        //     return new List<Card> { cards
        //         .OrderByDescending(c => (int)c.rank)
        //         .ThenByDescending(c => (int)c.suit)
        //         .First() };
        // }

        // /// <summary>
        // /// 寻找最佳Group2（小牌胜）
        // /// </summary>
        // private static List<Card> FindBestGroup2(List<Card> cards)
        // {
        //     // 选择最小牌（先按牌面，再按花色）
        //     return new List<Card> { cards
        //         .OrderBy(c => (int)c.rank)
        //         .ThenBy(c => (int)c.suit)
        //         .First() };
        // }

        // /// <summary>
        // /// 寻找最佳Group3（点数和大的胜）
        // /// </summary>
        // private static List<Card> FindBestGroup3(List<Card> cards)
        // {
        //     // 找出所有可能的2张组合
        //     var possibleGroups = new List<List<Card>>();
        //     for (int i = 0; i < cards.Count; i++)
        //     {
        //         for (int j = i + 1; j < cards.Count; j++)
        //         {
        //             possibleGroups.Add(new List<Card> { cards[i], cards[j] });
        //         }
        //     }

        //     // 选择点数和最大的组合
        //     return possibleGroups
        //         .OrderByDescending(group => 
        //             HandComparison.CalculateCardSum(group[0], group[1]))
        //         .First();
        // }

        // /// <summary>
        // /// 寻找最佳Group4（牌型大的胜）
        // /// </summary>
        // private static List<Card> FindBestGroup4(List<Card> cards)
        // {
        //     // 只有一种组合（3张牌）
        //     return cards;
        // }
    }
}