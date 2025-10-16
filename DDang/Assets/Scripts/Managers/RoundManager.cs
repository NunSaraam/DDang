using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoundState
{
    WaitingRound,           //���� ��� ����
    Playing,                //�÷��� ��
    End,                    //���� ����
    Store,                  //����
}

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    public GridManager grid;
    public ScoreUIManager sM;

    public int totalRounds = 3;             //�� ���� (ex : 3�� 2��
    public float gamePlayTime = 60f;        //���� �÷��� �ð� 60��
    public float extratime = 15f;           //�߰��ð� 15�� (�÷��̾� ������ 5�� ���Ϸ� ���̳� ��)
    public float roundWaitTime = 3f;        //���� ���� �� ���ð� 3��
    public float shopingTime = 20f;         //���� ���ѽð�

    private int currentRound = 1;
    private float remainingTime;            //���� �ð�
    private bool extraTimeUsed = false;         //�߰��ð� ��Ȱ��ȭ ����

    public RoundState currentState { get; private set; } = RoundState.WaitingRound;

    private void Start()
    {
        StartCoroutine(RoundLOOP());
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator RoundLOOP()
    {
        yield return StartCoroutine(HandleWaitRound());

        yield return StartCoroutine(HandlePlaying());

        yield return StartCoroutine(HandleRoundEnd());

        yield return StartCoroutine(HandleStore());
    }

    IEnumerator HandleWaitRound()
    {
        currentState = RoundState.WaitingRound;
        Debug.Log($"{currentRound}");
        sM.NextRound(currentRound);
        yield return new WaitForSeconds(roundWaitTime);

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

            //���� ���̰� 5������ �� �߰��ð�
            int p1 = grid.CountScore(PlayerType.Player1);
            int p2 = grid.CountScore(PlayerType.Player2);
            int diff = Mathf.Abs(p1 - p2);

            if (diff <= 5 && !extraTimeUsed && remainingTime <= 0)              //���̰� 5�� �����̰�, �߰��ð� ����� ���� �ʾҰ�, ���� �÷��� �ð��� 0������ ��
            {
                remainingTime += extratime;
                extraTimeUsed = true;
            }
        }
        Debug.Log("���� �ð� ����");
    }
    
    private IEnumerator HandleRoundEnd()
    {
        currentState = RoundState.End;

        PlayerType winner = CheckWinner();

        sM.RoundResult(winner);

        yield return new WaitForSeconds(5f);
    }

    IEnumerator HandleStore()
    {
        currentState = RoundState.Store;

        //�� �Ŵ��� ����

        float storeTimer = shopingTime;
        while (storeTimer > 0)
        {
            storeTimer -= Time.deltaTime;
            yield return null;
        }
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
