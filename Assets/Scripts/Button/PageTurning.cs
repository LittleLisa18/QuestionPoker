using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageTurning : MonoBehaviour
{
    private bool isFirst = true;
    private int pagination = 0;
    
    //首次运行时将页面设置到第一页
    void OnEnable()
    {
        if (isFirst)
        {
            isFirst = false;
            pagination = 0;
        }
        
        PageTurn(pagination);
    }
    
    //翻页代码
    private void PageTurn(int page)
    {
        if (page >= transform.childCount)
        {
            page = pagination = 0;
        }

        if (pagination < 0)
        {
            page = pagination = transform.childCount - 1;
        }
        
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i == page)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        Debug.Log(pagination);
    }

    //翻页按钮事件
    public void PageAdd()
    {
        PageTurn(++pagination);
    }

    public void PageRemove()
    {
        PageTurn(--pagination);
    }
    
}
