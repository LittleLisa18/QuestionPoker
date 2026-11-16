using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SuitUseButtonHandler : MonoBehaviour
{
    public Button selectCardsButton;

    void Start()
    {
        selectCardsButton.onClick.AddListener(OnSelectCardsButtonClick);
    }

    void OnSelectCardsButtonClick()
    {
        // 启用选卡功能
        EnableCardSelection(1);
        // 显示确认按钮
        SuitPropManager.Instance.ConfirmPropButton.gameObject.SetActive(true);
        SuitPropManager.Instance.diamondsButton.gameObject.SetActive(true);
        SuitPropManager.Instance.flowersButton.gameObject.SetActive(true);
        SuitPropManager.Instance.heartsButton.gameObject.SetActive(true);
        SuitPropManager.Instance.spadesButton.gameObject.SetActive(true);
        SuitPropManager.Instance.ConfirmPropButton.interactable = false;
        SuitPropManager.Instance.diamondsButton.interactable = false;
        SuitPropManager.Instance.flowersButton.interactable = false;
        SuitPropManager.Instance.heartsButton.interactable = false;
        SuitPropManager.Instance.spadesButton.interactable = false;

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

void EnableCardSelection(int customMaxSelectedCards)
{
    // 设置静态变量 maxSelectedCards
    PlayerCardSelectionHandler.maxSelectedCards = customMaxSelectedCards;

    // 激活所有卡牌的选择功能
    var allCards = FindObjectsOfType<PlayerCardSelectionHandler>();
    foreach (var card in allCards)
    {
        card.enabled = true;  // 启用选择功能
    }
}
}
