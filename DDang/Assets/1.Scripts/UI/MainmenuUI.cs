using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MainmenuUI : MonoBehaviour
{
    
    public void GameStart(string SceneName)
    {
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.LoadSceneWithLoading(SceneName);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
