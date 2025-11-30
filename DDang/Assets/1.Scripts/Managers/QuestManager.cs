using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public QuestSO[] activeQuest;

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

    public void ResetAllQuest()
    {
        if (activeQuest == null) return;

        foreach (var quest in activeQuest)
        {
            quest.ResetQuest();
        }
    }

    public void OnStunLanded(PlayerType attacker)
    {
        if (activeQuest == null) return;

        foreach (var quest in activeQuest)
        {
            quest.OnStunLanded(attacker);
        }
    }

    public void OnExtraTImeStarted(int p1Score, int p2Score)
    {
        if (activeQuest == null) return;

        foreach (var quest in activeQuest)
        {
            quest.OnExtraTimeStarted(p1Score, p2Score);
        }
    }

    public void OnRoundEnd(int p1Score, int p2Score, PlayerType winner)
    {
        if (activeQuest == null) return;

        foreach (var quest in activeQuest)
        {
            quest.OnRoundEnd(p1Score, p2Score, winner);
        }
    }

    public void CompleteQuest(QuestSO quest, PlayerType player)
    {
        if (player == PlayerType.None) return;

        CoinManager.Instance.AddCoins(player, quest.rewardCoins);
    }
}
