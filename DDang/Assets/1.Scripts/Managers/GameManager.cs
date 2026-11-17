using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;                                     // 중복이면 아래 코드 실행 안 함
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        currentState = GameState.Menu;
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    // 씬이 바뀌면 일시정지 상태를 항상 풀어 줌
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this) return;
        if (currentState == GameState.Pause && SoundManager.Instance != null) SoundManager.Instance.ResumeBGM();
        Time.timeScale = 1f;
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
        if (!UIManager.Instance.ShowPausePanel(true)) return;     // 일시정지창 없는 씬(로딩, 결과)은 무시

        currentState = GameState.Pause;
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
