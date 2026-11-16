using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NumberRollingEffect : MonoBehaviour
{
    public Text numberText;   // 用于显示数字的Text组件
    public int startValue = 1000;  // 初始值
    public int endValue = 2000;    // 目标值
    public float duration = 3f;    // 完成滚动的时间

    private void Start()
    {
        // 开始数字滚动效果
        StartCoroutine(RollNumber());
    }

    private IEnumerator RollNumber()
    {
        float elapsedTime = 0f;

        // 从初始值到目标值的过渡
        while (elapsedTime < duration)
        {
            // 计算当前的数字
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, elapsedTime / duration));

            // 更新Text组件的数字
            numberText.text = currentValue.ToString();

            // 增加已过时间
            elapsedTime += Time.deltaTime;

            // 等待下一帧
            yield return null;
        }

        // 确保最后显示目标值
        numberText.text = endValue.ToString();
    }
}
