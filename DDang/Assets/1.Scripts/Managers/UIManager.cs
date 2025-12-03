 using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject pausePanel;
    public Image bgmSlider;
    public Button bgmMinusButton;
    public Button bgmPlusButton;

    public TextMeshProUGUI roundWaitText;           //대기시간 텍스트
    public TextMeshProUGUI roundTimeText;           //진행시간 텍스트

    public GameObject roundWaitPanel;

    public Image[] player1WinImages;
    public Image[] player2WinImages;

    public Sprite defaultSprite;
    public Sprite player1WinSpreite;
    public Sprite player2WinSpreite;

    private int p1RoundWins = 0;
    private int p2RoundWins = 0;

    private float currentRoundWaitTime;
    private int currentRound = 1;
    private float currentRoundTime;

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

        pausePanel.SetActive(false);
    }


    private void Update()
    {
        if (pausePanel == null)
        {
            pausePanel = GameObject.Find("Setting");
            pausePanel.SetActive(false);
        }

        if (RoundManager.Instance != null && RoundManager.Instance.currentState != RoundState.WaitingRound) return;
    }

    public void ShowPausePanel(bool show)
    {
        pausePanel.SetActive(show);

        if (show)
            pausePanel.GetComponent<PausePanel>()?.RefreshUI();
    }

    public void RoundResult(PlayerType winner)
    {
        switch (winner)
        {
            case PlayerType.Player1:
                p1RoundWins++;
                break;
            case PlayerType.Player2:
                p2RoundWins++;
                break;
        }

        UpdateWinIcon();
        UpdateRoundUI();
    }

    public void ReconnectUI()          //다시 게임 씬으로 넘어올 때 재연결
    {


        roundWaitText = GameObject.Find("StartWaitText")?.GetComponent<TextMeshProUGUI>();
        roundTimeText = GameObject.Find("Round_Time_Text")?.GetComponent<TextMeshProUGUI>();
        roundWaitPanel = GameObject.Find("StartWaitPanel");

        GameObject p1Group = GameObject.Find("Player1_Win_Images");
        if (p1Group != null)
        {
            player1WinImages = p1Group.GetComponentsInChildren<Image>();
        }

        GameObject p2Group = GameObject.Find("Player2_Win_Images");
        if (p2Group != null)
        {
            player2WinImages = p2Group.GetComponentsInChildren<Image>();
        }


        if (IsUIReady())
        {
            SetRoundWins(RoundManager.Instance.p1roundWinCount, RoundManager.Instance.p2roundWinCount);
            UpdateRoundUI();
        }
    }

    public void UpdateRoundUI()
    {
        if (!IsUIReady()) return;

        roundWaitText.text = $"{currentRoundWaitTime:F1}";
        roundTimeText.text = $"{currentRoundTime:F2}";
    }

    public void NextRound(int roundNumber)              //RoundManager호출용
    {
        currentRound = roundNumber;
        UpdateRoundUI();  
    }

    public void RoundWait(float roundWait)
    {
        currentRoundWaitTime = roundWait;
        UpdateRoundUI();
    }

    public void RoundTime(float roundTime)
    {
        currentRoundTime = roundTime;
        UpdateRoundUI(); 
    }


    public void ResetAll()
    {
        p1RoundWins = 0;
        p2RoundWins = 0;
        currentRound = 1;

        UpdateWinIcon();
        UpdateRoundUI();
    }

    private void UpdateWinIcon()
    {
        for (int i = 0; i < player1WinImages.Length; i++)
        {
            if (i < p1RoundWins)
            {
                player1WinImages[i].sprite = player1WinSpreite;
            }
            else
            {
                player1WinImages[i].sprite = defaultSprite;
            }
        }

        for (int i = 0; i < player2WinImages.Length; i++)
        {
            if (i < p2RoundWins)
            {
                player2WinImages[i].sprite = player2WinSpreite;
            }
            else
            {
                player2WinImages[i].sprite = defaultSprite;
            }
        }
    }

    public void SetRoundWins(int p1Wins, int p2Wins)
    {
        this.p1RoundWins = p1Wins;
        this.p2RoundWins = p2Wins;
        UpdateWinIcon();
    }

    public bool IsUIReady()
    {
        return roundWaitText != null &&
               roundTimeText != null &&
               roundWaitPanel != null &&
               player1WinImages != null && player1WinImages.Length > 0 &&
               player2WinImages != null && player2WinImages.Length > 0;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            StartCoroutine(DelayedReconnect());
        }
    }

    IEnumerator DelayedReconnect()
    {
        yield return null;
        ReconnectUI();
    }

    public void ResetAllUI()
    {
        roundWaitText.text = "";
        roundTimeText.text = "";

        SetRoundWins(0, 0);
    }
}
