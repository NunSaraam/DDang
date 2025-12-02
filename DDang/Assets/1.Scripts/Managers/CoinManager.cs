using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public int player1Coins = 0;
    public int player2Coins = 0;

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

    public void AddCoins(PlayerType player, int amount)
    {
        if (player == PlayerType.Player1)
        {
            player1Coins += amount;
        }
        else if (player == PlayerType.Player2)
        {
            player2Coins += amount;
        }
    }

    public int GetCoins(PlayerType player)
    {
        return (player == PlayerType.Player1) ? player1Coins : player2Coins;
    }

    public void SpendCoins(PlayerType player, int amount)
    {
        if (player == PlayerType.Player1)
        {
            player1Coins = Mathf.Max(0, player1Coins - amount);
        }
        else if (player == PlayerType.Player2)
        {
            player2Coins = Mathf.Max(0, player2Coins - amount);
        }
    }

    public void ResetCoins()
    {
        player1Coins = 0;
        player2Coins = 0;
    }
}
