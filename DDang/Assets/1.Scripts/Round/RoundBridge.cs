using System.Collections;
using UnityEngine;

namespace Rules
{
    [RequireComponent(typeof(RoundManager))]
    public class RoundBridge : MonoBehaviour
    {
        [SerializeField] private float storeTime = 30f;

        private RoundManager round;

        private void Awake() => round = GetComponent<RoundManager>();

        private void OnEnable()
        {
            round.RoundStarted += OnRoundStarted;
            round.PhaseChanged += OnPhaseChanged;
            round.RemainingTimeChanged += OnRemainingTimeChanged;
            round.QuestTimeReached += OnQuestTime;
            round.AnimalSpawnReached += OnAnimalSpawn;
            round.RoundEnded += OnRoundEnded;
            round.MatchProgressed += OnMatchProgressed;
        }

        private void OnDisable()
        {
            round.RoundStarted -= OnRoundStarted;
            round.PhaseChanged -= OnPhaseChanged;
            round.RemainingTimeChanged -= OnRemainingTimeChanged;
            round.QuestTimeReached -= OnQuestTime;
            round.AnimalSpawnReached -= OnAnimalSpawn;
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
            if (phase == RoundPhase.SuddenDeath) BossManager.Instance.DespawnCurrentBoss();
            if (phase == RoundPhase.Store) StartCoroutine(StoreTimer());
        }

        private void OnRemainingTimeChanged(float time)
        {
            if (round.Phase == RoundPhase.Countdown) UIManager.Instance.RoundWait(time);
            else if (round.IsRoundActive) UIManager.Instance.RoundTime(time);
        }

        private void OnQuestTime() => QuestManager.Instance.ActivateQuest(QuestType.StunChain);

        private void OnAnimalSpawn() => BossManager.Instance.SpawnRandomBoss();

        private void OnRoundEnded(RoundResult result)
        {
            QuestManager.Instance?.OnRoundEnd(result.P1Tiles, result.P2Tiles, result.Winner);
            QuestManager.Instance?.ResetAllQuest();         // 미완료 퀘스트 정리 -> 다음 라운드에 새로 제시

            PlayerType loser = result.Winner == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1;
            CoinManager.Instance.AddCoins(result.Winner, 10);
            CoinManager.Instance.AddCoins(loser, 5);

            AchievementManager.Instance.AddValue(result.Winner, AchievementType.TotalWins, 1);
        }

        private void OnMatchProgressed(MatchProgress progress)
        {
            UIManager.Instance.SetRoundWins(round.Match.P1Wins, round.Match.P2Wins);
        }

        private IEnumerator StoreTimer()
        {
            StoreUI storeUI = null;
            while ((storeUI = FindAnyObjectByType<StoreUI>()) == null) yield return null;

            for (float t = storeTime; t > 0f; t -= Time.deltaTime)
            {
                storeUI.ShopingTime(t);
                yield return null;
            }
            round.FinishStore();
        }
    }
}
