using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
public class PeekPropManager : MonoBehaviour
{
    public static PeekPropManager Instance;
    [Header("UI设置")]
    public Button ConfirmPropButton;         // 回收按钮

    [Header("动画参数")]
    public float flipDuration = 0.8f;       // 翻牌动画持续时间
    public float delayBetweenFlips = 0.2f;  // 卡牌之间的翻牌延迟

    [Header("概率设置")]
    [Range(0f, 1f)]
    public float specialAnimationProbability = 0.2f;

    void Start()
    {
        if (Instance == null) Instance = this; // 单例初始化
        // 绑定按钮点击事件
        ConfirmPropButton.onClick.AddListener(RecycleSelectedCards);
        ConfirmPropButton.interactable = false; // 初始时按钮不可点击
    }

    public void EnemyUpdateConfirmButtonState()
    {
        if (EnemyCardSelectionHandler.selectedCardCount > 0)  // 至少选中一张卡
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
        EnemyCardSelectionHandler.selectedCardCount = 0;

        // 隐藏回收按钮
        ConfirmPropButton.gameObject.SetActive(false);

        // 禁用所有卡牌的选择功能
        var allCards = FindObjectsOfType<EnemyCardSelectionHandler>();
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
        var selectedCards = FindObjectsOfType<EnemyCardSelectionHandler>()
            .Where(c => c.isSelected)
            .ToList();

        // 执行翻牌动画
        foreach (var card in selectedCards)
        {
            float randomValue = Random.Range(0f, 1f); // 只生成一次随机数
            if (card != null)
            {
                // 查找卡牌的前后两面
                Transform back = card.transform.Find("Back");
                Transform front = card.transform.Find("Front");

                if (back != null && front != null)
                {
                    Debug.Log($"卡牌概率检测: 随机值={randomValue:F2}, 设定概率={specialAnimationProbability:F2}, " +
                              $"结果={(randomValue < specialAnimationProbability ? "特殊动画" : "普通动画")}");
                    if (randomValue < specialAnimationProbability)
                    {
                        // 特殊处理：保持组件不变，执行覆盖动画
                        StartCoroutine(SpecialCoverAnimation(card.gameObject, back, front));
                    }
                    else
                    {
                        // 正常执行翻牌动画
                        StartCoroutine(FlipSingleCard(card.gameObject, back, front));
                    }
                }
            }

            // 等待延迟后再翻下一张
            yield return new WaitForSeconds(delayBetweenFlips);

            // 仅对非特殊卡牌执行组件变更
            if (randomValue >= specialAnimationProbability)
            {
                EnemyCardSelectionHandler enemyHandler = card.gameObject.GetComponent<EnemyCardSelectionHandler>();
                if (enemyHandler != null)
                {
                    Destroy(enemyHandler); // 移除 EnemyCardSelectionHandler 组件
                }

                CardDragHandler dragHandler = card.gameObject.GetComponent<CardDragHandler>();
                if (dragHandler != null && dragHandler.isPlayerCard)
                {
                    card.gameObject.AddComponent<PlayerCardSelectionHandler>();
                }
            }
        }
        var allCards = FindObjectsOfType<EnemyCardSelectionHandler>();
        foreach (var card in allCards)
        {
            card.ResetCardState(); // 新增重置方法
        }
        // 恢复按钮状态
        ConfirmPropButton.interactable = true;
    }

