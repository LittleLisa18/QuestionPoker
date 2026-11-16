using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))] // 确保组件存在
public class ButtonAudioHandler : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip audioClipCancel;
    public AudioClip audioClipChoose;
    public AudioClip audioClipSide;
    public AudioClip audioClipChipThrow;
    public AudioClip audioClipCard;

    private AudioSource _audioSource;

    void Awake()
    {
        // 自动获取AudioSource组件
        _audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // 确保音频剪辑已分配
        if ((audioClipCancel == null) || (audioClipChoose == null))
        {
            Debug.LogError("Audio clip not assigned for the selected button type!");
            return;
        }
    }

    public void PlayAudioCancel()
    {
        _audioSource.PlayOneShot(audioClipCancel);
    }

    public void PlayAudioChoose()
    {
        _audioSource.PlayOneShot(audioClipChoose);
    }

    public void PlayAudioSide()
    {
        _audioSource.PlayOneShot(audioClipSide);
    }

    public void PlayAudioChipThrow()
    {
        _audioSource.PlayOneShot(audioClipChipThrow);
    }

    public void PlayAudioCard()
    {
        _audioSource.PlayOneShot(audioClipCard);
    }
}