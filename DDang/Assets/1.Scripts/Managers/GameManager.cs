using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Online;

public enum GameState
{
    Menu,           //메인 메뉴
    Pause,          //일시정지
    Resume,         //재개
    Gmae,           //게임
    Store,          //상점
}

// 게임 상태 (씬이 바뀌어도 남음): 일시정지·재개(시간 정지와 BGM)만 담당. 화면은 이벤트로 알림
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState currentState { get; private set; }
    public bool IsPaused => currentState == GameState.Pause;

    public event Action BackPressed;                // ESC: 열려 있는 메뉴가 받아서 처리 (받는 쪽이 없으면 아무 일도 안 함)
    public event Action<bool> PauseChanged;         // 일시정지 / 재개 → UI.PauseMenuPresenter가 메뉴를 열고 닫음

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
        if (Input.GetKeyDown(KeyCode.Escape)) BackPressed?.Invoke();
    }

    public void PauseGame()
    {
        if (IsPaused) return;
        currentState = GameState.Pause;
        PauseChanged?.Invoke(true);
        if (OnlineSession.IsOnline) return;         // 온라인: 상대 시간은 못 멈추므로 메뉴만 열고 게임은 계속

        Time.timeScale = 0f;
        if (SoundManager.Instance != null) SoundManager.Instance.PauseBGM();
    }

    public void ResumeGame()
    {
        bool wasPaused = IsPaused;
        currentState = GameState.Resume;
        Time.timeScale = 1f;
        if (wasPaused && SoundManager.Instance != null) SoundManager.Instance.ResumeBGM();
        PauseChanged?.Invoke(false);
    }
}
