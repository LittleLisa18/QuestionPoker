using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBettingSystem : MonoBehaviour
{
    [Header("UI References")]
    public Text demonTotalChipsText;  // 恶魔总筹码文本
    public Text demonBetText;        // 恶魔下注文本

    [Header("Settings")]
    public int initialDemonChips = int.MaxValue; // 恶魔初始总筹码(设为无限大)

    [Header("Joker Settings")]
    public bool IsJoker = false;
    public int JokerinitialDemonChips = 5000; // 大王初始总筹码

    [HideInInspector] private int demonCurrentTotalChips;   // 恶魔当前总筹码
    [HideInInspector] private int demonCurrentBet;          // 恶魔当前下注额

    public void InitializeDemon()
    {
        if (IsJoker)
        {
            demonCurrentTotalChips = JokerinitialDemonChips;
        }
        else
        {
            demonCurrentTotalChips = initialDemonChips;
        }
        demonCurrentBet = 0;
        UpdateUI();
    }

    public void SetDemonTotalChips(int totalChips)
    {
        demonCurrentTotalChips = totalChips;
        UpdateUI();
    }

    // 设置恶魔下注额(由游戏管理器调用)
    public void SetDemonBet(int betAmount)
    {
        demonCurrentBet = betAmount;
        UpdateUI();
    }

    public void DemonTotalChipsChange(int totalChipsChange)
    {
        demonCurrentTotalChips += totalChipsChange;
        UpdateUI();
    }

    public int GetDemonBet()
    {
        return demonCurrentBet;
    }

    public int GetDemonTotalChips()
    {
        return demonCurrentTotalChips;
    }

    public void ClearDemonBet()
    {
        demonCurrentBet = 0;
        UpdateUI();
    }

    // 确认恶魔下注(由玩家确认按钮调用)
    public void ConfirmDemonBet()
    {
        // 恶魔不需要实际确认，因为它的下注总是跟随玩家
        Debug.Log($"恶魔确认下注: {demonCurrentBet}筹码");
    }

    // 更新恶魔UI显示
    public void UpdateUI()
    {
        if (demonCurrentTotalChips == int.MaxValue)
        {
            demonTotalChipsText.text = "恶魔总筹码: ∞"; // 显示无限符号
        }
        else
        {
            demonTotalChipsText.text = "恶魔总筹码: " + demonCurrentTotalChips.ToString();
        }
        demonBetText.text = $"恶魔下注: {demonCurrentBet}";
    }

    // 恶魔获得筹码（阶段二使用）
    public void AddChips(int amount)
    {
        demonCurrentTotalChips += amount;
        UpdateUI();
    }

    // 恶魔失去筹码（阶段二使用）
    public void RemoveChips(int amount)
    {
        demonCurrentTotalChips -= amount;
        UpdateUI();
    }
}
