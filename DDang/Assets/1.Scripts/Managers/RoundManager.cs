using System;
using System.Collections;
using UnityEngine;

namespace Rules
{
    public class RoundManager : SingletonMonoBehaviour<RoundManager>
    {
        [SerializeField] private RoundRuleSettings rules;

        public RoundPhase Phase { get; private set; } = RoundPhase.None;
        public MatchScore Match { get; private set; }
        public float RemainingTime { get; private set; }
        public bool IsDuelRound => Match != null && Match.IsDuel;
        public RoundRuleSettings Rules => rules;

        public bool IsRoundActive => Phase is RoundPhase.Playing or RoundPhase.ExtraTime
                                           or RoundPhase.SuddenDeath or RoundPhase.GoldenGoal;

        public event Action<RoundPhase> PhaseChanged;
        public event Action<float> RemainingTimeChanged;
        public event Action<int> RoundStarted;              // 라운드 번호
        public event Action QuestTimeReached;               // 시작 후 10초
        public event Action AnimalWarningReached;           // 27초
        public event Action AnimalSpawnReached;             // 30초
        public event Action<RoundResult> RoundEnded;
        public event Action<MatchProgress> MatchProgressed;

        private IRoundScoreSource scoreSource;
        private PlayerType roundWinner;
        private PlayerType suddenDeathWinner;

        public void RegisterScoreSource(IRoundScoreSource source) => scoreSource = source;

        public void UnregisterScoreSource(IRoundScoreSource source)
        {
            if (scoreSource == source) scoreSource = null;
        }

        public int GetTiles(PlayerType player) => scoreSource?.CountTiles(player) ?? 0;
        public int PaintableCount => scoreSource?.PaintableCount ?? 0;

        public void StartMatch()
        {
            Match = new MatchScore(rules.winsToWin);
            SetPhase(RoundPhase.None);
        }

        public void BeginRound()                            // 게임 씬 준비 완료 후 호출
        {
            StopAllCoroutines();
            StartCoroutine(RoundRoutine());
        }

        public void ReportPlayerHit(PlayerType attacker)   // 공격 적중 시 호출 (서든데스 판정)
        {
            if (Phase == RoundPhase.SuddenDeath && suddenDeathWinner == PlayerType.None)
                suddenDeathWinner = attacker;
        }

        public void FinishStore()                           // 상점 종료 시 호출
        {
            if (Phase != RoundPhase.Store) return;
            SetPhase(RoundPhase.None);
            SceneLoadManager.Instance.LoadScene("GameScene");
        }

        private IEnumerator RoundRoutine()
        {
            yield return new WaitUntil(() => scoreSource != null);

            RoundStarted?.Invoke(Match.RoundNumber);
            yield return RunTimer(RoundPhase.Countdown, rules.countdownTime);

            if (IsDuelRound) yield return PlayDuelRound();
            else yield return PlayNormalRound();

            yield return EndRound();
        }

        private IEnumerator PlayNormalRound()
        {
            bool questSent = false, warned = false, spawned = false;

            SetPhase(RoundPhase.Playing);
            for (float elapsed = 0f; elapsed < rules.roundTime; elapsed += Time.deltaTime)
            {
                if (!questSent && elapsed >= rules.questTime) { questSent = true; QuestTimeReached?.Invoke(); }
                if (!warned && elapsed >= rules.animalWarningTime) { warned = true; AnimalWarningReached?.Invoke(); }
                if (!spawned && elapsed >= rules.animalSpawnTime) { spawned = true; AnimalSpawnReached?.Invoke(); }

                SetRemainingTime(rules.roundTime - elapsed);
                yield return null;
            }

            if (!IsWithinExtraTimeMargin())
            {
                roundWinner = Leader();
                yield break;
            }

            yield return RunTimer(RoundPhase.ExtraTime, rules.extraTime);
            roundWinner = Leader();
            if (roundWinner != PlayerType.None) yield break;

            suddenDeathWinner = PlayerType.None;
            SetPhase(RoundPhase.SuddenDeath);
            yield return new WaitUntil(() => suddenDeathWinner != PlayerType.None);
            roundWinner = suddenDeathWinner;
        }

        private IEnumerator PlayDuelRound()
        {
            yield return RunTimer(RoundPhase.Playing, rules.duelTime);

            SetPhase(RoundPhase.GoldenGoal);
            yield return new WaitUntil(() => Leader() != PlayerType.None);
            roundWinner = Leader();
        }

        private IEnumerator EndRound()
        {
            var result = new RoundResult(roundWinner,
                scoreSource.CountTiles(PlayerType.Player1),
                scoreSource.CountTiles(PlayerType.Player2),
                IsDuelRound);

            SetPhase(RoundPhase.RoundEnd);
            RoundEnded?.Invoke(result);

            MatchProgress progress = Match.RecordRound(roundWinner);
            MatchProgressed?.Invoke(progress);

            yield return new WaitForSeconds(rules.resultShowTime);

            switch (progress)
            {
                case MatchProgress.GameOver:
                    SetPhase(RoundPhase.GameOver);
                    SceneLoadManager.Instance.LoadScene("GameResult");
                    break;
                case MatchProgress.Duel:                    // 기본 능력치로 싸우므로 상점 생략
                    SetPhase(RoundPhase.None);
                    SceneLoadManager.Instance.LoadScene("GameScene");
                    break;
                default:
                    SetPhase(RoundPhase.Store);
                    SceneLoadManager.Instance.LoadScene("Store");
                    break;
            }
        }

        private IEnumerator RunTimer(RoundPhase phase, float duration)
        {
            SetPhase(phase);
            for (float t = duration; t > 0f; t -= Time.deltaTime)
            {
                SetRemainingTime(t);
                yield return null;
            }
            SetRemainingTime(0f);
        }

        private bool IsWithinExtraTimeMargin()
        {
            int margin = Mathf.RoundToInt(scoreSource.PaintableCount * rules.extraTimeMarginRatio);
            int diff = Mathf.Abs(scoreSource.CountTiles(PlayerType.Player1) - scoreSource.CountTiles(PlayerType.Player2));
            return diff <= margin;
        }

        private PlayerType Leader()
        {
            int p1 = scoreSource.CountTiles(PlayerType.Player1);
            int p2 = scoreSource.CountTiles(PlayerType.Player2);
            if (p1 == p2) return PlayerType.None;
            return p1 > p2 ? PlayerType.Player1 : PlayerType.Player2;
        }

        private void SetPhase(RoundPhase phase)
        {
            Phase = phase;
            PhaseChanged?.Invoke(phase);
        }

        private void SetRemainingTime(float time)
        {
            RemainingTime = time;
            RemainingTimeChanged?.Invoke(time);
        }
    }
}
