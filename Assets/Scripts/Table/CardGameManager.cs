using UnityEngine;
using System.Collections.Generic;
using CardGame;

public class CardGameManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerCardSetup
    {
        [Header("Group 1 (单张)")]
        public Card group1Card;
        
        [Header("Group 2 (单张)")]
        public Card group2Card;
        
        [Header("Group 3 (两张牌)")]
        public Card[] group3Cards = new Card[2];
        
        [Header("Group 4 (三张牌)")]
        public Card[] group4Cards = new Card[3];
        
        [Header("玩家名称")]
        public string playerName = "玩家";
    }

    [Header("玩家1设置")]
    public PlayerCardSetup player1Setup;

    [Header("玩家2设置")]
    public PlayerCardSetup player2Setup;

    [Header("游戏结果")]
    [TextArea(10, 20)]
    public string gameResultText;

    [ContextMenu("运行卡牌比较")]
    public void RunCardComparison()
    {
        // 创建玩家1的牌组
        var player1Groups = new CardGameScorer.PlayerCardGroups
        {
            PlayerName = player1Setup.playerName,
            Group1 = new List<Card> { player1Setup.group1Card },
            Group2 = new List<Card> { player1Setup.group2Card },
            Group3 = new List<Card>(player1Setup.group3Cards),
            Group4 = new List<Card>(player1Setup.group4Cards)
        };

        // 创建玩家2的牌组
        var player2Groups = new CardGameScorer.PlayerCardGroups
        {
            PlayerName = player2Setup.playerName,
            Group1 = new List<Card> { player2Setup.group1Card },
            Group2 = new List<Card> { player2Setup.group2Card },
            Group3 = new List<Card>(player2Setup.group3Cards),
            Group4 = new List<Card>(player2Setup.group4Cards)
        };

        // 进行比较并获取结果
        var result = CardGameScorer.CompareAndScore(player1Groups, player2Groups);
        
        // 格式化结果显示
        gameResultText = FormatGameResult(result);
        Debug.Log(gameResultText);
    }

    private string FormatGameResult(CardGameScorer.GameResult result)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("===== 卡牌游戏结果 =====");
        sb.AppendLine($"最终得分: {result.Player1Score} vs {result.Player2Score}");
        sb.AppendLine($"获胜者: {result.Winner}");
        sb.AppendLine();
        sb.AppendLine("===== 详细回合结果 =====");
        
        foreach (var round in result.RoundResults)
        {
            sb.AppendLine(round);
        }
        
        sb.AppendLine();
        sb.AppendLine("===== 组比较详情 =====");
        
        foreach (var detail in result.GroupDetails)
        {
            sb.AppendLine($"第{detail.GroupNumber}组: {detail.Rule}");
            sb.AppendLine($"{player1Setup.playerName}: {string.Join(", ", detail.Player1Cards)}");
            sb.AppendLine($"{player2Setup.playerName}: {string.Join(", ", detail.Player2Cards)}");
            
            if (!string.IsNullOrEmpty(detail.AdditionalInfo))
            {
                sb.AppendLine(detail.AdditionalInfo);
            }
            
            string winner = detail.ComparisonResult switch
            {
                1 => $"{player1Setup.playerName}胜 (+{detail.Points}分)",
                2 => $"{player2Setup.playerName}胜 (+{detail.Points}分)",
                _ => "平局"
            };
            
            sb.AppendLine($"结果: {winner}");
            sb.AppendLine();
        }
        
        return sb.ToString();
    }

    [ContextMenu("运行示例游戏")]
    public void RunExampleGame()
    {
        // 创建玩家1的牌组
        player1Setup = new PlayerCardSetup
        {
            playerName = "玩家1",
            group1Card = new Card(Suit.Spades, Rank.Ace),
            group2Card = new Card(Suit.Hearts, Rank.Two),
            group3Cards = new Card[]
            {
                new Card(Suit.Diamonds, Rank.King),
                new Card(Suit.Clubs, Rank.Queen)
            },
            group4Cards = new Card[]
            {
                new Card(Suit.Spades, Rank.Jack),
                new Card(Suit.Hearts, Rank.Jack),
                new Card(Suit.Diamonds, Rank.Jack)
            }
        };

        // 创建玩家2的牌组
        player2Setup = new PlayerCardSetup
        {
            playerName = "玩家2",
            group1Card = new Card(Suit.Hearts, Rank.King),
            group2Card = new Card(Suit.Spades, Rank.Five),
            group3Cards = new Card[]
            {
                new Card(Suit.Clubs, Rank.Ace),
                new Card(Suit.Diamonds, Rank.Ten)
            },
            group4Cards = new Card[]
            {
                new Card(Suit.Hearts, Rank.Seven),
                new Card(Suit.Diamonds, Rank.Eight),
                new Card(Suit.Spades, Rank.Nine)
            }
        };

        // 运行比较
        RunCardComparison();
    }
}