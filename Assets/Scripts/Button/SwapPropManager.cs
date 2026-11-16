using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
public class SwapPropManager : MonoBehaviour
{
    public static SwapPropManager Instance; 
    [Header("UI设置")]
    public Button ConfirmPropButton;         // 回收按钮
    public Transform deckPosition;       // 牌堆位置

    [Header("动画参数")]
    public float moveDuration = 0.5f;    // 移动时间
    public float scaleDownFactor = 0.3f;  // 缩放比例
    public float delayBetweenCards = 0.1f; // 卡牌间延迟

    [Header("发牌引用")]
    public CardDealer cardDealer;        // 发牌管理器

    void Start()
    {
        if (Instance == null) Instance = this; // 单例初始化
        // 绑定按钮点击事件
        ConfirmPropButton.onClick.AddListener(RecycleSelectedCards);
        ConfirmPropButton.interactable = false; // 初始时按钮不可点击
    }

    // 更新确认按钮的状态
    public void PlayerUpdateConfirmButtonState()
    {
        if (PlayerCardSelectionHandler.selectedCardCount > 0)  // 至少选中一张卡
        {
            ConfirmPropButton.interactable = true;
        }
        else
        {
            ConfirmPropButton.interactable = false;
        }
    }

    // 回收选中的卡牌
    public void RecycleSelectedCards()
    {
        // 重置选中的卡牌数量
        PlayerCardSelectionHandler.selectedCardCount = 0;

        // 隐藏回收按钮
        ConfirmPropButton.gameObject.SetActive(false);

        // 禁用所有卡牌的选择功能
        var allCards = FindObjectsOfType<PlayerCardSelectionHandler>();
        foreach (var card in allCards)
        {
            card.enabled = false;  // 禁用选择功能
        }

        StartCoroutine(RecycleCardsCoroutine());
        // 启用所有按钮
        ButtonDisabler buttonDisabler = FindObjectOfType<ButtonDisabler>();
        if (buttonDisabler != null)
        {
            buttonDisabler.EnableAllButtons();  // 启用所有按钮
        }
        // 恢复ToolCardManager的Update方法
        ToolCardManager toolCardManager = FindObjectOfType<ToolCardManager>();
        if (toolCardManager != null)
        {
            toolCardManager.shouldUpdate = true;  // 恢复Update执行
        }
        // 恢复CardFlipController的Update方法
        CardFlipController cardFlipController = FindObjectOfType<CardFlipController>();
        if (cardFlipController != null)
        {
            cardFlipController.isPropUsed = true;  // 恢复Update执行
        }
    }

    IEnumerator RecycleCardsCoroutine()
    {
        ConfirmPropButton.interactable = false;

        // 获取所有选中的卡牌
        var selectedCards = FindObjectsOfType<PlayerCardSelectionHandler>()
            .Where(c => c.isSelected)
            .ToList();

        // 记录空卡槽位置
        var emptySlots = new List<Transform>();

        // 播放回收动画并销毁卡牌
        foreach (var card in selectedCards)
        {
            // 记录卡牌位置作为空槽位
            emptySlots.Add(card.transform.parent);

            // 动画序列：移动+缩放
            Sequence animation = DOTween.Sequence();
            animation.Append(card.transform.DOMove(deckPosition.position, moveDuration))
                    .Join(card.transform.DOScale(scaleDownFactor, moveDuration))
                    .OnComplete(() => Destroy(card.gameObject));

            yield return new WaitForSeconds(delayBetweenCards);
        }

        // 等待动画完成
        yield return new WaitForSeconds(moveDuration);

        // 补充新卡牌到空槽位
        if (cardDealer != null && emptySlots.Count > 0)
        {
            // 从牌堆抽取新卡（数量等于回收的卡牌数）
            List<CardData> newCards = cardDealer.DrawCardsFromRemainingDeck(emptySlots.Count);

            // 发新牌到空槽位
            for (int i = 0; i < newCards.Count; i++)
            {
                if (emptySlots[i] == null) continue;

                GameObject newCard = Instantiate(
                    cardDealer.cardPrefab,
                    deckPosition.position,
                    Quaternion.identity
                );

                // 设置卡牌视觉
                cardDealer.SetCardVisual(newCard, newCards[i]);
                cardDealer.ShowCardFace(newCard, true);


                // 添加必要组件
                SpriteRenderer playerCardRenderer = newCard.AddComponent<SpriteRenderer>();
                CardDragHandler dragHandler = newCard.AddComponent<CardDragHandler>();
                newCard.AddComponent<PlayerCardSelectionHandler>();
                dragHandler.isPlayerCard = true; // 关键设置
                
                // 设置排序层
                SpriteRenderer renderer = newCard.GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.sortingLayerName = "ObjectsOnTable";
                cardDealer.SetSortingLayerForCard(newCard);
                
                // 发牌动画
                Tweener animation = newCard.transform.DOMove(emptySlots[i].position, moveDuration).SetEase(Ease.OutCubic);

                // 等待动画完成
                yield return animation.WaitForCompletion();

                // 将卡牌放入槽位
                CardDropZone dropZone = emptySlots[i].GetComponent<CardDropZone>();
                if (dropZone != null)
                {
                    dropZone.AcceptCard(dragHandler, null);
                }

                yield return new WaitForSeconds(delayBetweenCards);
            }
        }

        ConfirmPropButton.interactable = true;
    }
}