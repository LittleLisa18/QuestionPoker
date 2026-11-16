using UnityEngine;
using UnityEngine.EventSystems;

public class CardDropZone : MonoBehaviour, IDropHandler
{
    public enum DropZoneType { Hand, CenterPile }
    public DropZoneType zoneType = DropZoneType.CenterPile;
    public int pileIndex = 0;

    [Header("Visuals")]
    public Color highlightColor = Color.yellow;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    public CardDragHandler currentCard; // 单卡槽设计

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void OnDrop(PointerEventData eventData)
    {
        CardDragHandler card = eventData.pointerDrag?.GetComponent<CardDragHandler>();
        if (card != null && CanAcceptCard(card))
            AcceptCard(card, card.originalDropZone); // ✅ 传递起始卡槽
    }

    public bool CanAcceptCard(CardDragHandler card)
    {
        if (!card.isPlayerCard) return false; 
        // 新增：检查目标卡槽的卡牌所有权
        if (currentCard != null && !currentCard.isPlayerCard)
            return false; // 敌方卡牌不能被交换
        
        return zoneType == DropZoneType.CenterPile;
    }

    public void AcceptCard(CardDragHandler newCard, CardDropZone originZone) 
    {
        if (newCard == null) return;

        // 处理 originZone == null 的情况（如从非放置区返回）
        if (originZone == null)
        {
            if (currentCard != null)
            {
                Debug.LogWarning("Cannot accept card: Drop zone already occupied.");
                return; // 拒绝放置，避免堆叠
            }
            // 直接放置新卡（原放置区已通过 RemoveCard 清除）
            newCard.transform.SetParent(transform);
            newCard.transform.localPosition = Vector3.zero;
            currentCard = newCard;
            newCard.originalDropZone = this;
            return;
        }

        // 原有交换逻辑（originZone != null）
        if (currentCard != null && originZone != null) 
        {
            if (!currentCard.isPlayerCard)
            {
                Debug.LogWarning("Cannot swap with enemy card!");
                return;
            }
            CardDragHandler oldCard = currentCard;
            RemoveCard(oldCard);
            oldCard.transform.SetParent(originZone.transform);
            oldCard.transform.localPosition = Vector3.zero;
            originZone.currentCard = oldCard;
            oldCard.originalDropZone = originZone;
        }

        // 执行卡牌的放置
        if (currentCard != null)
        {
            Debug.LogWarning("Drop zone already occupied, unable to accept card.");
            return; // 防止重复放置
        }
    
        newCard.transform.SetParent(transform);
        newCard.transform.localPosition = Vector3.zero;
        currentCard = newCard;
        newCard.originalDropZone = this;

        if (originZone != null && originZone.currentCard == newCard) 
        {
            originZone.RemoveCard(newCard);
        }
    }

    public void RemoveCard(CardDragHandler card)
    {
        if (currentCard == card)
            currentCard = null;
    }

    public void SetHighlight(bool active)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = active ? highlightColor : originalColor;
    }
}
