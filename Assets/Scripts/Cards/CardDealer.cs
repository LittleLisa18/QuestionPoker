using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro; 

[System.Serializable]
public class CardData
{
    public string suit; // "hearts", "spades", etc.
    public string rank; // "A", "2", ..., "K"

    public CardData(string suit, string rank)
    {
        this.suit = suit;
        this.rank = rank;
    }
}

public class CardDealer : MonoBehaviour
{
    [Header("Card Setup")]
    public GameObject cardPrefab;
    public Transform deckPosition; // 发牌起始位置

    [Header("Slots")]
    public CardDropZone[] playerSlots = new CardDropZone[7];
    public CardDropZone[] enemySlots = new CardDropZone[7];

    [Header("Animation")]
    public float dealInterval = 0.2f;
    public float moveDuration = 0.5f;

    [Header("Betting System")]
    public BettingManager bettingManager;

    [Header("Game Settings")]
    public float flipProbability = 1 / 7f; 
    public Button PropButton;

    private List<CardData> deck = new List<CardData>();
 
    public EnemyCardManager enemyCardManager;

    public CardFlipController flipController;
    public List<CardData> remainingDeck = new List<CardData>();
    public bool IsJoker = false;
    public TooltipManager tooltipManager;

    [Header("Card Audio")]
    private AudioSource cardAudioSource;
    public AudioClip dealCardAudio;

    [Header("Preset Decks")]
    public bool useFixedDeck = false; // 控制是否使用预设牌组
    public int useFixedDeckEveryNRounds = 5; // 每n轮使用预设牌组

    public List<List<CardData>> presetDecks = new List<List<CardData>>(); // 多套预设牌组

    private List<CardData> fixedDeck = new List<CardData>(); // 当前选中的预设牌组

    void Awake()
    {
        cardAudioSource = GetComponent<AudioSource>();
        if (cardAudioSource == null)
        {
            cardAudioSource = gameObject.AddComponent<AudioSource>();
            cardAudioSource.playOnAwake = false;
        }
        // 初始化预设牌组
        InitializePresetDecks();
    }
    private void InitializePresetDecks()
    {
        // 1. 使用牌组模板定义（数据驱动）
        List<string[]> deckTemplates = new List<string[]>
        {
            // 预设牌组1
            new string[]
            {
                "diamonds 6",
                "spades 6",
                "hearts K",
                "hearts A",
                "spades 10",
                "diamonds 2",
                "flowers 8",
                "flowers 10",
                "diamonds 4",
                "flowers 5",
                "diamonds 9",
                "spades 7",
                "spades A",
                "spades 7",
            },

            // 预设牌组2
            new string[]
            {
                "diamonds K",
                "diamonds A",
                "flowers 3",
                "hearts 2",
                "spades K",
                "spades A",
                "hearts K",
                "hearts A",
                "flowers 6",
                "flowers 4",
                "flowers 8",
                "flowers 9",
                "hearts 10",
                "diamonds 2"
            },

            // 预设牌组3
            new string[]
            {
                "spades K",
                "diamonds K",
                "spades A",
                "hearts 2",
                "diamonds 10",
                "spades 7",
                "hearts K",
                "hearts 8",
                "flowers 5",
                "flowers 2",
                "flowers J",
                "flowers 6",
                "spades Q",
                "diamonds 2"
            },
            // 可扩展更多牌组...
        };
        // 2. 解析模板创建牌组
        foreach (var template in deckTemplates)
        {
            List<CardData> deck = new List<CardData>();

            foreach (string cardDef in template)
            {
                string[] parts = cardDef.Split(' ');
                if (parts.Length == 2)
                {
                    deck.Add(new CardData(parts[0], parts[1]));
                }
                else
                {
                    Debug.LogError($"无效的卡牌定义: {cardDef}");
                }
            }
            presetDecks.Add(deck);
        }
    }

    public void InitializeGame()
    {
        bettingManager = FindObjectOfType<BettingManager>();
        if (bettingManager == null)
        {
            Debug.LogError("BettingManager not found in the scene!");
            return;
        }

        bettingManager.InitializeGame();
        CreateAndShuffleDeck();
        StartDealing();
    }

