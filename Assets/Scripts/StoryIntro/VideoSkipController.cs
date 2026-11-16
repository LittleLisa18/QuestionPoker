using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI; // 用于 Button 交互
using UnityEngine.SceneManagement;

public class VideoSkipController : MonoBehaviour
{
    public VideoPlayer videoPlayer; // 拖拽赋值
    public Button skipButton;      // 拖拽赋值
    public string nextSceneName = "Login";

    void Start()
    {
        // 自动获取（如果挂载在同一个 GameObject）
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (skipButton == null)
            skipButton = GetComponent<Button>();
        
        if (nextSceneName == null)
            nextSceneName = "Login";

        // 绑定按钮点击事件
        skipButton.onClick.AddListener(SkipVideo);
    }

    // 跳过视频
    public void SkipVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop(); // 停止播放
            videoPlayer.gameObject.SetActive(false); // 隐藏（可选）

            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }
}