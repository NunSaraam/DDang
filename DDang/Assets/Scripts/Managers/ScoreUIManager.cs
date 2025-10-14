using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUIManager : MonoBehaviour
{
    public static ScoreUIManager Instance { get; private set; }

    public GridManager grid;
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;

    public TextMeshProUGUI player1WinCountText;
    public TextMeshProUGUI player2WinCountText;

    public TextMeshProUGUI winnerText;


    public TextMeshProUGUI roundText;

    private int p1RoundWins = 0;
    private int p2RoundWins = 0;
    private int currentRound = 1;

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

    private void Update()
    {
        if (grid == null) return;

        int p1Score = grid.CountScore(PlayerType.Player1);
        int p2Score = grid.CountScore(PlayerType.Player2);

        player1ScoreText.text = $"{p1Score}";
        player2ScoreText.text = $"{p2Score}";
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

        UpdateRoundUI();
    }



    public void UpdateRoundUI()
    {
        player1WinCountText.text = $"{p1RoundWins}";
        player2WinCountText.text = $"{p2RoundWins}";
        roundText.text = $"{currentRound}";
    }

    public void NextRound(int roundNumber)              //RoundManager»£√‚øÎ
    {
        currentRound = roundNumber;
        UpdateRoundUI();  
    }

    public void ResetAll()
    {
        p1RoundWins = 0;
        p2RoundWins = 0;
        currentRound = 1;

        UpdateRoundUI();
    }
}
