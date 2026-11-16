using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    // public GameObject blackBackdrop;
    public bool isPanelActive = false; // 用于控制面板和背景的状态

    private void Start()
    {
        // 初始时面板和背景都不可见
        isPanelActive = false;
        gameObject.SetActive(isPanelActive);
        // blackBackdrop.SetActive(isPanelActive);
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PanelSwitch();
            }
        }
    }

    // 开关信息页时切换状态
    public void PanelSwitch()
    {
        isPanelActive = !isPanelActive;
        gameObject.SetActive(isPanelActive);
        // blackBackdrop.SetActive(isPanelActive);
    }
}
