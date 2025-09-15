using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//���� ����
public enum GameState
{
    Menu,               //���θ޴�
    Playing,            //���� �÷��� ��
    Paused,             //�Ͻ����� ����
    Loading,            //�ε�
    GameOver            //���� ���� ����
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
        Application.targetFrameRate = 60;                           //������ ����
        Screen.sleepTimeout = SleepTimeout.NeverSleep;              //�ڵ����� ȭ���� ������ �ʵ���

        Debug.Log("GameManager �ʱ�ȭ �Ϸ�");
    }

    private IEnumerator InitializeManagers()
    {
        yield return new WaitForEndOfFrame();                       //���� ������ �������� ���� �� �ٸ� �Ŵ��� Ȯ��

        if (SceneManager.Instance != null)
            Debug.Log("���� Ȯ��");
    }

    private void HandleInput()
    {
        //ESCŰ�� ���� �Ͻ�����
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
        
        Debug.Log($"���� ���� ���� : {previousGameState} -> {currentState}");
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
