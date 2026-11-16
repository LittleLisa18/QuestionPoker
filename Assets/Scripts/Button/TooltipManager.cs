using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public GameObject diamondsPanel;   // 钻石面板
    public GameObject heartsPanel;     // 红心面板
    public Text diamondsText;          // 钻石计数文本
    public Text heartsText;            // 红心计数文本
    public int diamonds_count = 0;     // 钻石计数
    public Text diamondsCountText;     // 钻石计数数字
    public int hearts_count = 0;       // 红心计数
    public Text heartsCountText;     // 红心计数数字
    public Button hearts;
    public Button diamonds;
    public GameObject protectButtonPanel;   // 钻石面板
    public Text protectButtonText;          // 钻石计数文本

    public void ShowProtectButtonTooltip()
    {
        protectButtonPanel.SetActive(true); 
        protectButtonText.text = "守护符\nAll In失败后自动生效，恢复10%下注筹码\n使用后，可在道具区购买守护卡重新获得";
    }

    // 隐藏守护符提示框
    public void HideProtectButtonTooltip()
    {
        protectButtonPanel.SetActive(false);  // 隐藏守护符面板
    }


    // 鼠标悬停时显示提示框（DiamondsPanel）
    public void ShowDiamondsTooltip()
    {
        diamondsPanel.SetActive(true);  // 显示钻石面板
        diamondsText.text = GetDiamondsText();  // 设置钻石面板内容
    }

    // 鼠标移开时隐藏钻石提示框
    public void HideDiamondsTooltip()
    {
        diamondsPanel.SetActive(false);  // 隐藏钻石面板
    }

    // 鼠标悬停时显示提示框（HeartsPanel）
    public void ShowHeartsTooltip()
    {
        heartsPanel.SetActive(true);  // 显示红心面板
        heartsText.text = GetHeartsText();  // 设置红心面板内容
    }

    // 鼠标移开时隐藏红心提示框
    public void HideHeartsTooltip()
    {
        heartsPanel.SetActive(false);  // 隐藏红心面板
    }

    // 根据规则设置钻石面板的显示文本
    private string GetDiamondsText()
    {
        if (diamonds_count <= 5)
        {
            return "无特殊效果";
        }
        else if (diamonds_count > 5 && diamonds_count <= 8)
        {
            return "玩家下一轮手牌将有1张不可见";
        }
        else if (diamonds_count > 8 && diamonds_count <= 11)
        {
            return "玩家下一轮手牌将有2张不可见";
        }
        else
        {
            return "玩家下一轮手牌将有3张不可见";
        }
    }

    // 根据规则设置红心面板的显示文本
    private string GetHeartsText()
    {
        if (hearts_count <= 5)
        {
            return "无特殊效果";
        }
        else if (hearts_count > 5 && hearts_count <= 8)
        {
            return "boss剩余筹码总数增加300";
        }
        else if (hearts_count > 8 && hearts_count <= 11)
        {
            return "boss剩余筹码总数增加600";
        }
        else
        {
            return "boss剩余筹码总数增加900";
        }
    }
}
