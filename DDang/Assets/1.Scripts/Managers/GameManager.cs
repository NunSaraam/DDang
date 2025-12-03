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

    public GameState currentState { get; private set; }

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
        HandlePauseInput();
    }

    void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Pause)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        currentState = GameState.Pause;

        UIManager.Instance.ShowPausePanel(true);
        Time.timeScale = 0f;

        SoundManager.Instance.PauseBGM();
    }

    public void ResumeGame()
    {
        currentState = GameState.Resume;

        UIManager.Instance.ShowPausePanel(false);
        Time.timeScale = 1f;

        SoundManager.Instance.ResumeBGM();
    }
}
