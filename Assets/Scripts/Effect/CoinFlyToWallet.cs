using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CoinFlyToWallet : MonoBehaviour
{
    [Header("Coin Settings")]
    public Image coinImage;  // 金币的UI Image
    public Transform coinPosition; // 金币飞向的钱包位置

    [Header("Animation Settings")]
    public int numberOfCoins = 30; // 每次生成的金币数量
    public float moveDuration = 1f; // 每个金币飞向目标位置的时间
    public float scaleDownFactor = 0.3f; // 金币缩小的比例
    public float delayBetweenCoins = 0.2f; // 每个金币动画之间的延迟

    private bool isCoinFlying = true; // 控制金币飞行的标志位
    private List<Image> flyingCoins = new List<Image>(); // 存储正在飞行的金币，以便可以取消

    // public AudioClip collectCoinAudio; // 可选：金币到达时播放的声音
    private AudioSource audioSource;

    void Start()
    {
        // audioSource = GetComponent<AudioSource>(); // 如果不需要音效，可以注释掉这一行
    }

    // 外部调用此方法触发金币飞行效果
    public void StartCoinFlyEffect()
    {
        isCoinFlying = true; // 重启标志位
        StartCoroutine(CoinFlyEffectCoroutine());
    }

    // 外部调用此方法停止金币飞行效果
    public void StopCoinFlyEffect()
    {
        isCoinFlying = false;  // 设置标志为false，停止生成金币
        // 销毁所有当前正在飞行的金币
        foreach (var coin in flyingCoins)
        {
            Destroy(coin.gameObject);
        }
        flyingCoins.Clear();  // 清空正在飞行的金币列表
    }

    private IEnumerator CoinFlyEffectCoroutine()
    {
        while (isCoinFlying)  // 只要 isCoinFlying 为 true，就会一直循环
        {
            for (int i = 0; i < numberOfCoins; i++)
            {
                if (!isCoinFlying) break; // 如果停止标志被触发，则立刻退出

                // 使用 coinPosition 作为所有金币的起始位置
                Transform startPosition = coinPosition;

                // 创建一个新的金币Image
                Image coin = Instantiate(coinImage, startPosition.position, Quaternion.identity);
                coin.transform.SetParent(transform, false); // 保证它是UI元素，放在当前Canvas中

                // 一开始让金币不可见
                coin.gameObject.SetActive(false);

                // 设置金币的动画：飞向目标位置并缩小
                Sequence animation = DOTween.Sequence();

                // 先设置金币为可见
                coin.gameObject.SetActive(true);

                // 添加金币到正在飞行的列表中
                flyingCoins.Add(coin);

                animation.Append(coin.transform.DOMove(coinPosition.position, moveDuration))
                        .Join(coin.transform.DOScale(scaleDownFactor, moveDuration))
                        .OnComplete(() => 
                        {
                            flyingCoins.Remove(coin);  // 动画完成后从列表中移除
                            Destroy(coin.gameObject); // 销毁金币
                        });

                // 播放金币飞行时的声音（已注释掉，不再播放音效）
                // if (collectCoinAudio != null)
                // {
                //     audioSource.PlayOneShot(collectCoinAudio);
                // }

                // 在生成下一个金币之前等待一段时间
                yield return new WaitForSeconds(delayBetweenCoins);
            }

            // 如果金币飞行效果需要停止，则退出循环
            if (!isCoinFlying)
            {
                yield break; // 退出协程
            }
        }
    }
}
