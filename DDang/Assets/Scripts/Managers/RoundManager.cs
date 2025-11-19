using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoundState
{
    WaitingRound,           //라운드 대기 상태
    Playing,                //플레이 중
    End,                    //라운드 종료
    Store,                  //상점
}

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    public GridManager grid;
    public ScoreUIManager sM;

    public int totalRounds = 3;                 //총 라운드 (ex : 3판 2선
    public float gamePlayTime = 60f;            //라운드 플레이 시간 60초
    public float extratime = 15f;               //추가시간 15초 (플레이어 점수가 5점 이하로 차이날 때)
    public float roundWaitTime = 3f;            //라운드 시작 전 대기시간 3초
    public float shopingTime = 20f;             //상점 제한시간

    private int currentRound = 1;
    private float remainingWaitTime;
    private float remainingTime;                //남은 시간
    private bool extraTimeUsed = false;         //추가시간 비활성화 상태

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

        yield return StartCoroutine(HandleStore());
    }

    IEnumerator HandleWaitRound()
    {

        while (grid == null)
        {
            grid = GameObject.Find("GridManager")?.GetComponent<GridManager>();
            if (grid == null)
            {
                yield return null;
            }
        }
        Debug.Log("그리드 참조 완료");


        Debug.Log("웨잇 라운드 카운트 시작");
        sM.NextRound(currentRound);
        currentState = RoundState.WaitingRound;
        remainingWaitTime = roundWaitTime;
        Debug.Log($"{currentRound}");

        while (remainingWaitTime > 0)
        {
            sM.RoundWait(remainingWaitTime);
            remainingWaitTime -= Time.deltaTime;

            yield return null;

        }
        sM.roundWaitText.text = "Start!";
        yield return new WaitForSeconds(.5f);
        sM.RoundWait(0);
        sM.roundWaitPanel.SetActive(false);
    }

    IEnumerator HandlePlaying()
    {
        currentState = RoundState.Playing;
        remainingTime = gamePlayTime;
        extraTimeUsed = false;

        while (remainingTime > 0)
        {
            sM.RoundTime(remainingTime);
            remainingTime -= Time.deltaTime;
            yield return null;

            //점수 차이가 5이하일 때 추가시간
            int p1 = grid.CountScore(PlayerType.Player1);
            int p2 = grid.CountScore(PlayerType.Player2);
            int diff = Mathf.Abs(p1 - p2);

            if (diff <= 5 && !extraTimeUsed && remainingTime <= 0)              //차이가 5점 이하이고, 추가시간 사용을 하지 않았고, 게임 플레이 시간이 0이하일 때
            {
                remainingTime += extratime;
                extraTimeUsed = true;
            }
        }
        Debug.Log("라운드 시간 종료");
    }
    
    private IEnumerator HandleRoundEnd()
    {
        currentState = RoundState.End;

        PlayerType winner = CheckWinner();
        sM.RoundResult(winner);

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

        yield return new WaitForSeconds(5f);
    }

    IEnumerator HandleStore()
    {
        currentState = RoundState.Store;

        //씬 매니저 연결
        SceneLoadManager.Instance.LoadScene("Store");

        StoreUIManager sT = null;
        while ((sT = FindObjectOfType<StoreUIManager>()) == null)
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

        //SceneLoadManager.Instance.LoadScene("GameScene");
    }


    PlayerType CheckWinner()
    {
        int p1Score = grid.CountScore(PlayerType.Player1);
        int p2Score = grid.CountScore(PlayerType.Player2);

        if (p1Score > p2Score) return PlayerType.Player1;
        if (p1Score < p2Score) return PlayerType.Player2;
        return PlayerType.None;
    }
}
