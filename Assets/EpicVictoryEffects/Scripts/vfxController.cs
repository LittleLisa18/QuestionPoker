﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VfxController : MonoBehaviour
{
    [Header("Background FX Settings")]
    public GameObject[] bgFxPrefabs;  // 背景特效预制体数组（Inspector拖入）
    public int currentBgFxIndex = 0;  // 当前使用的特效索引
    private GameObject currentBgFxInstance; // 当前实例化的特效对象

    /// <summary>
    /// 播放背景特效
    /// </summary>
    public void PlayBgFx()
    {
        StopBgFx(); // 销毁旧特效避免重复
        
        if (currentBgFxIndex >= 0 && currentBgFxIndex < bgFxPrefabs.Length)
        {
            // 实例化新特效并设置层级
            currentBgFxInstance = Instantiate(bgFxPrefabs[currentBgFxIndex]);
            SetParticleLayer(currentBgFxInstance, "ObjectsEffect", 10); // 设置层级为 ObjectsEffect
        }
    }

    /// <summary>
    /// 停止并销毁背景特效
    /// </summary>
    public void StopBgFx()
    {
        if (currentBgFxInstance != null)
        {
            Destroy(currentBgFxInstance);
            currentBgFxInstance = null;
        }
    }

    /// <summary>
    /// 设置粒子系统渲染层级（解决Canvas显示问题）
    /// </summary>
    private void SetParticleLayer(GameObject effectObj, string layerName, int orderInLayer)
    {
        // 添加 ParticleSystemRenderer 组件（如果没有）
        ParticleSystemRenderer psr = effectObj.GetComponent<ParticleSystemRenderer>();
        if (psr == null)
        {
            psr = effectObj.AddComponent<ParticleSystemRenderer>();
        }

        // 设置层级和渲染顺序
        psr.sortingLayerName = layerName;    // 设置为 ObjectsEffect 层
        psr.sortingOrder = orderInLayer;     // 设置渲染顺序
    }
}
