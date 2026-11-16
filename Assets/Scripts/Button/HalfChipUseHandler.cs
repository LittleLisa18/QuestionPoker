using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HalfChipUseHandler : MonoBehaviour
{
    public Button halfChipUseButton;
    public BettingManager bettingManager;

    // Start is called before the first frame update
    void Start()
    {
        halfChipUseButton.onClick.AddListener(OnHalfChipUseButtonClick);
    }

    public void OnHalfChipUseButtonClick()
    {
        bettingManager.UseHalfChip();
    }
}

