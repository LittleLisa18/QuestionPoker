using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool isPlayerCard;
    public Vector3 originalPosition;
    private Transform originalParent;
    public CanvasGroup canvasGroup;
    private CardDropZone currentDropZone;
    public CardDropZone originalDropZone; // 关键：记录起始卡槽
    private int originalSiblingIndex;
    public bool isDragging = false;
    private bool isProcessingDrop = false;
    private PlayerCardSelectionHandler selectionHandler;
    private Vector3 originalScale;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        selectionHandler = GetComponent<PlayerCardSelectionHandler>();
        originalScale = transform.localScale;
        // 禁止敌人卡片拖动
        if (!isPlayerCard)
        {
            canvasGroup.blocksRaycasts = false;  // 禁用拖动事件
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isPlayerCard || isDragging || isProcessingDrop) {
            eventData.pointerDrag = null; // 关键：取消拖拽事件
            return;
        }

        if (selectionHandler != null && selectionHandler.isSelected)
        {
            selectionHandler.Deselect();
        }
        transform.localScale = originalScale;
        isDragging = true;

        originalPosition = transform.position;
        originalParent = transform.parent;
        originalDropZone = originalParent?.GetComponent<CardDropZone>();
        originalSiblingIndex = transform.GetSiblingIndex();

        // 关键修复：清除原放置区引用，防止状态不一致
        if (originalDropZone != null)
        {
            originalDropZone.RemoveCard(this); // 立即清除引用
        }

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 screenPos = new Vector3(
            eventData.position.x,
            eventData.position.y,
            Camera.main.WorldToScreenPoint(transform.position).z
        );
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        transform.position = worldPos;
        CheckCurrentDropZone();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || isProcessingDrop) return; // 防止并发
        isProcessingDrop = true;
        isDragging = false;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // ✅ 修复1：传递起始卡槽信息
        if (currentDropZone != null && currentDropZone.CanAcceptCard(this))
        {
            currentDropZone.AcceptCard(this, originalDropZone);
        }
        else
        {
            ReturnToPositionWithValidation(); // ✅ 修复2：安全位置重置
        }

        if (currentDropZone != null)
        {
            currentDropZone.SetHighlight(false);
            currentDropZone = null;
        }
        isProcessingDrop = false;
    }

    private void ReturnToPositionWithValidation()
    {
        if (originalDropZone == null) return;

        // 情况1：原放置区已被占用 → 寻找备用位置或拒绝（避免交换）
        if (originalDropZone.currentCard != null)
        {
            // 不再尝试交换，直接放置到备用位置（如根节点）
            transform.SetParent(transform.root);
            transform.position = originalPosition; // 或自定义安全位置
            Debug.LogWarning("Original drop zone occupied. Card placed in root.");
        }
        // 情况2：原放置区空闲 → 直接归位
        else
        {
            transform.SetParent(originalDropZone.transform);
            transform.localPosition = Vector3.zero;
            originalDropZone.currentCard = this; // 重设引用
        }
    }

    private void CheckCurrentDropZone()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            CardDropZone zone = result.gameObject.GetComponent<CardDropZone>();
            if (zone != null)
            {
                if (currentDropZone != null && currentDropZone != zone)
                    currentDropZone.SetHighlight(false);

                currentDropZone = zone;
                currentDropZone.SetHighlight(true);
                return;
            }
        }

        if (currentDropZone != null)
        {
            currentDropZone.SetHighlight(false);
            currentDropZone = null;
        }
    }
}
