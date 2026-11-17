using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    public static SceneLoadManager Instance { get; private set; }

    public string targetSceneName;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);                        // 새로 생긴 중복을 지움
        }
    }

    public void LoadScene(string sceneName)
    {
        targetSceneName = sceneName;
        SceneManager.LoadScene(targetSceneName);
    }

    public void LoadSceneWithLoading(string sceneName)
    {
        StartCoroutine(AsyncWithLoading(sceneName));
    }

    private IEnumerator AsyncWithLoading(string targetName)
    {
        SceneManager.LoadScene("LoadingScene");

        yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < .9f)
        {
            yield return null; 
        }

        Debug.Log($"로딩완료");
        yield return new WaitForSecondsRealtime(1);     // 게임 시간이 멈춰 있어도 진행

        asyncLoad.allowSceneActivation = true;
    }

}
