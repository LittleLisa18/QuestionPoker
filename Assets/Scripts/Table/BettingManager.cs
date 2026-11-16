using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class BettingManager : MonoBehaviour
{
    public PlayerBettingSystem playerBettingSystem;
    public EnemyBettingSystem demonBettingSystem;
    public CardComparisonManager cardComparisonManager;
    public ChangeStageHandler changeStageManager;
    public StoryTellingHandler storyTellingManagerRound1;
    public StoryTellingHandler storyTellingManagerRound3;
    public StoryTellingHandler storyTellingManagerRound5;
    public CardCollector cardCollector;
    public EnemyCardManager enemyCardManager;
    public EndGameHandler endGameManager;
    public EndGameHandler winJManager;
    public Button qaButton;
    public Button collectButton;
    public Button PropButton;
    public ProtectUseButtonHandler protectUseButtonHandler;
    public RemindHandler remindHandler;

    [Header("Round Display")]
    public Text roundInfoText;

    [Header("Monster Betting Amounts per Round")]
    public int monsterBet = 100;

    [Header("Game Settings")]
    public int maxRounds = 5;
    public int betStep = 50; // 每次加注的步长

    [Header("Boss Betting")]
    public double maxPossibleScore = 41.2;
    public double minPossibleScore = 0;
    public double minBetRatio = 0.1;
    public double maxBetRatio = 0.5;
    public int maxChipsThreshold = 10000;
    public int stage2DemonTotalChipsCorrect = 8000;
    public int stage2DemonTotalChipsWrong = 10000;
    public int stage2PlayerChipsRemove = 1000;
    public int HeartAddLevel1 = 100;
    public int HeartAddLevel2 = 300;
    public int HeartAddLevel3 = 500;

    [Header("UI Settings")]
    public GameObject LoseGameOverPanel;
    public GameObject WinGameOverPanel;

    public LoadSceneController loadSceneController; 
    public Text restCostText;
    public ToolCardManager toolCardManager;
    public int currentRound = 0;
    private int baseBet = 0;
    private bool isDoubleChip = false;
    private bool isHalfChip = false;
    private int RestCost = 3;
    private bool isPlayerForceToAllIn = false;
    private double bossHandScore;
    private int bossTotalChips;
    public bool IsJoker = false;
    public TooltipManager tooltipManager;
    public Text playerWinText;  // 玩家胜利文本
    public Text playerLossText;  // 玩家失败文本
    public Text playerTieText;  // 回合结果文本
    public bool isProtectUsed = false;
    private int stage2DemonTotalChips;
    private int maxBet;
    private bool isWin = false;

    // 发牌或发牌之前调用
    public void InitializeGame()
    {
        changeStageManager.gameObject.SetActive(false);
        storyTellingManagerRound1.gameObject.SetActive(false);
        storyTellingManagerRound3.gameObject.SetActive(false);
        storyTellingManagerRound5.gameObject.SetActive(false);

        if (playerBettingSystem == null || demonBettingSystem == null)
        {
            Debug.LogError("Betting systems not assigned!");
            return;
        }
        currentRound = 0;
        // 初始化Cost
        if (IsJoker)
        {
            RestCost = 4;
            UpdateJokerRoundText();
        }
        else
        {
            RestCost = 3;
            UpdateRoundText();
        }        
        restCostText.text = "玩家剩余行动点：" + RestCost.ToString();
        demonBettingSystem.InitializeDemon();
        playerBettingSystem.InitializePlayer();
        UpdateUIStates(false, false, false, false); 
    }

    private void Start()
    {
        // 在Start或Awake方法中绑定按钮点击事件
        qaButton.onClick.AddListener(() => StartCoroutine(ShowQASessionWithDelay()));
    }

    //重置cost
    public void ResetCost(bool isJoker)
    {
        IsJoker = isJoker;
        if (IsJoker)
        {
            RestCost = 4;
        }
        else
        {
            RestCost = 3;
        }
        restCostText.text = "玩家剩余行动点：" + RestCost.ToString();

        Debug.Log("currentRound: " + currentRound);
    }

    // 发牌之后调用
    public void StartMonsterBettingRound()
    {
        maxBet = Math.Min(playerBettingSystem.GetPlayerTotalChips(), demonBettingSystem.GetDemonTotalChips());

        if (IsJoker)
        {
            bossHandScore = enemyCardManager.GetBestScore();
            bossTotalChips = demonBettingSystem.GetDemonTotalChips();
            monsterBet = CalculateBossBet(bossHandScore, bossTotalChips);
            Debug.Log("MonsterBet: " + monsterBet);

            baseBet = monsterBet;
            demonBettingSystem.SetDemonBet(baseBet);

            if (demonBettingSystem.GetDemonTotalChips() - baseBet <= 0)
            {
                demonBettingSystem.SetDemonBet(demonBettingSystem.GetDemonTotalChips());
                Debug.Log("Due to the lack of Boss chips, Boss all in");
                demonBettingSystem.SetDemonTotalChips(0);
            }
            else
            {
                Debug.Log("Decrease demon total chips" + baseBet);
                demonBettingSystem.DemonTotalChipsChange(-baseBet);
            }
        }
        else
        {
            // 设置怪物基础下注
            if (currentRound < maxRounds)
            {
                bossHandScore = enemyCardManager.GetBestScore();
                bossTotalChips = 1500;
                monsterBet = CalculateBossBet(bossHandScore, bossTotalChips);
                Debug.Log("MonsterBet: " + monsterBet);
            }
            else
            {
                bossHandScore = enemyCardManager.GetBestScore();
                bossTotalChips = demonBettingSystem.GetDemonTotalChips();
                monsterBet = CalculateBossBet(bossHandScore, bossTotalChips);
                Debug.Log("MonsterBet: " + monsterBet);
            }

            baseBet = monsterBet;
            demonBettingSystem.SetDemonBet(baseBet);
            if (currentRound >= maxRounds)
            {
                if (demonBettingSystem.GetDemonTotalChips() - baseBet <= 0)
                {
                    demonBettingSystem.SetDemonBet(demonBettingSystem.GetDemonTotalChips());
                    Debug.Log("Due to the lack of Boss chips, Boss all in");
                    demonBettingSystem.SetDemonTotalChips(0);
                }
                else
                {
                    Debug.Log("Decrease demon total chips" + baseBet);
                    demonBettingSystem.DemonTotalChipsChange(-baseBet);
                }
            }
        }

        // 玩家自动跟注且更新玩家最小跟注量
        if (playerBettingSystem.GetPlayerTotalChips() <= baseBet)
        {
            playerBettingSystem.SetPlayerBet(playerBettingSystem.GetPlayerTotalChips());
            isPlayerForceToAllIn = true;
            Debug.Log("玩家all in了");
        }
        else
        {
            // 玩家跟注，使用怪物当前下注量
            playerBettingSystem.SetPlayerBet(baseBet);
            Debug.Log($"Player follows bet: {baseBet}");
        }
        playerBettingSystem.UpdateUI();

        playerBettingSystem.minBet = baseBet;
        Debug.Log($"Round {currentRound + 1}: Monster first bets {baseBet}");

        // 如果是第一局显示提示
        if (currentRound == 0 && !IsJoker)
        {
            remindHandler.InitializeRemind();
        }

        // 改变按钮状态
        UpdateUIStates(true, true, false, true);

        // 重置ToolCardManager中的所有UseButton为可点击状态
        toolCardManager.ResetUseButtons();
        PropButton.interactable = true;
    }

    public void PlayerIncreaseBet()
    {
        Debug.Log("MaxBet: " + maxBet);
        // 玩家增加下注
        int newBet = playerBettingSystem.GetPlayerBet() + betStep;
        Debug.Log("newBet: " + newBet);

        int newTotalChips = playerBettingSystem.GetPlayerTotalChips() - betStep;
        if (newBet >= maxBet)
        {
            newBet = maxBet;
            playerBettingSystem.increaseButton.interactable = false;
            playerBettingSystem.decreaseButton.interactable = true;
            Debug.Log("超过任意一方总筹码量了");
        }

        if (newTotalChips <= 0)
        {
            playerBettingSystem.SetPlayerBet(playerBettingSystem.GetPlayerTotalChips() + playerBettingSystem.GetPlayerBet());
            playerBettingSystem.increaseButton.interactable = false;
            playerBettingSystem.decreaseButton.interactable = true;
            Debug.Log("玩家all in了");
        }
        else
        {
            playerBettingSystem.SetPlayerBet(newBet);
            playerBettingSystem.decreaseButton.interactable = true;
        }

        DemonFollowBet();
    }

    public void PlayerDecreaseBet()
    {
        // 玩家减少下注，但不能低于怪物当前下注
        int newBet = playerBettingSystem.GetPlayerBet() - betStep;

        if (newBet < baseBet)
        {
            playerBettingSystem.decreaseButton.interactable = false;
            playerBettingSystem.increaseButton.interactable = true;
            Debug.Log("试图设置的下注额低于怪物当前下注！");
            return;
        }
        else
        {
            playerBettingSystem.SetPlayerBet(newBet);
            playerBettingSystem.increaseButton.interactable = true;
        }

        DemonFollowBet();
    }

    public void PlayerAllIn()
    {
        // int allInBet = maxBet;
        int allInBet = Math.Min(maxBet, playerBettingSystem.GetPlayerTotalChips() + playerBettingSystem.GetPlayerBet());
        if (currentRound < maxRounds) {
            playerBettingSystem.SetPlayerBet(allInBet);
        }
        else{
            if (playerBettingSystem.GetPlayerTotalChips() + playerBettingSystem.GetPlayerBet() <= baseBet) {
                isPlayerForceToAllIn = true;
                playerBettingSystem.SetPlayerBet(allInBet);
                Debug.Log("玩家被动all in了");
            }
            else{
                playerBettingSystem.SetPlayerBet(allInBet);
                Debug.Log("玩家迫使恶魔all in了");
            }
        }

        DemonFollowBet();
    }

    public IEnumerator OpenCardsCoroutine()
    {
        int realPlayerTotalChips = playerBettingSystem.GetPlayerTotalChips() + playerBettingSystem.GetPlayerBet();
        int realDemonTotalChips = demonBettingSystem.GetDemonTotalChips() + demonBettingSystem.GetDemonBet();

        Debug.Log("realPlayerTotalChips: " + realPlayerTotalChips);
        Debug.Log("realDemonTotalChips: " + realDemonTotalChips);

        Debug.Log("isPlayerForceToAllIn: " + isPlayerForceToAllIn);
        Debug.Log("玩家下注：" + playerBettingSystem.GetPlayerBet());
        Debug.Log("恶魔下注：" + demonBettingSystem.GetDemonBet());

        if (IsJoker)
        {
            // 获取比牌结果
            CardComparisonManager.MatchResult result = cardComparisonManager.matchResult;
            Debug.Log("CardComparisonManager matchResult: " + cardComparisonManager.matchResult);

            if (result == CardComparisonManager.MatchResult.Victory)
            {
                if (isPlayerForceToAllIn)
                {
                    // Player筹码变化
                    int winnings = (int)(realDemonTotalChips * 0.5f);
                    playerBettingSystem.AddChips(playerBettingSystem.GetPlayerBet() + winnings);
                    isPlayerForceToAllIn = false;

                    playerWinText.text = $"玩家触底All in获胜，获得恶魔总筹码的一半";
                    Debug.Log("Player wins! Gains " + winnings + " chips");

                    // Demon筹码变化
                    demonBettingSystem.AddChips(demonBettingSystem.GetDemonBet() - winnings);
                    Debug.Log("Boss loses! Loses " + winnings + " chips");
                }
                else
                {
                    Debug.Log("到这里了");
                    Debug.Log("playerBettingSystem.GetPlayerBet(): " + playerBettingSystem.GetPlayerBet());
                    Debug.Log("demonBettingSystem.GetDemonBet(): " + demonBettingSystem.GetDemonBet());

                    // Player筹码变化
                    int winnings = playerBettingSystem.GetPlayerBet() * (isDoubleChip ? 2 : 1);

                    Debug.Log("winnings: " + winnings);
                    playerBettingSystem.AddChips(playerBettingSystem.GetPlayerBet() + winnings);

                    playerWinText.text = $"玩家获胜！获得{winnings}筹码";
                    Debug.Log($"Player wins! Gains {winnings} chips");

                    // Demon筹码变化
                    demonBettingSystem.AddChips(demonBettingSystem.GetDemonBet() - winnings);
                    Debug.Log("Boss loses! Loses " + winnings + " chips");
                }
            }
            else if (result == CardComparisonManager.MatchResult.Defeat)
            {
                // 判断玩家是否全押且isProtectUsed为false，且isHalfChip为false
                if (playerBettingSystem.GetPlayerTotalChips() == 0 && !isProtectUsed && !isHalfChip)
                {
                    // 玩家全押且isProtectUsed为false时，返还90%的筹码
                    int losses = (int)Mathf.Floor(playerBettingSystem.GetPlayerBet() * (isHalfChip ? 0.5f : 1f) * 0.9f);
                    playerBettingSystem.AddChips(playerBettingSystem.GetPlayerBet() - losses);

                    Debug.Log($"Player loses! Loses {losses} chips (90% loss due to all-in)");
                    playerLossText.text = $"玩家All In失败！守护符触发返还{playerBettingSystem.GetPlayerBet() - losses}筹码";

                    // 设置isProtectUsed为true，防止下次继续扣除90%
                    isProtectUsed = true;
                    protectUseButtonHandler.HideButtonWithAnimation();

                    // Demon筹码变化
                    demonBettingSystem.AddChips(demonBettingSystem.GetDemonBet() + losses);
                    Debug.Log("Boss wins! Gains " + losses + " chips");
                }
                else
                {
                    // 玩家未全押，或已经全押并且isProtectUsed为true，或者isHalfChip为true
                    int losses = (int)Mathf.Floor(playerBettingSystem.GetPlayerBet() * (isHalfChip ? 0.5f : 1f));
                    playerBettingSystem.AddChips(playerBettingSystem.GetPlayerBet() - losses);

                    Debug.Log($"Player loses! Loses {losses} chips");
                    playerLossText.text = $"玩家失败！损失{losses}筹码";

                    // Demon筹码变化
                    demonBettingSystem.AddChips(demonBettingSystem.GetDemonBet() + losses);
                    Debug.Log("Boss wins! Gains " + losses + " chips");
                }
            }
            else
            {
                // Player筹码变化
                playerBettingSystem.AddChips(playerBettingSystem.GetPlayerBet());
                Debug.Log("It's a tie!");
                playerTieText.text = "平局！不获得额外筹码";
                demonBettingSystem.AddChips(demonBettingSystem.GetDemonBet());
            }

            // 计算红心数对恶魔筹码的影响
            int heartsCount = tooltipManager.hearts_count;
            if (heartsCount > 5 && heartsCount <= 8){
                demonBettingSystem.AddChips(HeartAddLevel1);
                playerWinText.text += $"\n 红心之愈，恶魔回复{HeartAddLevel1}筹码";
                playerLossText.text += $"\n 红心之愈，恶魔回复{HeartAddLevel1}筹码";
                playerTieText.text += $"\n 红心之愈，恶魔回复{HeartAddLevel1}筹码";
            }
            else if (heartsCount > 8 && heartsCount <= 11){
                demonBettingSystem.AddChips(HeartAddLevel2);
                playerWinText.text += $"\n 红心之愈，恶魔回复{HeartAddLevel2}筹码";
                playerLossText.text += $"\n 红心之愈，恶魔回复{HeartAddLevel2}筹码";
                playerTieText.text += $"\n 红心之愈，恶魔回复{HeartAddLevel2}筹码";
            }
            else if (heartsCount > 11){
                demonBettingSystem.AddChips(HeartAddLevel3);
                playerWinText.text += $"\n 红心之愈，恶魔回复{HeartAddLevel3}筹码";
                playerLossText.text += $"\n 红心之愈，恶魔回复{HeartAddLevel3}筹码";
                playerTieText.text += $"\n 红心之愈，恶魔回复{HeartAddLevel3}筹码";
            }
        }
        else
        {
            // 获取比牌结果
            CardComparisonManager.MatchResult result = cardComparisonManager.matchResult;
            Debug.Log("CardComparisonManager matchResult: " + cardComparisonManager.matchResult);

            if (result == CardComparisonManager.MatchResult.Victory)
            {
                if (currentRound < maxRounds)
                {
                    // Player筹码变化
                    int winnings = playerBettingSystem.GetPlayerBet() * (isDoubleChip ? 2 : 1);
                    playerBettingSystem.AddChips(playerBettingSystem.GetPlayerBet() + winnings);
                    Debug.Log($"Player wins! Gains {winnings} chips");
                    playerWinText.text = $"玩家获胜！获得{winnings}筹码";
                }
                else
                {
                    if (isPlayerForceToAllIn)
                    {
                        // Player筹码变化
                        int winnings = (int)(realDemonTotalChips * 0.5f);
                        playerBettingSystem.AddChips(playerBettingSystem.GetPlayerBet() + winnings);
                        isPlayerForceToAllIn = false;
                        playerWinText.text = $"玩家触底All in获胜，获得恶魔总筹码的一半";
                        Debug.Log("Player wins! Gains " + winnings + " chips");
                        // Demon筹码变化
                        demonBettingSystem.AddChips(playerBettingSystem.GetPlayerBet() - winnings);
                        Debug.Log("Boss loses! Loses " + winnings + " chips");
                    }
                    else
                    {
                        // Player筹码变化
                        int winnings = playerBettingSystem.GetPlayerBet() * (isDoubleChip ? 2 : 1);
                        playerBettingSystem.AddChips(playerBettingSystem.GetPlayerBet() + winnings);
                        playerWinText.text = $"玩家获胜！获得{winnings}筹码";
                        Debug.Log($"Player wins! Gains {winnings} chips");
                        // Demon筹码变化
                        demonBettingSystem.AddChips(playerBettingSystem.GetPlayerBet() - winnings);
                        Debug.Log("Boss loses! Loses " + winnings + " chips");
                    }
                }
            }
            else if (result == CardComparisonManager.MatchResult.Defeat)
            {
                if (currentRound < maxRounds)
                {
                    // 判断玩家是否全押且isProtectUsed为false
                    if (playerBettingSystem.GetPlayerTotalChips() == 0 && !isProtectUsed && !isHalfChip)
                    {
                        // 玩家全押且isProtectUsed为false时，只扣除90%的筹码
                        int losses = (int)Mathf.Floor(playerBettingSystem.GetPlayerBet() * (isHalfChip ? 0.5f : 1f) * 0.9f);
                        playerBettingSystem.AddChips(playerBettingSystem.GetPlayerBet() - losses);
                        Debug.Log($"Player loses! Loses {losses}");
                        playerLossText.text = $"玩家All In失败！守护符触发返还{playerBettingSystem.GetPlayerBet() - losses}筹码";

                        // 设置isProtectUsed为true，防止下次继续扣除90%
                        isProtectUsed = true;
                        protectUseButtonHandler.HideButtonWithAnimation();
                    }
                    else
                    {
                        // 玩家未全押，或已经全押并且isProtectUsed为true
                        int losses = (int)Mathf.Floor(playerBettingSystem.GetPlayerBet() * (isHalfChip ? 0.5f : 1f));
                        playerBettingSystem.AddChips(playerBettingSystem.GetPlayerBet() - losses);
                        Debug.Log($"Player loses! Loses {losses} chips");
                        playerLossText.text = $"玩家失败！损失{losses}筹码";
                    }
                }
                else
                {
                    // 处理最终回合的逻辑
                    Debug.Log("playerBettingSystem.GetPlayerBet(): " + playerBettingSystem.GetPlayerBet());

                    // 判断玩家是否全押且isProtectUsed为false
                    if (playerBettingSystem.GetPlayerTotalChips() == 0 && !isProtectUsed && !isHalfChip)
                    {
                        // 玩家全押且isProtectUsed为false时，只扣除90%的筹码
                        int losses = (int)Mathf.Floor(playerBettingSystem.GetPlayerBet() * (isHalfChip ? 0.5f : 1f) * 0.9f);
                        playerBettingSystem.AddChips(playerBettingSystem.GetPlayerBet() - losses);
                        Debug.Log($"Player loses! Loses {losses} chips");
                        playerLossText.text = $"玩家All In失败！守护符触发返还{playerBettingSystem.GetPlayerBet() - losses}筹码";

                        // 设置isProtectUsed为true，防止下次继续扣除90%
                        isProtectUsed = true;
                        protectUseButtonHandler.HideButtonWithAnimation();

                        // Demon筹码变化
                        demonBettingSystem.AddChips(demonBettingSystem.GetDemonBet() + losses);
                        Debug.Log("Boss wins! Gains " + losses + " chips");
                    }
                    else
                    {
                        // 玩家未全押，或已经全押并且isProtectUsed为true
                        int losses = (int)Mathf.Floor(playerBettingSystem.GetPlayerBet() * (isHalfChip ? 0.5f : 1f));
                        playerBettingSystem.AddChips(playerBettingSystem.GetPlayerBet() - losses);
                        Debug.Log($"Player loses! Loses {losses} chips");
                        playerLossText.text = $"玩家失败！损失{losses}筹码";

                        // Demon筹码变化
                        demonBettingSystem.AddChips(demonBettingSystem.GetDemonBet() + losses);
                        Debug.Log("Boss wins! Gains " + losses + " chips");
                    }
                }
            }
            else
            {
                playerBettingSystem.AddChips(playerBettingSystem.GetPlayerBet());
                Debug.Log("It's a tie!");
                playerTieText.text = "平局！不获得额外筹码";
                if (currentRound >= maxRounds)
                {
                    demonBettingSystem.AddChips(playerBettingSystem.GetPlayerBet());
                }
            }
        }

        playerBettingSystem.ResetPlayerBet();
        demonBettingSystem.SetDemonBet(0);

        isDoubleChip = false;
        isHalfChip = false;

        playerBettingSystem.UpdateUI();

        // 等待筹码更新协程完成
        while (playerBettingSystem.isUpdatingChips)
        {
            yield return null; // 每帧检查一次
        } 

        qaButton.gameObject.SetActive(true);

        if (!playerBettingSystem.isUpdatingChips && playerBettingSystem.GetPlayerTotalChips() <= 0)
        Debug.Log("玩家：" + playerBettingSystem.GetPlayerTotalChips());

        if (playerBettingSystem.GetPlayerTotalChips() <= 0)
        {
            isWin = false;
            HandleZeroChips(isWin);

            Debug.Log("玩家没筹码了，游戏结束");
            // return;
        }

        if (demonBettingSystem.GetDemonTotalChips() <= 0 && (IsJoker || currentRound >= maxRounds))
        {
            isWin = true;
            HandleZeroChips(isWin);

            Debug.Log("恶魔没筹码了，游戏结束");
            // return;
        }
        Debug.Log("恶魔筹码：" + demonBettingSystem.GetDemonTotalChips());
        
        if (IsJoker)
        {
            // 设置按钮为可见
            tooltipManager.heartsCountText.gameObject.SetActive(true);
            tooltipManager.diamondsCountText.gameObject.SetActive(true);
            tooltipManager.hearts.gameObject.SetActive(true);
            tooltipManager.diamonds.gameObject.SetActive(true);
        }
        // 准备下一局
        playerBettingSystem.ClearPlayerBet();
        demonBettingSystem.ClearDemonBet();
        UpdateUIStates(false, false, false, false);
        yield break;
    }

    public void DemonFollowBet()
    {
        // 获取玩家当前下注额
        int playerBet = playerBettingSystem.GetPlayerBet();
        
        // 如果当前回合是阶段二（currentRound >= maxRounds）或者 IsJoker 为 true
        if (currentRound >= maxRounds || IsJoker)
        {
            // 计算需要跟注的差额
            int followAmount = playerBet - demonBettingSystem.GetDemonBet();

            // 如果恶魔的剩余筹码不足以跟注，则全押
            if (demonBettingSystem.GetDemonTotalChips() <= followAmount)
            {
                demonBettingSystem.SetDemonBet(demonBettingSystem.GetDemonBet() + demonBettingSystem.GetDemonTotalChips());
                demonBettingSystem.SetDemonTotalChips(0);
                Debug.Log("恶魔全押！剩余筹码: 0");
            }
            else
            {
                // 正常跟注，更新下注额和总筹码
                demonBettingSystem.SetDemonBet(playerBet);
                demonBettingSystem.DemonTotalChipsChange(-followAmount);
                Debug.Log($"恶魔跟注: {followAmount}, 剩余筹码: {demonBettingSystem.GetDemonTotalChips()}");
            }
        }
        else
        {
            // 阶段一（前5回合），怪物直接匹配玩家下注（总筹码不受影响）
            demonBettingSystem.SetDemonBet(playerBet);
            Debug.Log($"恶魔匹配下注: {playerBet}");
        }
    }

    public void UseDoubleChip()
    {
        isDoubleChip = true;
    }

    public void UseHalfChip()
    {
        isHalfChip = true;
    }

    public void DecreasePlayerTotalChips(int amount)
    {
        playerBettingSystem.DecreasePlayerTotalChips(amount);
    }

    public void DecreaseRestCost(int amount)
    {
        RestCost -= amount;
        restCostText.text = "玩家剩余行动点：" + RestCost.ToString();
    }

    public int GetRestCost()
    {
        return RestCost;
    }

    public int CalculateStage2DemonTotalChips(int correctClueCount)
    {
        int stage2DemonTotalChips;

        switch (correctClueCount)
        {
            case 0:
                stage2DemonTotalChips = 10000;
                break;
            case 1:
                stage2DemonTotalChips = 9999;
                break;
            case 2:
                stage2DemonTotalChips = 8888;
                break;
            case 3:
                stage2DemonTotalChips = 6666;
                break;
            case 4:
                stage2DemonTotalChips = 5555;
                break;
            case 5:
                stage2DemonTotalChips = 4444;
                break;
            default:
                stage2DemonTotalChips = 10000;
                break;
        }
        return stage2DemonTotalChips;
    }

    public bool GetIsPlayerForceToAllIn()
    {
        return isPlayerForceToAllIn;
    }

    public int CalculateBossBet(double handScore, int currentChips)
    {
        // 连续化基础比例
        double baseBetRatio = CalculateBaseBetRatio(handScore);
        
        // 连续化动态调整
        double dynamicAdjustment = CalculateDynamicAdjustment(currentChips);
        
        // 最终下注计算
        double theoreticalBet = currentChips * baseBetRatio * dynamicAdjustment;
        int actualBet = AdjustTo25Multiple(theoreticalBet, currentChips);
        
        Debug.Log($"Boss下注: 手牌={handScore:F1}, 筹码={currentChips}, 比例={baseBetRatio:F2}, 调整={dynamicAdjustment:F2}, 下注={actualBet}");
        return actualBet;
    }

    private double CalculateBaseBetRatio(double handScore)
    {
        // 使用Sigmoid函数实现平滑过渡 (范围映射到0.1~0.9)
        double normalizedScore = (handScore - minPossibleScore) / (maxPossibleScore - minPossibleScore);
        double sigmoid = 1 / (1 + Math.Exp(-12 * (normalizedScore - 0.5))); // 调整12控制曲线陡峭度
        return minBetRatio + (maxBetRatio - minBetRatio) * sigmoid;
    }

    private double CalculateDynamicAdjustment(int currentChips)
    {
        // 使用对数函数实现筹码敏感度递减 (筹码越多，调整幅度越小)
        double logChips = Math.Log(currentChips + 1); // +1防止log(0)
        double maxLog = Math.Log(maxChipsThreshold + 1);
        double normalized = logChips / maxLog;
        
        // 幂函数控制曲线形状 (0.5表示平方根，使低筹码时更敏感)
        return Math.Pow(normalized, 0.5); 
    }

    // 调整为25的整数倍并确保不超过当前筹码
    private int AdjustTo25Multiple(double amount, int currentChips)
    {
        // 计算最接近的25倍数
        int roundedBet = (int)(Math.Round(amount / 25) * 25);
        
        // 确保不超过当前筹码量
        roundedBet = Math.Min(roundedBet, currentChips);
        
        // 确保不低于最小下注额
        return Math.Max(roundedBet, 25);
    }

    private IEnumerator ShowQASessionWithDelay()
    {
        // 隐藏按钮
        qaButton.gameObject.SetActive(false);
        // 设置按钮为不可见
        tooltipManager.heartsCountText.gameObject.SetActive(false);
        tooltipManager.diamondsCountText.gameObject.SetActive(false);
        tooltipManager.hearts.gameObject.SetActive(false);
        tooltipManager.diamonds.gameObject.SetActive(false);
        cardComparisonManager.CloseAllPanelsAndEffects();
        Debug.Log("currentRound: " + currentRound);

        if (IsJoker) // 如果IsJoker为true
        {
            // 跳过Q&A面板，直接开始收集卡牌
            cardCollector.StartCollectCards();
            currentRound++;
            UpdateJokerRoundText();
            Debug.Log("currentRound (IsJoker): " + currentRound);
        }

        else if (currentRound == 0)
        {
            storyTellingManagerRound1.gameObject.SetActive(true);
            storyTellingManagerRound1.InitializeStoryTelling();
            currentRound++;
            Debug.Log("currentRound: " + currentRound);
        }
        else if (currentRound == 2)
        {
            storyTellingManagerRound3.gameObject.SetActive(true);
            storyTellingManagerRound3.InitializeStoryTelling();
            currentRound++;
            Debug.Log("currentRound: " + currentRound);
        }
        else if (currentRound == 4)
        {
            storyTellingManagerRound5.gameObject.SetActive(true);
            storyTellingManagerRound5.InitializeStoryTelling();

            // 等待故事播放完成
            yield return new WaitUntil(() => storyTellingManagerRound5.isDone);

            changeStageManager.gameObject.SetActive(true);
            changeStageManager.InitializeChangeStage();
            currentRound++;
            Debug.Log("currentRound: " + currentRound);

            yield return new WaitUntil(() => changeStageManager.isDone);

            bool isCorrectAnswer = changeStageManager.GetIsCorrectAnswer();
            if (isCorrectAnswer)
            {
                if (playerBettingSystem.GetPlayerTotalChips() <= 6000)
                {
                    stage2DemonTotalChips = stage2DemonTotalChipsCorrect;
                }
                else
                {
                    int playerChips = playerBettingSystem.GetPlayerTotalChips();
                    stage2DemonTotalChips = (int)(Mathf.Round((playerChips * 1.4f) / 100f) * 100);
                }
            }
            else
            {
                if (playerBettingSystem.GetPlayerTotalChips() <= 6000)
                {
                    stage2DemonTotalChips = stage2DemonTotalChipsWrong;
                }
                else
                {
                    int playerChips = playerBettingSystem.GetPlayerTotalChips();
                    stage2DemonTotalChips = (int)(Mathf.Round((playerChips * 1.4f) / 100f) * 100) + 2000;
                }
                playerBettingSystem.RemoveChips(stage2PlayerChipsRemove);
            }
            demonBettingSystem.SetDemonTotalChips(stage2DemonTotalChips);

            yield return new WaitUntil(() => playerBettingSystem.isDone);

            if (playerBettingSystem.GetPlayerTotalChips() <= 0)
            {
                isWin = false;

                HandleZeroChips(isWin);
            }
        }
        else
        {
            currentRound++;
            UpdateRoundText();
            Debug.Log("currentRound: " + currentRound);
            cardCollector.StartCollectCards();
            // currentRound++;
        }
        yield break;
    }

    public void UpdateRoundText()
    {
        if (roundInfoText != null)
        {
            // // 显示当前回合 (currentRound从0开始，显示时加1)
            if (currentRound < 5)
            {
                roundInfoText.text = $"Round {currentRound + 1}/5";
            }
            else
            {
                roundInfoText.text = $"Round {currentRound - 4}";
            }
        }
        else
        {
            Debug.LogWarning("Round display text not assigned!");
        }
    }

    public void UpdateJokerRoundText()
    {
        if (roundInfoText != null)
        {
            roundInfoText.text = $"Round {currentRound + 1}";
        }
        else
        {
            Debug.LogWarning("Round display text not assigned!");
        }
    }

    void UpdateUIStates(bool incActive, bool decActive, bool openActive, bool allInActive)
    {
        playerBettingSystem.increaseButton.interactable = incActive;
        playerBettingSystem.decreaseButton.interactable = decActive;
        playerBettingSystem.openCardButton.interactable = openActive;
        playerBettingSystem.allInButton.interactable = allInActive;
    }

    void HandleZeroChips(bool isWin)
    {
        Debug.Log("玩家没筹码了，游戏结束");
        playerBettingSystem.ClearPlayerBet();
        demonBettingSystem.ClearDemonBet();
        qaButton.gameObject.SetActive(false);
        collectButton.gameObject.SetActive(false);
        UpdateUIStates(false, false, false, false);
        StartCoroutine(HandleGameOver(isWin));
        // return;
    }
    private IEnumerator HandleGameOver(bool isWin)
    {
        // 等待3秒
        yield return new WaitForSeconds(6f);

        if (isWin)
        {
            WinGameOverPanel.SetActive(true);
        }
        else
        {
            LoseGameOverPanel.SetActive(true);  
        }

        // 等待3秒
        yield return new WaitForSeconds(2f);

        if (isWin)
        {
            if (IsJoker)
            {
                // 加载下一场景
                LoadSceneController loadSceneController = new LoadSceneController();
                loadSceneController.nextSceneName = "Win"; 
                loadSceneController.LoadNextScene();
            }
            else
            {
                winJManager.gameObject.SetActive(true);
                winJManager.InitializeEndGame();
                yield return new WaitUntil(() => winJManager.isDone);

                // 加载下一场景
                LoadSceneController loadSceneController = new LoadSceneController();
                loadSceneController.nextSceneName = "J_Joker";  // 设置目标场景名为 Login
                loadSceneController.LoadNextScene();
            }
        }
        else
        {
            endGameManager.gameObject.SetActive(true);
            endGameManager.InitializeEndGame();
            yield return new WaitUntil(() => endGameManager.isDone);

            // 加载下一场景（Login）
            LoadSceneController loadSceneController = new LoadSceneController();
            loadSceneController.nextSceneName = "Login";  // 设置目标场景名为 Login
            loadSceneController.LoadNextScene();
        }
    }
}
