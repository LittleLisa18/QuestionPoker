using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoSceneHandler : MonoBehaviour
{
    public string nextSceneName;

    void Start()
    {
        GetComponent<VideoPlayer>().loopPointReached += OnVideoEnd;
    }

    // 视频结束时调用
    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}