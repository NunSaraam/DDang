using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Menu,           //메인 메뉴
    Pause,          //일시정지
    Resume,         //재개
    Gmae,           //게임
    Store,          //상점
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState currentState {  get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        currentState = GameState.Menu;
    }

    private void Update()
    {
        TogglePause();
    }

    void TogglePause()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            currentState = GameState.Pause;
            UIManager.Instance.pausePanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else if (Input.GetKey(KeyCode.Escape) && currentState == GameState.Pause)
        {
            currentState = GameState.Resume;
            UIManager.Instance.pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
