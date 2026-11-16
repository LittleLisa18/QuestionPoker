using UnityEngine;
using System.Collections.Generic;
using CardGame;

public class SmartPlayerManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerCardSetup
    {
        [Header("玩家名称")]
        public string playerName = "玩家";
        
        [Header("是否为智能玩家")]
        public bool isSmartPlayer = false;
        
        [Header("初始7张卡牌")]
        public List<Card> initialCards = new List<Card>(7);
        
        [Header("分组结果")]
        public List<Card> group1 = new List<Card>();
        public List<Card> group2 = new List<Card>();
        public List<Card> group3 = new List<Card>();
        public List<Card> group4 = new List<Card>();
    }

    [Header("玩家1设置")]
    public PlayerCardSetup player1Setup;

    [Header("玩家2设置")]
    public PlayerCardSetup player2Setup;

    [Header("游戏结果")]
    [TextArea(10, 20)]
    public string gameResultText;

    [ContextMenu("优化智能玩家分组")]
    public void OptimizeSmartPlayers()
    {
        // 优化玩家1（如果是智能玩家）
        if (player1Setup.isSmartPlayer && player1Setup.initialCards.Count == 7)
        {
            var groups = SmartCardPlayer.OptimizeCardGroups(player1Setup.initialCards, player1Setup.playerName);
            player1Setup.group1 = groups.Group1;
            player1Setup.group2 = groups.Group2;
            player1Setup.group3 = groups.Group3;
            player1Setup.group4 = groups.Group4;
            Debug.Log($"{player1Setup.playerName}分组优化完成！");
        }

        // 优化玩家2（如果是智能玩家）
        if (player2Setup.isSmartPlayer && player2Setup.initialCards.Count == 7)
        {
            var groups = SmartCardPlayer.OptimizeCardGroups(player2Setup.initialCards, player2Setup.playerName);
            player2Setup.group1 = groups.Group1;
            player2Setup.group2 = groups.Group2;
            player2Setup.group3 = groups.Group3;
            player2Setup.group4 = groups.Group4;
            Debug.Log($"{player2Setup.playerName}分组优化完成！");
        }
    }

    [ContextMenu("运行卡牌比较")]
    public void RunCardComparison()
    {
        // 创建玩家1的牌组
        var player1Groups = new CardGameScorer.PlayerCardGroups
        {
            PlayerName = player1Setup.playerName,
            Group1 = player1Setup.group1,
            Group2 = player1Setup.group2,
            Group3 = player1Setup.group3,
            Group4 = player1Setup.group4
        };

        // 创建玩家2的牌组
        var player2Groups = new CardGameScorer.PlayerCardGroups
        {
            PlayerName = player2Setup.playerName,
            Group1 = player2Setup.group1,
            Group2 = player2Setup.group2,
            Group3 = player2Setup.group3,
            Group4 = player2Setup.group4
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
        sb.AppendLine($"最终得分: {player1Setup.playerName} {result.Player1Score} - {player2Setup.playerName} {result.Player2Score}");
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
        // 玩家1设置为智能玩家
        player1Setup = new PlayerCardSetup
        {
            playerName = "智能玩家",
            isSmartPlayer = true,
            initialCards = new List<Card>
            {
                new Card(Suit.Spades, Rank.Ace),
                new Card(Suit.Hearts, Rank.Two),
                new Card(Suit.Diamonds, Rank.King),
                new Card(Suit.Clubs, Rank.Queen),
                new Card(Suit.Spades, Rank.Jack),
                new Card(Suit.Hearts, Rank.Jack),
                new Card(Suit.Diamonds, Rank.Jack)
            }
        };

        // 玩家2设置为普通玩家
        player2Setup = new PlayerCardSetup
        {
            playerName = "普通玩家",
            isSmartPlayer = false,
            group1 = new List<Card> { new Card(Suit.Hearts, Rank.King) },
            group2 = new List<Card> { new Card(Suit.Spades, Rank.Five) },
            group3 = new List<Card> { 
                new Card(Suit.Clubs, Rank.Ace),
                new Card(Suit.Diamonds, Rank.Ten)
            },
            group4 = new List<Card> { 
                new Card(Suit.Hearts, Rank.Seven),
                new Card(Suit.Diamonds, Rank.Eight),
                new Card(Suit.Spades, Rank.Nine)
            }
        };

        // 优化智能玩家分组
        OptimizeSmartPlayers();
        
        // 运行比较
        RunCardComparison();
    }
}