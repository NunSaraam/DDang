using System;
using System.Collections;
using UnityEngine;
using Online;

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

        public event Action ScoreChanged;                   // 칠한 칸 수가 바뀜 (등록된 맵의 이벤트를 그대로 알림)

        public void RegisterScoreSource(IRoundScoreSource source)
        {
            UnregisterScoreSource(scoreSource);
            scoreSource = source;
            if (source != null) source.ScoreChanged += RaiseScoreChanged;
            RaiseScoreChanged();                            // 새 맵 → 0:0부터
        }

        public void UnregisterScoreSource(IRoundScoreSource source)
        {
            if (source == null || scoreSource != source) return;
            source.ScoreChanged -= RaiseScoreChanged;
            scoreSource = null;
        }

        private void RaiseScoreChanged() => ScoreChanged?.Invoke();

        public int GetTiles(PlayerType player) => scoreSource?.CountTiles(player) ?? 0;
        public int PaintableCount => scoreSource?.PaintableCount ?? 0;
        public RoundResult? LastResult { get; private set; }       // 상점에서 패자 확인용

        public void StartMatch()
        {
            Match = new MatchScore(rules.winsToWin);
            LastResult = null;
            SetPhase(RoundPhase.None);
        }

        // 온라인에서 방장이 아닌 PC: 직접 진행하지 않고 OnlineRound가 받은 대로 따라감
        public bool IsFollower => OnlineSession.IsOnline && !OnlineSession.Instance.Runner.IsSharedModeMasterClient;

        public void BeginRound()                            // 게임 씬 준비 완료 후 호출
        {
            StopAllCoroutines();
            if (IsFollower) return;
            StartCoroutine(RoundRoutine());
        }

        public void StopMatch()                             // 온라인 대전 중 나가기: 진행 중인 라운드 정리
        {
            StopAllCoroutines();
            SetPhase(RoundPhase.None);
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
            LoadScene("GameScene");                         // 온라인: 방장이 바꾸면 상대도 따라옴
        }

        private IEnumerator RoundRoutine()
        {
            yield return new WaitUntil(() => scoreSource != null);
            if (OnlineSession.IsOnline)                     // 온라인: 상대 PC도 맵 준비가 끝날 때까지 대기
                yield return new WaitUntil(() => OnlineRound.Instance != null && OnlineRound.Instance.GuestReady);

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

            LastResult = result;
            SetPhase(RoundPhase.RoundEnd);
            RoundEnded?.Invoke(result);

            MatchProgress progress = Match.RecordRound(roundWinner);
            MatchProgressed?.Invoke(progress);

            yield return new WaitForSeconds(rules.resultShowTime);

            switch (progress)
            {
                case MatchProgress.GameOver:
                    SetPhase(RoundPhase.GameOver);
                    LoadScene("GameResult");
                    break;
                case MatchProgress.Duel:                    // 기본 능력치로 싸우므로 상점 생략
                    SetPhase(RoundPhase.None);
                    LoadScene("GameScene");
                    break;
                default:
                    SetPhase(RoundPhase.Store);
                    LoadScene("Store");
                    break;
            }
        }

        private static void LoadScene(string name)
        {
            if (OnlineSession.IsOnline) OnlineSession.Instance.LoadScene(name);   // 방장이 바꾸면 상대도 따라옴
            else SceneLoadManager.Instance.LoadScene(name);
        }

        // ── 온라인 상대 PC용: OnlineRound가 방장 상태를 받아 호출 ──
        public void FollowPhase(RoundPhase phase)
        {
            if (phase == RoundPhase.Countdown) RoundStarted?.Invoke(Match.RoundNumber);
            SetPhase(phase);
        }

        public void FollowTime(float time)
        {
            if (time != RemainingTime) SetRemainingTime(time);
        }

        public void FollowQuestTime() => QuestTimeReached?.Invoke();

        public void FollowRoundEnd(RoundResult result)
        {
            LastResult = result;
            SetPhase(RoundPhase.RoundEnd);
            RoundEnded?.Invoke(result);
            MatchProgressed?.Invoke(Match.RecordRound(result.Winner));    // 같은 승자 기록 → 두 PC의 매치 점수가 같음
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
