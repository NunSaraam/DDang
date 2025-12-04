using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MainmenuUI : MonoBehaviour
{
    public GameObject SettingPanel;
    public void GameStart(string SceneName)
    {
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.LoadSceneWithLoading(SceneName);
        }
    }

    public void CloseSetting()
    {
        SettingPanel.SetActive(false);
        Time.timeScale = 1.0f;
        SoundManager.Instance.ResumeBGM();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
