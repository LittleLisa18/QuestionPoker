using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class TableBgmManager : MonoBehaviour
{
    [Header("BGM Settings")]
    public AudioClip bgmClip;
    [Range(0.1f, 5f)] public float fadeInDuration = 2f;
    [Range(0.1f, 5f)] public float fadeOutDuration = 2f;
    [Range(0f, 1f)] public float maxVolume = 1f;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = bgmClip;
        audioSource.loop = false;
        audioSource.volume = 0f; // 初始静音
    }

    void Start()
    {
        StartBGM();
    }

    public void StartBGM()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        audioSource.Play();
        fadeCoroutine = StartCoroutine(FadeIn());

        // 播完后启用循环
        Invoke("EnableLoop", bgmClip.length - fadeOutDuration);
    }

    void EnableLoop()
    {
        audioSource.loop = true;
        fadeCoroutine = StartCoroutine(FadeOutInTransition());
    }

    IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            audioSource.volume = Mathf.Lerp(0f, maxVolume, timer / fadeInDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = maxVolume;
    }

    IEnumerator FadeOutInTransition()
    {
        // 淡出当前播放
        float timer = 0f;
        while (timer < fadeOutDuration)
        {
            audioSource.volume = Mathf.Lerp(maxVolume, 0f, timer / fadeOutDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        // 重新播放并淡入
        audioSource.volume = 0f;
        audioSource.Play();
        yield return FadeIn();
    }

    public void StopBGM()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float timer = 0f;
        
        while (timer < fadeOutDuration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        
        audioSource.Stop();
        audioSource.volume = maxVolume; // 重置音量
    }

    void OnDestroy()
    {
        // 确保停止所有协程
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
    }
}