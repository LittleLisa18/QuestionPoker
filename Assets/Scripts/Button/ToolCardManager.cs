using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolCardManager : MonoBehaviour
{
    // 0: Peek; 1: Swap; 2: +1; 3: -1; 4: Double; 5: Half; 6: Suit; 7: Reverse
    [Header("UI设置")]
    // 前4张卡的Buy按钮
    public Button[] BuyButtons;
    // 前4张卡的Use按钮，后2张卡是BuyAndUse按钮
    public Button[] UseButtons;

    // 所有卡的购买所需要的筹码数量
    public int[] toolCardPrices;
    // 所有卡的购买所需要的Cost
    public int[] toolCardCosts;

    public BettingManager bettingManager;

    // 控制是否执行Update的标志
    public bool shouldUpdate = true;

    // Start is called before the first frame update
    void Start()
    {
        // 一开始禁用前1-4张的Use按钮
        for (int i = 0; i < BuyButtons.Length; i++)
        {
            UseButtons[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < BuyButtons.Length; i++)
        {
            int index = i;
            BuyButtons[i].onClick.AddListener(() => BuyToolCardFirstFourCards(index));
        }

        for (int i = 0; i < 4; i++)
        {
            int index = i;
            UseButtons[i].onClick.AddListener(() => DecreaseCost(index));
        }

        for (int i = 4; i < UseButtons.Length; i++)
        {
            int index = i;
            UseButtons[i].onClick.AddListener(() => BuyToolCardSecondFourCards(index));
        }
    }
    void Update()
    {
        if (!shouldUpdate) return;  // 如果 shouldUpdate 为 false，直接跳过 Update 方法

        else
        {
            // 获取当前资源
            int currentChips = bettingManager.playerBettingSystem.GetPlayerTotalChips();
            int currentCost = bettingManager.GetRestCost();

            // 更新前4张卡的Buy按钮状态
            for (int i = 0; i < BuyButtons.Length; i++)
            {
                // 只有未购买的卡才需要检查Buy按钮
                if (UseButtons[i].gameObject.activeSelf == false)
                {
                    BuyButtons[i].interactable = (currentChips >= toolCardPrices[i]);
                }
            }

            // 更新所有Use按钮状态（包括后2张卡的BuyAndUse按钮）
            for (int i = 0; i < UseButtons.Length; i++)
            {
                // 前4张卡需要已购买且Cost足够
                if (i < 4) 
                {
                    UseButtons[i].interactable = UseButtons[i].gameObject.activeSelf && 
                                            (currentCost >= toolCardCosts[i]);
                }
                // 后2张卡需要同时满足筹码和Cost
                else 
                {
                    UseButtons[i].interactable = (currentChips >= toolCardPrices[i]) && 
                                                (currentCost >= toolCardCosts[i]);
                }
            }
        }
    }

    public void BuyToolCardFirstFourCards(int index)
    {
        if (bettingManager.playerBettingSystem.GetPlayerTotalChips() >= toolCardPrices[index])
        {
            bettingManager.DecreasePlayerTotalChips(toolCardPrices[index]);
            UseButtons[index].gameObject.SetActive(true);
            BuyButtons[index].gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Not enough chips");
        }
    }

    public void DecreaseCost(int index)
    {
        if (bettingManager.GetRestCost() >= toolCardCosts[index])
        {
            bettingManager.DecreaseRestCost(toolCardCosts[index]);
        }
        else
        {
            Debug.Log("Not enough cost");
        }
    }

    private void UpdateFirstFourUseButtons()
    {
        int remainingCost = bettingManager.GetRestCost(); // 获取当前剩余的成本

        // 遍历所有的UseButtons
        for (int i = 0; i < UseButtons.Length; i++)
        {
            // 如果某个Use按钮对应的卡牌的cost大于剩余资源，就禁用该按钮
            if (toolCardCosts[i] > remainingCost)
            {
                UseButtons[i].interactable = false; // 禁用按钮的点击
            }
            else
            {
                UseButtons[i].interactable = true;  // 启用按钮的点击
            }
        }
    }
    
    public void ResetUseButtons()
    {
        // 遍历前四个UseButtons并设置为可点击
        for (int i = 0; i < 4; i++)
        {
            UseButtons[i].interactable = true;  // 设置所有按钮为可点击
        }
    }

    public void BuyToolCardSecondFourCards(int index)
    {
        if (bettingManager.playerBettingSystem.GetPlayerTotalChips() >= toolCardPrices[index] && bettingManager.GetRestCost() >= toolCardCosts[index])
        {
            bettingManager.DecreasePlayerTotalChips(toolCardPrices[index]);
            bettingManager.DecreaseRestCost(toolCardCosts[index]);
        }
        else if (bettingManager.playerBettingSystem.GetPlayerTotalChips() < toolCardPrices[index])
        {
            Debug.Log("Not enough chips");
        }
        else if (bettingManager.GetRestCost() < toolCardCosts[index])
        {
            Debug.Log("Not enough cost");
        }
        else
        {
            Debug.Log("Not enough chips or cost");
        }
    }
    private void UpdateSecondFourUseButtons()
    {
        int remainingCost = bettingManager.GetRestCost(); // 获取当前剩余的成本
        int playerChips = bettingManager.playerBettingSystem.GetPlayerTotalChips();

        // 更新前4个购买按钮
        for (int i = 0; i < BuyButtons.Length; i++)
        {
            if (BuyButtons[i].gameObject.activeSelf)
            {
                BuyButtons[i].interactable = (playerChips >= toolCardPrices[i]);
            }
        }

        // 更新后4个购买使用按钮
        for (int i = 4; i < UseButtons.Length; i++)
        {
            UseButtons[i].interactable = (playerChips >= toolCardPrices[i] && 
                                         remainingCost >= toolCardCosts[i]);
        }
    }
}