using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//게임 상태
public enum GameState
{
    Menu,               //메인메뉴
    Playing,            //게임 플레이 중
    Paused,             //일시정지 상태
    Loading,            //로딩
    GameOver            //게임 종료 상태
}

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    public GameState currentState = GameState.Menu;
    public bool isGamePaused = false;

    public float gameTime = 0f;

    private GameState previousGameState;

    protected override void Awake()
    {
        base.Awake();
        InitializeGame();
    }

    private void Start()
    {
        StartCoroutine(InitializeManagers());
    }

    private void Update()
    {
        if (currentState == GameState.Playing && !isGamePaused)
        {
            gameTime += Time.deltaTime;
        }

        HandleInput();
    }

    private void InitializeGame()
    {
        Application.targetFrameRate = 60;                           //프레임 제한
        Screen.sleepTimeout = SleepTimeout.NeverSleep;              //자동으로 화면이 꺼지지 않도록

        Debug.Log("GameManager 초기화 완료");
    }

    private IEnumerator InitializeManagers()
    {
        yield return new WaitForEndOfFrame();                       //현재 프레임 랜더링이 끝난 후 다른 매니저 확인

        if (SceneManager.Instance != null)
            Debug.Log("연결 확인");
    }

    private void HandleInput()
    {
        //ESC키로 게임 일시정지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState != GameState.Paused)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }

    public void ChangeGameState(GameState newstate)
    {
        if (currentState == newstate) return;

        previousGameState = currentState;
        currentState = newstate;

        OnGameStateChanged(newstate);
        
        Debug.Log($"게임 상태 변경 : {previousGameState} -> {currentState}");
    }

    private void OnGameStateChanged(GameState newstate)
    {
        switch (newstate)
        {
            case GameState.Menu:
                Cursor.lockState = CursorLockMode.None; 
                break;
            case GameState.Playing:
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case GameState.Paused:
                Cursor.lockState = CursorLockMode.None;
                break;

            case GameState.Loading:
                Cursor.lockState = CursorLockMode.None;
                break;

            case GameState.GameOver:
                Cursor.lockState = CursorLockMode.Locked;
                break; 
        }
    }

    public void StartGame()
    {
        ResetGameStats();
        ChangeGameState(GameState.Playing);
        GameEvents.GameResumed();
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;

        isGamePaused = true;
        ChangeGameState(GameState.Paused);
        GameEvents.GamePaused();
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;

        isGamePaused = false;
        ChangeGameState(GameState.Playing);
        GameEvents.GameResumed();
    }

    public void GameOver()
    {
        ChangeGameState(GameState.GameOver);
    }

    public void RestartGame()
    {
        ResetGameStats();
        ChangeGameState(GameState.Playing);
    }

    public void GoToMainMenu()
    {
        ChangeGameState(GameState.Menu);
    }

    private void ResetGameStats()
    {
        gameTime = 0f;
    }
}
