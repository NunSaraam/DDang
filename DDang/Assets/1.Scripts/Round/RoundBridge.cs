using System.Collections;
using UnityEngine;

namespace Rules
{
    [RequireComponent(typeof(RoundManager))]
    public class RoundBridge : MonoBehaviour
    {
        private RoundManager round;

        private void Awake() => round = GetComponent<RoundManager>();

        private void OnEnable()
        {
            round.RoundStarted += OnRoundStarted;
            round.PhaseChanged += OnPhaseChanged;
            round.RemainingTimeChanged += OnRemainingTimeChanged;
            round.QuestTimeReached += OnQuestTime;
            round.RoundEnded += OnRoundEnded;
            round.MatchProgressed += OnMatchProgressed;
        }

        private void OnDisable()
        {
            round.RoundStarted -= OnRoundStarted;
            round.PhaseChanged -= OnPhaseChanged;
            round.RemainingTimeChanged -= OnRemainingTimeChanged;
            round.QuestTimeReached -= OnQuestTime;
            round.RoundEnded -= OnRoundEnded;
            round.MatchProgressed -= OnMatchProgressed;
        }

        private void OnRoundStarted(int roundNumber)
        {
            UIManager.Instance.NextRound(roundNumber);
            UIManager.Instance.roundWaitPanel.SetActive(true);
        }

        private void OnPhaseChanged(RoundPhase phase)
        {
            if (phase == RoundPhase.Playing) UIManager.Instance.roundWaitPanel.SetActive(false);
        }

        private void OnRemainingTimeChanged(float time)
        {
            if (round.Phase == RoundPhase.Countdown) UIManager.Instance.RoundWait(time);
            else if (round.IsRoundActive) UIManager.Instance.RoundTime(time);
        }

        private void OnQuestTime() => QuestManager.Instance.ActivateQuest(QuestType.StunChain);


        private void OnRoundEnded(RoundResult result)
        {
            QuestManager.Instance?.OnRoundEnd(result.P1Tiles, result.P2Tiles, result.Winner);
            QuestManager.Instance?.ResetAllQuest();         // 미완료 퀘스트 정리 -> 다음 라운드에 새로 제시

            AchievementManager.Instance.AddValue(result.Winner, AchievementType.TotalWins, 1);
        }

        private void OnMatchProgressed(MatchProgress progress)
        {
            UIManager.Instance.SetRoundWins(round.Match.P1Wins, round.Match.P2Wins);
        }
    }
}
