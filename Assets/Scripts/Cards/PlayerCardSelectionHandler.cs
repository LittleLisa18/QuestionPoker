using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerCardSelectionHandler : MonoBehaviour, IPointerClickHandler
{
    public static int selectedCardCount = 0;  // 记录当前已选择的卡牌数量
    public static int maxSelectedCards = 3;  // 最大可选择卡牌数

    public bool isSelected = false;
    public float selectedScale = 1.2f;
    public Color highlightColor = Color.yellow;
    private Vector3 originalScale;
    private Outline outline;

    void Start()
    {
        originalScale = transform.localScale;

        // 获取并添加Outline组件
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        // 初始状态下不启用高亮
        outline.enabled = false;

        // 设置高亮颜色和边缘宽度
        outline.effectColor = highlightColor; // 使用effectColor设置高亮颜色
        outline.effectDistance = new Vector2(5, 5);  // 使用effectDistance设置边缘宽度，调整为你需要的宽度

        // 初始状态下禁用卡牌选择（直到selectCardsButton被点击）
        enabled = false;
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
        // 避免拖拽过程中触发选中
        CardDragHandler dragHandler = GetComponent<CardDragHandler>();
        if (dragHandler != null && dragHandler.isDragging) return;

        // 控制选择数量
        if (!isSelected && selectedCardCount < maxSelectedCards)
        {
            ToggleSelection();
        }
        else if (isSelected)
        {
            ToggleSelection();
        }
    }

    public void ToggleSelection()
    {
        isSelected = !isSelected;
        ApplyVisualFeedback();

        if (isSelected)
        {
            selectedCardCount++;
        }
        else
        {
            selectedCardCount--;
        }

        // 更新回收按钮状态
        SwapPropManager.Instance.PlayerUpdateConfirmButtonState();
        Add1PropManager.Instance.PlayerUpdateConfirmButtonState();
        Minus1PropManager.Instance.PlayerUpdateConfirmButtonState();
        SuitPropManager.Instance.PlayerUpdateConfirmButtonState();
    }

    private void ApplyVisualFeedback()
    {
        // 缩放效果
        transform.localScale = isSelected ? originalScale * selectedScale : originalScale;

        // 启用或禁用Outline组件进行高亮
        outline.enabled = isSelected;

        // 层级提升（防止被遮挡）
        if (isSelected) transform.SetAsLastSibling();
    }

    // 外部调用取消选中（如拖拽开始时）
    public void Deselect()
    {
        isSelected = false;
        ApplyVisualFeedback();
    }

    // 允许外部启用选择（当按钮点击后）
    public void EnableSelection()
    {
        enabled = true;
    }
}
