using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDisabler : MonoBehaviour
{
    public BettingManager bettingManager; // 绑定BettingManager
    public Button[] buttons; // 绑定7个按钮
    public Button[] useButtons;
    public bool isJoker = false;
    private Button lastDisabledButton; // 上一轮禁用的按钮
    private int lastRound = -1; // 上一轮的值

    void Start()
    {
        if (buttons.Length != 7)
        {
            Debug.LogError("按钮数量必须是7个!");
        }
    }

    void Update()
    {
        if (isJoker)
        {
            return;
        }
        // 检查currentRound是否发生变化
        if (bettingManager.currentRound != lastRound && bettingManager.currentRound > 4)
        {
            // 更新上一轮值
            lastRound = bettingManager.currentRound;

            // 禁用一个随机按钮
            DisableRandomButton();
        }
    }

    // 随机禁用一个按钮
    private void DisableRandomButton()
    {
        // 如果当前有按钮被禁用，则恢复上一个禁用的按钮
        if (lastDisabledButton != null)
        {
            lastDisabledButton.interactable = true;
        }

        // 随机选择一个按钮来禁用
        int index = Random.Range(0, buttons.Length);
        Button buttonToDisable = buttons[index];

        // 禁用按钮
        buttonToDisable.interactable = false;
        lastDisabledButton = buttonToDisable;
    }
    // 禁用所有按钮
    public void DisableAllButtons()
    {
        foreach (Button button in useButtons)
        {
            button.interactable = false;
        }
    }

    // 启用所有按钮
    public void EnableAllButtons()
    {
        foreach (Button button in useButtons)
        {
            button.interactable = true;
        }
    }
}
