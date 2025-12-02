using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public QuestSO[] questData;

    private List<QuestSO> activeQuest = new List<QuestSO>();

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

    public void ActivateQuest(QuestType type)
    {
        QuestSO qs = FindQuestByType(type);
        if (qs == null)
        {
            return;
        }

        if (activeQuest.Contains(qs))
        {
            return;
        }

        activeQuest.Add(qs);
        qs.ResetQuest();

        QuestUI.Instance.CreateQuest(qs);
    }

    private QuestSO FindQuestByType(QuestType type)
    {
        foreach (var q in questData)
        {
            if (q != null && q.type == type)
                return q;
        }
        return null;
    }

    public void ResetAllQuest()
    {
        if (activeQuest == null) return;

        foreach (var quest in activeQuest)
        {
            quest.ResetQuest();
            QuestUI.Instance.CompleteQuestUI(quest, PlayerType.None, 0);
        }

        activeQuest.Clear();
    }

    public void OnStunLanded(PlayerType attacker)
    {
        if (activeQuest == null) return;

        for (int i = activeQuest.Count - 1; i >= 0; i--)
        {
            activeQuest[i].OnStunLanded(attacker);
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

    public void CompleteQuest(QuestSO qs, PlayerType player)
    {
        if (player == PlayerType.None) return;

        CoinManager.Instance.AddCoins(player, qs.rewardCoins);

        QuestUI.Instance.CompleteQuestUI(qs, player, qs.rewardCoins);

        activeQuest.Remove(qs);

        qs.ResetQuest();
    }
}
