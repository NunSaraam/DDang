using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoundState
{
    WaitingRound,           //라운드 대기 상태
    Playing,                //플레이 중
    End,                    //라운드 종료
    Store,                  //상점
    Result,                 //게임 결과
}

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    public GridGenerator grid;

    public int winningPoint = 3;                //총 라운드 (ex : 5판 3선
    public float gamePlayTime = 60f;            //라운드 플레이 시간 60초
    public float extratime = 15f;               //추가시간 15초 (플레이어 점수가 5점 이하로 차이날 때)
    public float roundWaitTime = 3f;            //라운드 시작 전 대기시간 3초
    public float shopingTime = 20f;             //상점 제한시간

    private int currentRound = 1;
    private float remainingWaitTime;
    private float remainingTime;                //남은 시간
    private bool extraTimeUsed = false;         //추가시간 비활성화 상태

    public int p1roundWinCount = 0;
    public int p2roundWinCount = 0;

    public PlayerType gameWinner;

    private bool bossSpawned = false;

    private bool stunQuestActive;
    private bool reverseQuestActive;

    public RoundState currentState { get; private set; } = RoundState.WaitingRound;


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
    }

    public void Begin()
    {
        StartCoroutine(RoundLOOP());
    }

    IEnumerator RoundLOOP()
    {
        Debug.Log("라운드 루프 시작");

        Debug.Log("웨잇 라운드 루프 시작");
        yield return StartCoroutine(HandleWaitRound());

        yield return StartCoroutine(HandlePlaying());

        yield return StartCoroutine(HandleRoundEnd());

        if (currentState != RoundState.Result) 
        {
            yield return StartCoroutine(HandleStore());
        }
    }

    IEnumerator HandleWaitRound()
    {
        while (grid == null)
        {
            grid = GameObject.Find("GridGenerator")?.GetComponent<GridGenerator>();
            if (grid == null)
            {
                yield return null;
            }
        }
        Debug.Log("Grid 참조 완료");


        Debug.Log("WaitRound 카운트 시작");
        UIManager.Instance.NextRound(currentRound);
        currentState = RoundState.WaitingRound;
        remainingWaitTime = roundWaitTime;
        Debug.Log($"Round: {currentRound}");

        while (remainingWaitTime > 0)
        {
            UIManager.Instance.RoundWait(remainingWaitTime);
            remainingWaitTime -= Time.deltaTime;

            yield return null;

        }
        UIManager.Instance.roundWaitText.text = "Start!";
        yield return new WaitForSeconds(.5f);
        UIManager.Instance.RoundWait(0);
        UIManager.Instance.roundWaitPanel.SetActive(false);
    }

    IEnumerator HandlePlaying()
    {
        currentState = RoundState.Playing;
        remainingTime = gamePlayTime;
        extraTimeUsed = false;

        StartCoroutine(BossTimer());

        while (remainingTime > 0)
        {
            if (!stunQuestActive && remainingTime <= 25f)
            {
                stunQuestActive = true;
                QuestManager.Instance.ActivateQuest(QuestType.StunChain);
            }

            if (!reverseQuestActive && remainingTime <= 10f)
            {

                int p1 = grid.CountScore(PlayerType.Player1);
                int p2 = grid.CountScore(PlayerType.Player2);
                int diff = Mathf.Abs(p1 - p2);

                if (diff <= 10)
                {
                    reverseQuestActive = true;
                    QuestManager.Instance.ActivateQuest(QuestType.Reverse);
                }
            }

            UIManager.Instance.RoundTime(remainingTime);
            remainingTime -= Time.deltaTime;
            yield return null;

            //점수 차이가 5이하일 때 추가시간
            int p1Score = grid.CountScore(PlayerType.Player1);
            int p2Score = grid.CountScore(PlayerType.Player2);
            int scoreDiff = Mathf.Abs(p1Score - p2Score);

            if (scoreDiff <= 5 && !extraTimeUsed && remainingTime <= 0)              //차이가 5점 이하이고, 추가시간 사용을 하지 않았고, 게임 플레이 시간이 0이하일 때
            {
                QuestManager.Instance?.OnExtraTImeStarted(p1Score, p2Score);

                remainingTime += extratime;
                extraTimeUsed = true;
            }
        }
        Debug.Log("라운드 시간 종료");
    }
    
    private IEnumerator HandleRoundEnd()
    {
        currentState = RoundState.End;

        bossSpawned = false;

        stunQuestActive = false;
        reverseQuestActive = false;

        int p1Score = grid.CountScore(PlayerType.Player1);
        int p2Score = grid.CountScore(PlayerType.Player2);

        PlayerType winner = CheckWinner();

        UIManager.Instance.RoundResult(winner);

        QuestManager.Instance?.OnRoundEnd(p1Score, p2Score, winner);

        if (winner != PlayerType.None)
        {
            CoinManager.Instance.AddCoins(winner, 10);
        }

        PlayerType loser = (winner == PlayerType.Player1) ? PlayerType.Player2 :
                           (winner == PlayerType.Player2) ? PlayerType.Player1 :
                           PlayerType.None;

        if (loser != PlayerType.None)
        {
            CoinManager.Instance.AddCoins(loser, 5);
        }


        switch (winner)
        {
            case PlayerType.Player1:
                if (p1roundWinCount < winningPoint)
                {
                    p1roundWinCount++;
                }
                break;

            case PlayerType.Player2:
                if (p2roundWinCount < winningPoint)
                {
                    p2roundWinCount++;
                }
                break;
        }

        Debug.Log($"{currentRound} 승자 = {winner}");

        currentRound++;
        UIManager.Instance.SetRoundWins(p1roundWinCount, p2roundWinCount);

        if (winner == PlayerType.Player1 && p1roundWinCount >= winningPoint)
        {
            gameWinner = PlayerType.Player1;
            currentState = RoundState.Result;

            SceneLoadManager.Instance.LoadScene("GameResult");
        }
        else if (winner == PlayerType.Player2 && p2roundWinCount >= winningPoint)
        {
            gameWinner = PlayerType.Player2;
            currentState = RoundState.Result;

            SceneLoadManager.Instance.LoadScene("GameResult");
        }

        yield return new WaitForSeconds(5f);

    }

    IEnumerator HandleStore()
    {
        currentState = RoundState.Store;

        //씬 매니저 연결
        SceneLoadManager.Instance.LoadScene("Store");

        StoreUI sT = null;
        while ((sT = FindObjectOfType<StoreUI>()) == null)
        {
            yield return null;
        }

        float storeTimer = shopingTime;
        while (storeTimer > 0)
        {
            sT.ShopingTime(storeTimer);
            storeTimer -= Time.deltaTime;
            yield return null;
        }

        SceneLoadManager.Instance.LoadScene("GameScene");

        currentState = RoundState.WaitingRound;
    }

    IEnumerator BossTimer()
    {
        if (bossSpawned) yield break;
        while (remainingTime > 33f)
            yield return null;

        bossSpawned = true;

        //UIManager.Instance.ShowBossWarning(true);

        yield return new WaitForSeconds(3f);

        //UIManager.Instance.ShowBossWarning(false);

        BossManager.Instance.SpawnRandomBoss();
    }

    PlayerType CheckWinner()
    {
        int p1Score = grid.CountScore(PlayerType.Player1);
        int p2Score = grid.CountScore(PlayerType.Player2);

        if (p1Score > p2Score) return PlayerType.Player1;
        if (p1Score < p2Score) return PlayerType.Player2;
        return PlayerType.None;
    }

    public void ResetAll()
    {
        currentRound = 1;
        p1roundWinCount = 0;
        p2roundWinCount = 0;

        extraTimeUsed = false;
        bossSpawned = false;

        gameWinner = PlayerType.None;

        stunQuestActive = false;
        reverseQuestActive = false;

        foreach (var player in FindObjectsOfType<PlayerMovement>())
        {
            player.ResetStats();
        }

        currentState = RoundState.WaitingRound;

        // 다른 매니저들도 초기화
        QuestManager.Instance?.ResetAllQuest();
        CoinManager.Instance?.ResetCoins();
        UIManager.Instance?.ResetAllUI();
    }
}