    public void CreateAndShuffleDeck()
    {
        string[] suits = { "hearts", "spades", "flowers", "diamonds" };
        string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

        deck.Clear();
        foreach (string suit in suits)
        {
            foreach (string rank in ranks)
            {
                deck.Add(new CardData(suit, rank));
            }
        }

        // 判断是否使用预设牌组
        if (!IsJoker)
        {
            if (bettingManager.currentRound == 0)
            {
                useFixedDeck = true; // 第1轮直接使用第一组预设牌组
            }
            else if (bettingManager.currentRound > 5 && bettingManager.currentRound % useFixedDeckEveryNRounds == 0 && presetDecks.Count > 0)
            {
                // 每n轮使用一次预设牌组
                useFixedDeck = true;
            }
        }

        // 处理预设牌组逻辑
        if (useFixedDeck && presetDecks.Count > 0)
        {
            // 随机选择一套预设牌组
            int presetIndex;

            // 如果是第一次回合（确保选择第一组预设牌组）
            if (bettingManager.currentRound == 0)
            {
                presetIndex = 0; // 强制选择第一组
            }
            else
            {
                // 随机选择一个预设牌组
                presetIndex = Random.Range(0, presetDecks.Count);
            }

            fixedDeck = new List<CardData>(presetDecks[presetIndex]);
            Debug.Log($"<color=yellow>[预设牌组] 选择牌组 #{presetIndex} | 牌数: {fixedDeck.Count}</color>");

            // 移除该预设牌组以防重复使用
            presetDecks.RemoveAt(presetIndex);
            Debug.Log($"<color=blue>[预设牌组] 剩余可用牌组数量: {presetDecks.Count}</color>");

            // 确保预设牌组有14张牌
            if (fixedDeck.Count < 14)
            {
                useFixedDeck = false; // 回退到随机模式
            }
            else
            {
                // 移除重复卡牌
                int removedCount = 0;
                foreach (CardData card in fixedDeck)
                {
                    removedCount += deck.RemoveAll(c => c.suit == card.suit && c.rank == card.rank);
                }
            }
        }

        // 洗牌（Fisher–Yates）
        for (int i = 0; i < deck.Count; i++)
        {
            int rand = Random.Range(i, deck.Count);
            var temp = deck[i];
            deck[i] = deck[rand];
            deck[rand] = temp;
        }
        Debug.Log($"<color=cyan>[洗牌] 标准牌组洗牌完成</color>");

        remainingDeck = new List<CardData>();

        if (useFixedDeck)
        {
            remainingDeck.AddRange(fixedDeck); // 前14张为预设牌组
            Debug.Log($"<color=green>[牌堆构建] 添加预设牌组: {fixedDeck.Count}张</color>");
        }
        remainingDeck.AddRange(deck); // 后续为随机牌组

        useFixedDeck = false;
    }
    
    public List<CardData> DrawCardsFromRemainingDeck(int count)
    {
        List<CardData> drawnCards = new List<CardData>();
        int actualCount = Mathf.Min(count, remainingDeck.Count);

        if (actualCount <= 0)
        {
            Debug.LogWarning("牌堆已空，无法抽牌！");
            return drawnCards;
        }

        for (int i = 0; i < actualCount; i++)
        {
            drawnCards.Add(remainingDeck[0]); // 从顶部取牌
            remainingDeck.RemoveAt(0);
        }
        return drawnCards;
    }

    public void StartDealing()
    {
        bettingManager.ResetCost(IsJoker);
        StartCoroutine(DealCardsCoroutine());
    }

