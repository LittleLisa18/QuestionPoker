using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyCardSelectionHandler : MonoBehaviour, IPointerClickHandler
{
    public static EnemyCardSelectionHandler currentSelectedCard = null;
    public static int selectedCardCount = 0;  // ✅ 保留选中计数
    public static int maxSelectedCards = 1;   // 最大选择数固定为1

    public bool isSelected = false;
    public float selectedScale = 1.2f;
    public Color highlightColor = Color.red;
    private Vector3 originalScale;
    private Outline outline;

    void Start()
    {
        originalScale = transform.localScale;
        outline = GetComponent<Outline>();
        
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        outline.enabled = false;
        outline.effectColor = highlightColor;
        outline.effectDistance = new Vector2(5, 5);
        enabled = false; // 初始禁用选择功能
    }
    public Vector3 GetOriginalScale()
    {
        return originalScale;
    }
    
    public void ResetCardState()
    {
        isSelected = false;
        selectedCardCount = 0;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        CardDragHandler dragHandler = GetComponent<CardDragHandler>();
        if (dragHandler != null && dragHandler.isDragging) return;

        ToggleSelection();
    }

    public void ToggleSelection()
    {
        // ✅ 使用selectedCardCount判断是否可选中
        if (!isSelected && selectedCardCount < maxSelectedCards) 
        {
            SelectCard();
        }
        else if (isSelected) 
        {
            DeselectCard();
        }
        else if (!isSelected && selectedCardCount >= maxSelectedCards) 
        {
            // ✅ 先取消已选卡牌再选中当前卡牌
            currentSelectedCard?.DeselectCard();
            SelectCard();
        }
    }

    private void SelectCard()
    {
        isSelected = true;
        currentSelectedCard = this;
        selectedCardCount++; // ✅ 更新选中计数
        ApplyVisualFeedback();
        PeekPropManager.Instance?.EnemyUpdateConfirmButtonState();
    }

    public void DeselectCard()
    {
        isSelected = false;
        if (currentSelectedCard == this) 
        {
            currentSelectedCard = null;
        }
        selectedCardCount = Mathf.Max(0, selectedCardCount - 1); // ✅ 确保计数不<0
        ApplyVisualFeedback();
        PeekPropManager.Instance?.EnemyUpdateConfirmButtonState();
    }

    private void ApplyVisualFeedback()
    {
        transform.localScale = isSelected ? originalScale * selectedScale : originalScale;
        outline.enabled = isSelected;
        if (isSelected) transform.SetAsLastSibling();
    }

    public void Deselect()
    {
        DeselectCard();
    }

    public void EnableSelection()
    {
        enabled = true;
    }
}
