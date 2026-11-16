using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoubleChipUseHandler : MonoBehaviour
{

    public Button doubleChipUseButton;
    public BettingManager bettingManager;

    // Start is called before the first frame update
    void Start()
    {
        doubleChipUseButton.onClick.AddListener(OnDoubleChipUseButtonClick);
    }

    public void OnDoubleChipUseButtonClick()
    {
        bettingManager.UseDoubleChip();
    }
}
