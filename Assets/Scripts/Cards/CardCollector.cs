using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;

public class CardCollector : MonoBehaviour
{
    [Header("动画参数")]
    public float moveDuration = 0.5f;
    public float delayBetweenCards = 0.05f;
    public float scaleDownFactor = 0.3f;

    [Header("按钮绑定")]
    public Button collectButton;

    [Header("牌堆位置")]
    public Transform deckPosition;

    [Header("分数管理器")]
    public CardComparisonManager comparisonManager; // 添加对分数管理器的引用

    [Header("下注管理器")]
    public BettingManager bettingManager;

    [Header("发牌管理器")]
    public CardDealer cardDealer;

    [Header("音效")]
    private AudioSource cardAudioSource;
    public AudioClip collectCardAudio;

    void Awake()
    {
        cardAudioSource = GetComponent<AudioSource>();
        if (cardAudioSource == null)
        {
            cardAudioSource = gameObject.AddComponent<AudioSource>();
            cardAudioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        collectButton.onClick.AddListener(StartCollectCards);
    }

    public void StartCollectCards()
    {
        bettingManager.UpdateRoundText();
        StartCoroutine(CollectCardsCoroutine());
        collectButton.gameObject.SetActive(false);
    }

    IEnumerator CollectCardsCoroutine()
    {
        collectButton.interactable = false;

        // 在动画开始前隐藏所有分数显示
        if (comparisonManager != null)
        {
            foreach (Text scoreText in comparisonManager.groupScoreTexts)
            {
                if (scoreText != null)
                {
                    scoreText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogWarning("CardComparisonManager reference is missing in CardCollector.");
        }

        // 查找所有卡牌对象
        CardDragHandler[] allCards = FindObjectsOfType<CardDragHandler>()
            .Where(c => c.gameObject.name.Contains("Card"))
            .ToArray();

        // 顺序执行动画
        for (int i = 0; i < allCards.Length; i++)
        {
            CardDragHandler card = allCards[i];

            Sequence animation = DOTween.Sequence();

            // 核心动画：移动+缩放
            animation.Append(card.transform.DOMove(deckPosition.position, moveDuration))
                    .Join(card.transform.DOScale(scaleDownFactor, moveDuration))
                    .OnComplete(() => card.gameObject.SetActive(false)).OnStart(() => PlaySound(collectCardAudio));

            yield return new WaitForSeconds(delayBetweenCards);
        }

        // 等待动画完成
        yield return new WaitForSeconds(moveDuration);

        // 销毁卡牌对象
        foreach (var card in allCards)
        {
            Destroy(card.gameObject);
        }


        // 销毁卡牌后触发重新发牌
        if (cardDealer != null)
        {
            // 调用发牌管理器重新发牌
            cardDealer.CreateAndShuffleDeck();
            cardDealer.StartDealing();
        }
        else
        {
            Debug.LogError("CardDealer reference missing!");
        }


        // // 销毁卡牌后触发下注回合
        // if (bettingManager != null)
        // {
        //     bettingManager.StartMonsterBettingRound(); // 关键调用
        // }
        // else
        // {
        //     Debug.LogError("BettingManager reference missing!");
        // }

        collectButton.interactable = true;
    }

    private void PlaySound(AudioClip clip, float volume = 0.5f)
    {
        if (clip != null && cardAudioSource != null)
        {
            cardAudioSource.PlayOneShot(clip, volume);
        }
    }
}
