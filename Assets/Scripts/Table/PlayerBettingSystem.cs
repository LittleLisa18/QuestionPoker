using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBettingSystem : MonoBehaviour
{
    [Header("UI References")]
    public Text playerTotalChipsText;
    public Text playerBetText;
    public Button increaseButton;
    public Button decreaseButton;
    public Button openCardButton;
    public Button allInButton;

    [Header("Settings")]
    public int initialPlayerChips = 1000;
    public int minBet = 0;
    public int betStep = 50;
    public bool isUpdatingChips; // 新增状态锁
    public int JokerInitialChips = 4000;
    public bool IsJoker = false;
    public bool isDone = false;

    [HideInInspector] private int playerCurrentTotalChips;
    [HideInInspector] private int playerCurrentBet;

    private BettingManager bettingManager;

    public void InitializePlayer()
    {
        // 获取BettingManager
        bettingManager = FindObjectOfType<BettingManager>();
        if (bettingManager == null)
        {
            Debug.LogError("BettingManager not found in the scene!");
            return;
        }

        // 初始化玩家筹码
        if (IsJoker)
        {
            playerCurrentTotalChips = JokerInitialChips;
        }
        else
        {
            playerCurrentTotalChips = initialPlayerChips;
        }
        playerCurrentBet = 0;

        // 设置按钮事件
        increaseButton.onClick.AddListener(PlayerIncreaseBet);
        decreaseButton.onClick.AddListener(PlayerDecreaseBet);
        allInButton.onClick.AddListener(PlayerAllIn);
        // openCardButton.onClick.AddListener(OpenCards);

        // 更新UI
        UpdateUI();
    }

    public void ClearPlayerBet()
    {
        playerCurrentBet = 0;
        UpdateUI();
    }

    void PlayerIncreaseBet()
    {
        bettingManager.PlayerIncreaseBet();
        UpdateUI();
    }

    void PlayerDecreaseBet()
    {
        bettingManager.PlayerDecreaseBet();
        UpdateUI();
    }

    void PlayerAllIn()
    {
        bettingManager.PlayerAllIn();
        UpdateUI();
    }

    void OpenCards()
    {
        // bettingManager.OpenCards();
        StartCoroutine(bettingManager.OpenCardsCoroutine());
        UpdateUI();
    }

    void Fold()
    {
        Debug.Log("玩家选择弃牌！");
        // 这里可以添加弃牌后的逻辑，例如结束游戏或跳过回合
        // bettingManager.HandleZeroChips();
    }
    
    // 设置玩家下注额(由游戏管理器调用)
    public void SetPlayerBet(int betAmount)
    {
        // 实时更新玩家的总筹码
        playerCurrentTotalChips -= (betAmount - playerCurrentBet);
        playerCurrentBet = betAmount;
        UpdateUI();
    }

    public int GetPlayerBet()
    {
        return playerCurrentBet;
    }

    public int GetPlayerTotalChips()
    {
        return playerCurrentTotalChips;
    }

    public void DecreasePlayerTotalChips(int amount)
    {
        playerCurrentTotalChips -= amount;
        UpdateUI();
    }
    
    public void UpdateUI()
    {
        playerTotalChipsText.text = $"玩家总筹码: {playerCurrentTotalChips}";
        playerBetText.text = $"玩家下注: {playerCurrentBet}";
    }

    public void ResetPlayerBet()
    {
        playerCurrentBet = 0;
        UpdateUI();
    }

    // public void AddChips(int amount)
    // {
    //     Debug.Log("玩家筹码：" + playerCurrentTotalChips);
    //     playerCurrentTotalChips += amount;
    //     Debug.Log("玩家筹码：" + playerCurrentTotalChips);
    //     UpdateUI();
    // }

    // // 移除筹码（输钱时调用）
    // public void RemoveChips(int amount)
    // {
    //     playerCurrentTotalChips = Mathf.Max(0, playerCurrentTotalChips - amount);
    //     UpdateUI();
    // }

    // 添加筹码（赢钱时调用）
    public void AddChips(int amount)
    {
        isDone = false;
        StartCoroutine(UpdateChipsAmount(playerCurrentTotalChips, playerCurrentTotalChips + amount, 3f, amount)); // 1秒动画
    }

    // 移除筹码（输钱时调用）
    public void RemoveChips(int amount)
    {
        isDone = false;
        StartCoroutine(UpdateChipsAmount(playerCurrentTotalChips, Mathf.Max(0, playerCurrentTotalChips - amount), 3f, amount)); // 1秒动画
    }

    // 更新筹码数值，带有平滑过渡效果
    private IEnumerator UpdateChipsAmount(int startValue, int endValue, float duration, int amount)
    {
        isUpdatingChips = true;
        float elapsedTime = 0f;
        Debug.Log("玩家筹码：" + endValue);

        while (elapsedTime < duration)
        {
            playerCurrentTotalChips = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, elapsedTime / duration)); // 平滑过渡
            UpdateUI();  // 更新UI显示
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerCurrentTotalChips = endValue; // 确保最终数值为目标值
        UpdateUI();  // 更新UI显示
        isUpdatingChips = false;
        isDone = true;
    }

    public void ShowFailureMessage()
    {
        // 示例：在控制台输出失败信息
        Debug.LogError("玩家失败！游戏结束。");

        // 如果有UI元素用于显示失败信息，可以在这里启用或更新它们
        // 例如：
        // if (failureText != null)
        // {
        //     failureText.text = "玩家失败！游戏结束。";
        //     failureText.gameObject.SetActive(true);
        // }
    }
}
