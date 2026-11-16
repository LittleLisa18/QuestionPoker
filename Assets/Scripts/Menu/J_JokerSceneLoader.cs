using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class J_JokerSceneLoader : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()    
    {
        //停3s
        yield return new WaitForSeconds(2f);

        //加载Table_Joker场景
        LoadSceneController loadSceneController = new LoadSceneController();
        loadSceneController.nextSceneName = "Table_Joker";  
        loadSceneController.LoadNextScene();
    }
}