    IEnumerator DealCardsCoroutine()
    {
        PropButton.interactable = false;

        // 如果 IsJoker 为 true，计算需要背面朝上的卡牌数量
        int faceDownCount = 0;
        if (IsJoker)
        {
            // 根据 diamonds_count 设置背面朝上的卡牌数量
            if (tooltipManager.diamonds_count <= 5)
            {
                faceDownCount = 0; // 无特殊效果
            }
            else if (tooltipManager.diamonds_count > 5 && tooltipManager.diamonds_count <= 8)
            {
                faceDownCount = 1; // 玩家下一轮手牌将有1张不可见
            }
            else if (tooltipManager.diamonds_count > 8 && tooltipManager.diamonds_count <= 11)
            {
                faceDownCount = 2; // 玩家下一轮手牌将有2张不可见
            }
            else
            {
                faceDownCount = 3; // 玩家下一轮手牌将有3张不可见
            }
        }
        for (int i = 0; i < 7; i++)
        {
            CardData playerCardData = remainingDeck[0];
            remainingDeck.RemoveAt(0);

            // // 判断当前回合是否超过5，决定是否设置背面朝上的概率
            // bool isFaceDown = bettingManager.currentRound > 4 && Random.value < flipProbability;

            bool isFaceDown = false;
            if (IsJoker && i < faceDownCount)
            {
                isFaceDown = true; // 设置背面朝上
            }
            else if (!IsJoker && bettingManager.currentRound > 4 && Random.value < flipProbability)
            {
                isFaceDown = true; // 其他情况下使用原有的概率
            }

            // 玩家牌
            GameObject playerCard = Instantiate(cardPrefab, deckPosition.position, Quaternion.identity);
            if (playerCard == null) continue; // 确保卡牌实例化成功

            // 为玩家卡牌添加SpriteRenderer组件
            SpriteRenderer playerCardRenderer = playerCard.AddComponent<SpriteRenderer>();

            playerCard.AddComponent<CardDragHandler>();
            playerCard.GetComponent<CardDragHandler>().isPlayerCard = true;  // 设置为玩家卡牌

            // 根据卡牌正反面朝上来添加不同的选择处理器
            if (isFaceDown)
            {
                // 背面朝上时，添加EnemyCardSelectionHandler
                playerCard.AddComponent<EnemyCardSelectionHandler>();
            }
            else
            {
                // 正面朝上时，添加PlayerCardSelectionHandler
                playerCard.AddComponent<PlayerCardSelectionHandler>();
            }

            SetCardVisual(playerCard, playerCardData);
            // 根据概率设置卡牌的朝向
            ShowCardFace(playerCard, !isFaceDown);  // 如果是背面朝上，则设置为false

            if (playerCardRenderer != null)
            {
                playerCardRenderer.sortingLayerName = "ObjectsOnTable";  // 将卡牌放置在ObjectsOnTable层
            }

            // 设置所有子物体的Sorting Layer
            SetSortingLayerForCard(playerCard);

            // 发牌动画
            playerCard.transform.DOMove(playerSlots[i].transform.position, moveDuration).SetEase(Ease.OutCubic).OnStart(() => PlaySound(dealCardAudio));

            // 等待动画完成后放置卡牌
            yield return new WaitForSeconds(moveDuration);

            // 将卡牌放入对应的玩家Pile
            if (playerSlots[i] != null)
            {
                playerSlots[i].AcceptCard(playerCard.GetComponent<CardDragHandler>(), null);
            }

            yield return new WaitForSeconds(dealInterval);

            // 敌人牌
            CardData enemyCardData = remainingDeck[0];
            remainingDeck.RemoveAt(0);

            // 新增：Joker模式下的花色转换
            if (IsJoker)
            {
                enemyCardData = ConvertEnemyCardForJoker(enemyCardData);
            }

            GameObject enemyCard = Instantiate(cardPrefab, deckPosition.position, Quaternion.identity);
            if (enemyCard == null) continue; // 确保卡牌实例化成功

            SpriteRenderer enemyCardRenderer = enemyCard.AddComponent<SpriteRenderer>();

            enemyCard.AddComponent<CardDragHandler>();
            enemyCard.AddComponent<EnemyCardSelectionHandler>();
            enemyCard.GetComponent<CardDragHandler>().isPlayerCard = false;  // 设置为敌人卡牌

            // SetCardVisual(enemyCard, deck[i + 7]);
            SetCardVisual(enemyCard, enemyCardData);
            ShowCardFace(enemyCard, false);
            if (enemyCardRenderer != null)
            {
                enemyCardRenderer.sortingLayerName = "ObjectsOnTable";  // 将卡牌放置在ObjectsOnTable层
            }

            // 设置所有子物体的Sorting Layer
            SetSortingLayerForCard(enemyCard);

            // 发牌动画
            enemyCard.transform.DOMove(enemySlots[i].transform.position, moveDuration).SetEase(Ease.OutCubic).OnStart(() => PlaySound(dealCardAudio));

            // 等待动画完成后放置卡牌
            yield return new WaitForSeconds(moveDuration);

            // 确保敌人卡牌已经传递给 enemySlots[i]
            if (enemySlots[i] != null)
            {
                enemySlots[i].AcceptCard(enemyCard.GetComponent<CardDragHandler>(), null);
            }

            yield return new WaitForSeconds(dealInterval);
        }
    
        if (flipController != null)
        {
            flipController.ResetFlipState();
        }

        yield return new WaitForSeconds(1f); // 等待所有动画完成

        if (enemyCardManager != null)
        {
            enemyCardManager.DistributeEnemyCards();

        }
        else
        {
            Debug.LogWarning("EnemyCardManager reference missing!");
        }
    }

