using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Store;

// 퀘스트 진행 (모델). 화면은 직접 부르지 않고 이벤트로 알림 → UI.QuestPresenter가 패널에 반영
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    public QuestSO[] questData;

    private List<QuestSO> activeQuest = new List<QuestSO>();

    public event Action<QuestSO> QuestStarted;                          // 퀘스트 시작 (패널에 추가)
    public event Action<QuestSO, int, int> QuestProgressed;             // 진행도 (P1, P2)
    public event Action<QuestSO, string, string> QuestLinesChanged;     // 퀘스트별 안내 문구 (역전 퀘스트)
    public event Action<QuestSO, PlayerType, int> QuestFinished;        // 완료 (None이면 정리만)

    // 퀘스트 데이터(SO)가 부름 → 이벤트로 알림
    public void ReportProgress(QuestSO qs, int p1, int p2) => QuestProgressed?.Invoke(qs, p1, p2);
    public void ReportLines(QuestSO qs, string p1, string p2) => QuestLinesChanged?.Invoke(qs, p1, p2);

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

        QuestStarted?.Invoke(qs);
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
            QuestFinished?.Invoke(quest, PlayerType.None, 0);
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

        for (int i = activeQuest.Count - 1; i >= 0; i--)        // 완료 시 목록에서 빠지므로 역순 순회
        {
            activeQuest[i].OnRoundEnd(p1Score, p2Score, winner);
        }
    }

    public void CompleteQuest(QuestSO qs, PlayerType player)
    {
        if (player == PlayerType.None) return;

        CoinManager.Instance.AddCoins(player, qs.rewardCoins);

        QuestFinished?.Invoke(qs, player, qs.rewardCoins);

        activeQuest.Remove(qs);

        qs.ResetQuest();
    }
}
