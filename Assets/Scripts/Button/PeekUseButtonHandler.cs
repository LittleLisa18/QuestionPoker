
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PeekUseButtonHandler : MonoBehaviour
{
    public Button selectCardsButton;

    void Start()
    {
        selectCardsButton.onClick.AddListener(OnSelectCardsButtonClick);
    }

    void OnSelectCardsButtonClick()
    {
        // 启用选卡功能
        EnableCardSelection();
        // 显示确认按钮
        PeekPropManager.Instance.ConfirmPropButton.gameObject.SetActive(true);
        PeekPropManager.Instance.ConfirmPropButton.interactable = false;

        // 禁用所有按钮
        ButtonDisabler buttonDisabler = FindObjectOfType<ButtonDisabler>();
        if (buttonDisabler != null)
        {
            buttonDisabler.DisableAllButtons();  // 禁用所有按钮
        }
        // 禁用ToolCardManager的Update方法
        ToolCardManager toolCardManager = FindObjectOfType<ToolCardManager>();
        if (toolCardManager != null)
        {
            toolCardManager.shouldUpdate = false;  // 禁用Update执行
        }
        // 禁用CardFlipController的Update方法
        CardFlipController cardFlipController = FindObjectOfType<CardFlipController>();
        if (cardFlipController != null)
        {
            cardFlipController.isPropUsed = false;  // 恢复Update执行
        }
    }

    void EnableCardSelection()
    {
        // 激活所有敌方卡牌的选择功能
        var allEnemyCards = FindObjectsOfType<EnemyCardSelectionHandler>();
        foreach (var card in allEnemyCards)
        {
            card.enabled = true;  // 启用选择功能
        }
    }
}