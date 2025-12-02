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
            Destroy(Instance);
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
        yield return new WaitForSeconds(1);

        asyncLoad.allowSceneActivation = true;
    }

}