    // Joker模式下的敌人牌花色转换
    private CardData ConvertEnemyCardForJoker(CardData originalData)
    {
        if (!IsJoker) return originalData; // 非Joker模式不转换

        string convertedSuit = originalData.suit switch
        {
            "flowers" => "diamonds", // ♣ → ♦
            "spades" => "hearts",    // ♠ → ♥
            _ => originalData.suit   // 其他花色不变
        };
        return new CardData(convertedSuit, originalData.rank);
    }

    public void SetSortingLayerForCard(GameObject card)
    {
        // 设置卡牌根节点的Sorting Layer和Order
        SpriteRenderer cardRenderer = card.GetComponent<SpriteRenderer>();
        if (cardRenderer != null)
        {
            cardRenderer.sortingLayerName = "ObjectsOnTable";
            cardRenderer.sortingOrder = 1; // 卡牌基础层级设为1
        }

        // 遍历卡牌所有子物体并设置Sorting Layer和Order
        foreach (Transform child in card.transform)
        {
            SetSortingOrderRecursive(child);
        }
    }

    void SetSortingOrderRecursive(Transform obj)
    {
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // 确保所有组件都设置了Sorting Layer
            spriteRenderer.sortingLayerName = "ObjectsOnTable";
            
            // 根据子物体名称设置不同层级
            switch (obj.name)
            {
                case "Back":
                case "Front":
                    spriteRenderer.sortingOrder = 2; // 卡牌正反面设为2
                    break;
                case "Type":
                case "Suit1":
                case "Suit2":
                    spriteRenderer.sortingOrder = 3; // 花色和类型设为3
                    break;
                case "Cover":  // 新增Cover层级设置
                    spriteRenderer.sortingOrder = 4; // 确保覆盖在其他元素之上
                    break;
                default:
                    spriteRenderer.sortingOrder = 1; // 其他保持基础层级
                    break;
            }
        }

        // 递归处理子物体
        foreach (Transform child in obj)
        {
            SetSortingOrderRecursive(child);
        }
    }


    public void ShowCardFace(GameObject card, bool isPlayer)
    {
        Transform front = card.transform.Find("Front");
        Transform back = card.transform.Find("Back");
        Transform cover = card.transform.Find("Front/Cover"); // 获取Cover对象

        if (front != null && back != null)
        {
            // 默认隐藏Cover
            cover.gameObject.SetActive(false);
            front.gameObject.SetActive(isPlayer);
            back.gameObject.SetActive(!isPlayer);
        }
    }

    public void SetCardVisual(GameObject card, CardData data)
    {
        Transform type = card.transform.Find("Front/Type");
        Transform suit1 = card.transform.Find("Front/Suit1");
        Transform suit2 = card.transform.Find("Front/Suit2");

        Sprite typeSprite = Resources.Load<Sprite>("CardElements/Types/" + data.rank);
        Sprite suitSprite = Resources.Load<Sprite>("CardElements/Suits/" + data.suit);

        if (type != null)
        {
            SpriteRenderer typeRenderer = type.GetComponent<SpriteRenderer>();
            if (typeRenderer != null)
            {
                typeRenderer.sprite = typeSprite; 
            }

            if (data.suit == "hearts" || data.suit == "diamonds")
            {
                typeRenderer.color = new Color(0.8f, 0.1f, 0.1f); // 红色花色（红桃、方块）
            }
            else
            {
                typeRenderer.color = Color.black; // 黑色花色（黑桃、梅花）
            }
        }

        if (suit1 != null) suit1.GetComponent<SpriteRenderer>().sprite = suitSprite;
        if (suit2 != null) suit2.GetComponent<SpriteRenderer>().sprite = suitSprite;
    }

    private void PlaySound(AudioClip clip, float volume = 0.5f)
    {
        if (clip != null && cardAudioSource != null)
        {
            cardAudioSource.PlayOneShot(clip, volume);
        }
    }
}
