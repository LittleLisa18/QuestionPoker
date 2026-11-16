using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public GameObject ruleInfo;
    public GameObject information;
    public GameObject clueInfo;
    public GameObject propInfo;

    public void RuleInfo()
    {
        ruleInfo.GetComponent<PanelController>().PanelSwitch();
    }

    public void Information()
    {
        information.GetComponent<PanelController>().PanelSwitch(); 
    }

    public void ClueInfo()
    {
        clueInfo.GetComponent<PanelController>().PanelSwitch();
    }

    public void PropInfo()
    {
        propInfo.GetComponent<PanelController>().PanelSwitch();
    }
}
