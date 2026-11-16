using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ProtectUseButtonHandler : MonoBehaviour
{
    public Button selectCardsButton;
    public BettingManager bettingManager;
    public Button ProtectButton;
    private Vector3 originalScale; 
    private CanvasGroup buttonCanvasGroup;

    void Start()
    {
        selectCardsButton.onClick.AddListener(OnSelectCardsButtonClick);
        originalScale = ProtectButton.transform.localScale;
        // 确保ProtectButton上有CanvasGroup组件
        buttonCanvasGroup = ProtectButton.GetComponent<CanvasGroup>();
        if (buttonCanvasGroup == null)
        {
            buttonCanvasGroup = ProtectButton.gameObject.AddComponent<CanvasGroup>();
        }    
    }


    void OnSelectCardsButtonClick()
    {
        // 设置bettingManager.isProtectUsed为false
        bettingManager.isProtectUsed = false;

        ShowButtonWithAnimation();
    }

    // 按钮显示动画
    void ShowButtonWithAnimation()
    {
        ProtectButton.gameObject.SetActive(true);
        
        // 重置为初始状态
        buttonCanvasGroup.alpha = 0;
        ProtectButton.transform.localScale = originalScale * 0.8f; // 初始缩小
        
        // 并行执行动画
        Sequence showAnim = DOTween.Sequence();
        showAnim.Join(buttonCanvasGroup.DOFade(1, 1.5f));
        showAnim.Join(ProtectButton.transform.DOScale(originalScale, 1.5f).SetEase(Ease.OutBack)); // 添加弹性效果
    }

    // 按钮隐藏动画
    public void HideButtonWithAnimation()
    {
        // 使用Sequence确保动画同步完成
        Sequence hideAnim = DOTween.Sequence();
        hideAnim.Join(buttonCanvasGroup.DOFade(0, 1.5f));
        hideAnim.Join(ProtectButton.transform.DOScale(originalScale * 0.7f, 1.5f).SetEase(Ease.InBack));
        hideAnim.OnComplete(() => ProtectButton.gameObject.SetActive(false));
    }
}