    // 翻动单张卡牌的动画
    IEnumerator FlipSingleCard(GameObject card, Transform back, Transform front)
    {
        if (front.gameObject.activeSelf)
        {
            yield break;  // 跳过这张卡牌的翻牌
        }
        EnemyCardSelectionHandler selectionHandler = card.GetComponent<EnemyCardSelectionHandler>();
        if (selectionHandler != null)
        {
            // 使用 EnemyCardSelectionHandler 中记录的原始缩放值
            card.transform.localScale = selectionHandler.GetOriginalScale();
        }

        // 初始状态：显示背面，隐藏正面
        back.gameObject.SetActive(true);
        front.gameObject.SetActive(false);

        // 创建旋转序列
        Sequence flipSequence = DOTween.Sequence();

        // 第一阶段：旋转90度（到侧面）
        flipSequence.Append(card.transform.DORotate(new Vector3(0, 90, 0), flipDuration / 2)
            .SetEase(Ease.OutQuad));

        // 第二阶段：切换正反面并完成旋转
        flipSequence.AppendCallback(() =>
        {
            // 切换正反面显示
            back.gameObject.SetActive(false);
            front.gameObject.SetActive(true);

            // 设置到180度位置
            card.transform.eulerAngles = new Vector3(0, 270, 0);
        });

        // 第三阶段：完成旋转到0度
        flipSequence.Append(card.transform.DORotate(Vector3.zero, flipDuration / 2)
            .SetEase(Ease.InQuad));

        yield return flipSequence.WaitForCompletion();
    }

    IEnumerator SpecialCoverAnimation(GameObject card, Transform back, Transform front)
    {
        // 1. 恢复原始缩放值
        EnemyCardSelectionHandler selectionHandler = card.GetComponent<EnemyCardSelectionHandler>();
        if (selectionHandler != null)
        {
            card.transform.localScale = selectionHandler.GetOriginalScale();
        }

        // 2. 临时激活front以获取子对象
        bool wasFrontActive = front.gameObject.activeSelf;
        front.gameObject.SetActive(true); // 临时激活以查找子对象

        // 3. 获取Cover和花色元素
        Transform cover = front.Find("Cover");
        Transform type = front.Find("Type");
        Transform suit1 = front.Find("Suit1");
        Transform suit2 = front.Find("Suit2");

        // 4. 恢复front的原始激活状态
        front.gameObject.SetActive(wasFrontActive);

        if (cover == null)
        {
            Debug.LogError("Cover对象未找到！请确保Front下有Cover子对象");
            yield break;
        }

        // 5. 初始状态设置
        cover.gameObject.SetActive(false);
        back.gameObject.SetActive(true);
        front.gameObject.SetActive(false);

        Sequence flipSequence = DOTween.Sequence();

        // ===== 第一阶段：背面翻转到侧面 (0° → 90°) =====
        flipSequence.Append(card.transform.DORotate(new Vector3(0, 90, 0), flipDuration / 4));

        // ===== 切换显示Cover并隐藏花色元素 =====
        flipSequence.AppendCallback(() =>
        {
            back.gameObject.SetActive(false);
            front.gameObject.SetActive(true); // 必须激活front才能显示其子对象
            cover.gameObject.SetActive(true);

            if (type != null) type.gameObject.SetActive(false);
            if (suit1 != null) suit1.gameObject.SetActive(false);
            if (suit2 != null) suit2.gameObject.SetActive(false);
        });

        // ===== 第二阶段：侧面翻转到Cover朝上 (90° → 180°) =====
        flipSequence.Append(card.transform.DORotate(new Vector3(0, 180, 0), flipDuration / 4));

        // ===== 停留0.5秒显示Cover =====
        flipSequence.AppendInterval(0.5f);

        // ===== 第三阶段：Cover朝上翻转到侧面 (180° → 270°) =====
        flipSequence.Append(card.transform.DORotate(new Vector3(0, 270, 0), flipDuration / 4));

        // ===== 切换回背面并恢复花色元素 =====
        flipSequence.AppendCallback(() =>
        {
            cover.gameObject.SetActive(false);
            front.gameObject.SetActive(false); // 隐藏front
            back.gameObject.SetActive(true);

            if (type != null) type.gameObject.SetActive(true);
            if (suit1 != null) suit1.gameObject.SetActive(true);
            if (suit2 != null) suit2.gameObject.SetActive(true);
        });

        // ===== 第四阶段：侧面翻转到背面 (270° → 360°) =====
        flipSequence.Append(card.transform.DORotate(Vector3.zero, flipDuration / 4));

        yield return flipSequence.WaitForCompletion();
    }
}