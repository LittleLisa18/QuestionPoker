using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingAnimator : MonoBehaviour
{
    public float bounceHeight = 50f;    // 跳动高度
    public float bounceSpeed = 5f;     // 跳动速度
    
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private float timer = 0f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    void Update()
    {
        timer += Time.deltaTime * bounceSpeed;
        // 使用正弦函数创建上下跳动效果
        float bounceOffset = Mathf.Sin(timer) * bounceHeight;
        rectTransform.anchoredPosition = originalPosition + new Vector2(0, bounceOffset);
    }
}
