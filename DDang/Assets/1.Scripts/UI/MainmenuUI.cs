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
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();   // 일시정지 상태까지 같이 해제
        else
        {
            Time.timeScale = 1f;
            SoundManager.Instance.ResumeBGM();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
