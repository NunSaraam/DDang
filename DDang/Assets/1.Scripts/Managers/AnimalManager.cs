using System;
using System.Collections.Generic;
using Rules;
using UnityEngine;
using UnityEngine.Serialization;

namespace Animal
{
    public class AnimalManager : SingletonMonoBehaviour<AnimalManager>
    {
        [SerializeField] private List<AnimalData> roster = new();

        [Header("등장 위치 (RandomFloor)")]
        [SerializeField] private int spawnEdgeMargin = 2;          // 가장자리에서 최소 칸 수
        [SerializeField] private int spawnPlayerDistance = 3;      // 플레이어와 최소 칸 수

        [Header("테스트")]
        [Tooltip("지정하면 매 라운드 이 동물만 등장 (중복 규칙 무시)")]
        [SerializeField, FormerlySerializedAs("forceBoss")] private AnimalData forceAnimal;

        public AnimalBase Current { get; private set; }
        public AnimalData Pending { get; private set; }               // 경고 중인 동물

        public event Action<AnimalData> Warned;                       // 27초: 경고 UI
        public event Action<AnimalBase> Spawned;
        public event Action Despawned;

        private readonly List<AnimalData> usedThisMatch = new();
        private MatchScore poolMatch;
        private RoundManager round;

        private void Start()
        {
            if (Instance != this) return;                           // 씬 재로드로 생긴 중복 개체
            round = RoundManager.Instance;
            if (round == null) return;

            round.AnimalWarningReached += OnWarning;
            round.AnimalSpawnReached += OnSpawnTime;
            round.PhaseChanged += OnPhaseChanged;
        }

        protected override void OnDestroy()
        {
            if (round != null)
            {
                round.AnimalWarningReached -= OnWarning;
                round.AnimalSpawnReached -= OnSpawnTime;
                round.PhaseChanged -= OnPhaseChanged;
            }
            base.OnDestroy();
        }

        private void OnWarning()
        {
            Pending = PickNext();
            if (Pending != null) Warned?.Invoke(Pending);
        }

        private void OnSpawnTime()
        {
            AnimalData data = Pending ?? PickNext();
            Pending = null;
            if (data != null) Spawn(data);
        }

        private void OnPhaseChanged(RoundPhase phase)
        {
            // 서든데스와 라운드 종료 시 퇴장
            if (phase is RoundPhase.SuddenDeath or RoundPhase.RoundEnd)
            {
                Pending = null;
                DespawnCurrent();
            }
        }

        // 한 판(매치) 안에서 중복 없이 뽑기
        private AnimalData PickNext()
        {
            if (forceAnimal != null) return forceAnimal;

            if (round.Match != poolMatch)                           // 새 매치 -> 사용 기록 초기화
            {
                poolMatch = round.Match;
                usedThisMatch.Clear();
            }

            var candidates = roster.FindAll(d => d != null && d.inRotation && !usedThisMatch.Contains(d));
            if (candidates.Count == 0) return null;

            AnimalData pick = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            usedThisMatch.Add(pick);
            return pick;
        }

        public void Spawn(AnimalData data)
        {
            DespawnCurrent();

            GridGenerator grid = FindAnyObjectByType<GridGenerator>();
            Vector2Int cell = data.spawnArea == AnimalSpawnArea.Center
                ? new Vector2Int(grid.gridSizeX / 2, grid.gridSizeZ / 2)
                : PickSpawnCell(grid);

            Current = Instantiate(data.prefab, grid.CellToWorld(cell, 0.5f), Quaternion.identity);   // 0.5 = 바닥 블록 윗면
            Current.Begin(data, grid);
            Spawned?.Invoke(Current);
        }

        // 가장자리·플레이어와 떨어진 바닥 칸. 없으면 아무 바닥 칸
        private Vector2Int PickSpawnCell(GridGenerator grid)
        {
            var players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
            var candidates = new List<Vector2Int>();

            foreach (Vector2Int c in grid.FloorCells)
            {
                if (!grid.IsWalkable(c)) continue;
                int edge = Mathf.Min(Mathf.Min(c.x, c.y), Mathf.Min(grid.gridSizeX - 1 - c.x, grid.gridSizeZ - 1 - c.y));
                if (edge < spawnEdgeMargin) continue;

                bool nearPlayer = false;
                foreach (PlayerMovement p in players)
                {
                    Vector2Int pc = grid.WorldToGridPosition(p.transform.position);
                    if (Mathf.Max(Mathf.Abs(pc.x - c.x), Mathf.Abs(pc.y - c.y)) < spawnPlayerDistance) { nearPlayer = true; break; }
                }
                if (!nearPlayer) candidates.Add(c);
            }

            return candidates.Count > 0 ? candidates[UnityEngine.Random.Range(0, candidates.Count)] : grid.RandomFloorCell();
        }

        public void DespawnCurrent()
        {
            if (Current != null) Current.Finish();                  // Finish -> NotifyFinished에서 정리
        }

        internal void NotifyFinished(AnimalBase animal)
        {
            if (Current != animal) return;
            Current = null;
            Despawned?.Invoke();
        }

        [ContextMenu("테스트: Force Animal 지금 등장")]
        private void SpawnForceAnimalNow()
        {
            if (forceAnimal != null) Spawn(forceAnimal);
        }
    }
}
