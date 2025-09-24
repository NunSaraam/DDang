using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestScoreUI : MonoBehaviour
{
    public GridManager grid;
    public TextMeshProUGUI player1Score;
    public TextMeshProUGUI player2Score;

    private void Update()
    {
        int p1Score = grid.CountScore(PlayerType.Player1);
        int p2Score = grid.CountScore(PlayerType.Player2);

        player1Score.text = $"P1 : {p1Score}";
        player2Score.text = $"P2 : {p2Score}";
    }
}
