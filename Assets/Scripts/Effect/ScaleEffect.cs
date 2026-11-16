using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleEffect : MonoBehaviour
{
    public float scaleSpeed = 1f;       // 缩放速度
    public float minScale = 0.8f;       // 最小缩放值
    public float maxScale = 1.2f;       // 最大缩放值
    public bool pingPong = true;        // 是否循环缩放
    
    private Vector3 originalScale;
    private float currentScaleFactor = 1f;
    private bool scalingUp = true;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // 计算当前缩放因子
        if (pingPong)
        {
            currentScaleFactor = scalingUp ? 
                currentScaleFactor + scaleSpeed * Time.deltaTime :
                currentScaleFactor - scaleSpeed * Time.deltaTime;
            
            // 检查是否到达边界
            if (currentScaleFactor > maxScale)
            {
                currentScaleFactor = maxScale;
                scalingUp = false;
            }
            else if (currentScaleFactor < minScale)
            {
                currentScaleFactor = minScale;
                scalingUp = true;
            }
        }
        else
        {
            // 单次缩放模式
            currentScaleFactor = Mathf.PingPong(Time.time * scaleSpeed, maxScale - minScale) + minScale;
        }
        
        // 应用缩放
        transform.localScale = originalScale * currentScaleFactor;
    }
}
