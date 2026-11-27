using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animal
{
    // 황소: 어슬렁거리다가 상하좌우 일직선 위에 플레이어가 보이면(장애물에 안 가리고 sightRange 안) 그쪽으로
    //       1초 경고선을 보여준 뒤 장애물·맵 끝까지 일직선 돌진. 한참(maxWanderTime) 못 보면
    //       플레이어에게 가장 가까이 지나가는 방향(6칸 이상)으로 돌진.
    //       경로 위 플레이어는 1초 기절(한 돌진에 한 명당 1번). 3회 돌진하면 퇴장.
    //       어슬렁거리는 중이나 경고 중에 맞으면 때린 방향(때린 사람 → 황소)으로 짧은 경고 후 돌진
    // 프리팹: 루트에 이 스크립트 + 트리거 콜라이더(Animal 레이어), 모델은 첫 번째 자식
    // 온라인: 생각·판정은 방장. 상대 PC는 경고선·돌진 신호로 연출만 재생 (이동은 OnlineAnimal이 전달)
    public class Bull : AnimalBase
    {
        [Header("돌진")]
        [SerializeField, Min(1)] private int chargeCount = 3;
        [SerializeField] private float walkSpeed = 2f;              // 어슬렁거릴 때 속도 (칸/초)
        [SerializeField] private float minWanderTime = 0.5f;        // 돌진 직후 최소 이만큼은 걸은 뒤에 다시 돌진
        [SerializeField] private float maxWanderTime = 6f;          // 이만큼 걸어도 아무도 안 보이면 가장 가까운 쪽으로 돌진
        [SerializeField] private int sightRange = 10;               // 이 칸 수 안의 일직선 위 플레이어를 봄
        [SerializeField] private float laneWidth = 0.6f;            // 일직선에서 옆으로 이만큼까지 벗어나도 "보임"
        [SerializeField] private bool aimAtLeader = false;          // 켜면 점수 높은 플레이어만 노림
        [SerializeField, Min(1)] private int minChargeCells = 6;    // 못 봤을 때 고르는 방향은 이 칸 수 이상 달릴 수 있는 쪽만 (없으면 가장 긴 쪽)
        [SerializeField] private float warnTime = 1f;               // 경고선 표시 시간
        [SerializeField] private float hitWarnTime = 0.4f;          // 맞아서 돌진할 때 경고 시간
        [SerializeField] private bool hitChargeCounts = true;       // 맞아서 한 돌진도 3회에 포함
        [SerializeField] private float chargeSpeed = 12f;           // 칸/초 (플레이어 5)
        [SerializeField] private float stunDuration = 1f;
        [SerializeField] private float hitRadius = 0.7f;            // 황소 중심에서 이 거리 안의 플레이어를 들이받음

        [Header("경고선")]
        [SerializeField] private Material warningMaterial;          // 비우면 코드로 만듦
        [SerializeField] private Color warningColor = new Color(1f, 0.15f, 0.15f, 0.55f);
        [SerializeField] private float tileTop = 0.5f;              // 바닥 블럭 윗면 높이
        [SerializeField] private float blinkInterval = 0.12f;

        [Header("연출")]
        [SerializeField] private float pawShake = 0.04f;            // 경고 중 발 구르며 떨림
        [SerializeField] private float runBob = 0.12f;              // 달릴 때 들썩임
        [SerializeField] private Color dustColor = new Color(0.85f, 0.75f, 0.6f, 0.8f);

        private static readonly Vector2Int[] Directions =
            { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        private const byte EventWarn = 1, EventCharge = 2;          // 온라인 상대 PC: 경고선 / 돌진 연출

        private enum Phase { Resting, Warning, Charging }

        private Phase phase = Phase.Resting;
        private PlayerMovement hitBy;                               // 맞으면 기록, 루틴에서 처리
        private Vector2Int? spotted;                                // 걷다가 플레이어를 본 방향
        private Transform model;
        private Vector3 modelPos, modelScale;
        private float baseY;
        private Transform fxRoot;
        private Material runtimeMaterial;
        private readonly List<GameObject> warningTiles = new();
        private Coroutine remoteWarn;

        protected override void OnSpawn()
        {
            Setup();
            baseY = transform.position.y;
            transform.position = Grid.CellToWorld(CurrentCell, baseY);
            StartCoroutine(ChargeLoop());
        }

        // 온라인 상대 PC: 모델·경고선 준비만 (이동은 OnlineAnimal이 전달)
        protected override void OnRemoteSpawn() => Setup();

        protected override void OnRemoteEvent(byte id, int a, int b)
        {
            if (id == EventWarn)
            {
                // a = 시작 칸 x | 시작 칸 z << 8 | 방향 << 16,  b = 칸 수 | 맞아서인지 << 8
                var start = new Vector2Int(a & 0xFF, (a >> 8) & 0xFF);
                Vector2Int dir = Directions[(a >> 16) & 0x3];
                var path = new List<Vector2Int>();
                for (int i = 1; i <= (b & 0xFF); i++) path.Add(start + dir * i);

                if (remoteWarn != null) StopCoroutine(remoteWarn);  // 경고 중에 맞아서 방향이 바뀌면 다시
                remoteWarn = StartCoroutine(Warn(path, (b >> 8) != 0 ? hitWarnTime : warnTime, interruptible: false));
            }
            else if (id == EventCharge)
            {
                if (remoteWarn != null) StopCoroutine(remoteWarn);
                remoteWarn = null;
                ResetModel();
                StartCoroutine(ChargeFx(a));
            }
        }

        // 모델 기준값, 콜라이더 확인, 연출 부모·머티리얼
        private void Setup()
        {
            model = transform.childCount > 0 ? transform.GetChild(0) : null;
            if (model != null)
            {
                modelPos = model.localPosition;
                modelScale = model.localScale;
            }
            if (GetComponentInChildren<Collider>() == null)
                Debug.LogWarning("Bull: 콜라이더가 없어서 맞지 않습니다. 루트에 Box Collider(Is Trigger)를 붙이고 레이어를 Animal로.", this);

            fxRoot = new GameObject("BullFx").transform;
            if (warningMaterial == null) warningMaterial = runtimeMaterial = AnimalFx.CreateTransparentMaterial();
        }

        protected override void OnDespawn()
        {
            ClearWarning();
            if (fxRoot != null) Destroy(fxRoot.gameObject);
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
        }

        protected override void OnHit(PlayerMovement attacker)
        {
            if (attacker != null && phase != Phase.Charging) hitBy = attacker;   // 돌진 중에는 무시
        }

        private IEnumerator ChargeLoop()
        {
            int done = 0;
            while (done < chargeCount)
            {
                // 어슬렁거리기: 플레이어를 보거나, 맞거나, 너무 오래 걸으면 끝
                phase = Phase.Resting;
                spotted = null;
                yield return Wander();

                // 방향 정하기 + 경고. 경고 중에 맞으면 맞은 방향으로 다시 경고
                Vector2Int dir;
                List<Vector2Int> path;
                bool byHit;
                while (true)
                {
                    byHit = hitBy != null;
                    dir = byHit ? HitDirection(hitBy) : spotted ?? DirectionToPlayer();
                    hitBy = null;
                    spotted = null;
                    path = BuildPath(dir);
                    if (path.Count == 0) dir = BestOpenDirection(out path);   // 그 방향이 바로 막혀 있으면 가장 길게 갈 수 있는 쪽

                    phase = Phase.Warning;
                    transform.rotation = Quaternion.LookRotation(AnimalFx.ToWorldDir(dir));
                    Vector2Int from = CurrentCell;
                    SendEvent(EventWarn, from.x | (from.y << 8) | (System.Array.IndexOf(Directions, dir) << 16),
                              path.Count | ((byHit ? 1 : 0) << 8));        // 상대 PC: 같은 칸에 경고선
                    yield return Warn(path, byHit ? hitWarnTime : warnTime, interruptible: !byHit);
                    if (byHit) { hitBy = null; break; }             // 맞아서 하는 짧은 경고 중에 또 맞은 건 무시
                    if (hitBy == null) break;                       // 보통 경고 중에 맞으면 맞은 방향으로 다시
                }

                if (path.Count > 0)
                {
                    phase = Phase.Charging;
                    SendEvent(EventCharge, path.Count);             // 상대 PC: 돌진 연출
                    yield return Charge(path);
                }

                if (!byHit || hitChargeCounts) done++;
            }
            Finish();
        }

        // ───────── 어슬렁거리기 ─────────

        private IEnumerator Wander()
        {
            Vector2Int facing = AnimalFx.SnapToCellDir(transform.forward);
            for (float elapsed = 0f; elapsed < maxWanderTime && hitBy == null;)
            {
                // 칸 중앙에 설 때마다 사방을 둘러봄
                if (elapsed >= minWanderTime && TrySpotPlayer(out Vector2Int seen))
                {
                    spotted = seen;
                    break;
                }

                Vector2Int cell = CurrentCell;
                var options = new List<Vector2Int>();
                foreach (Vector2Int d in Directions)
                    if (d != -facing && Grid.IsWalkable(cell + d)) options.Add(d);
                if (Grid.IsWalkable(cell + facing) && Random.value < 0.5f) options.Add(facing);   // 직진 가중치
                if (options.Count == 0 && Grid.IsWalkable(cell - facing)) options.Add(-facing);

                if (options.Count == 0)                             // 갇혀 있으면 제자리에서 기다림
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                    continue;
                }

                facing = options[Random.Range(0, options.Count)];
                transform.rotation = Quaternion.LookRotation(AnimalFx.ToWorldDir(facing));

                Vector3 start = transform.position;
                Vector3 end = Grid.CellToWorld(cell + facing, baseY);
                float stepTime = 1f / walkSpeed;
                for (float t = 0f; t < stepTime; t += Time.deltaTime)
                {
                    transform.position = Vector3.Lerp(start, end, t / stepTime);
                    if (model != null)                              // 느릿느릿 들썩
                        model.localPosition = modelPos + Vector3.up * Mathf.Abs(Mathf.Sin(t / stepTime * Mathf.PI * 2f)) * runBob * 0.5f;
                    yield return null;
                }
                transform.position = end;
                elapsed += stepTime;
            }
            ResetModel();
        }

        // ───────── 방향과 경로 ─────────

        // 상하좌우 일직선 위, 장애물 앞까지(최대 sightRange칸) 안에 플레이어가 있으면 가장 가까운 쪽
        private bool TrySpotPlayer(out Vector2Int dir)
        {
            dir = Vector2Int.zero;
            float nearest = float.MaxValue;
            List<PlayerMovement> targets = GetTargets();
            Vector2 from = ToFlat(Grid.CellToWorld(CurrentCell));

            foreach (Vector2Int d in Directions)
            {
                float reach = Mathf.Min(BuildPath(d).Count, sightRange) + 0.5f;   // 마지막 칸 끝까지
                if (reach < 1f) continue;
                Vector2 forward = new Vector2(d.x, d.y);

                foreach (PlayerMovement p in targets)
                {
                    Vector2 rel = ToFlat(p.transform.position) - from;
                    float along = Vector2.Dot(rel, forward);
                    float side = Mathf.Abs(rel.x * forward.y - rel.y * forward.x);
                    if (along <= 0.3f || along > reach || side > laneWidth) continue;
                    if (along < nearest) { nearest = along; dir = d; }
                }
            }
            return nearest < float.MaxValue;
        }

        // 노릴 수 있는 플레이어: 기절·무적이면 맞힐 수 없으니 제외 (둘 다 그렇면 전부). aimAtLeader면 1등만
        private List<PlayerMovement> GetTargets()
        {
            var targets = new List<PlayerMovement>();
            if (aimAtLeader)
            {
                PlayerMovement leader = GetLeader();
                if (leader != null) targets.Add(leader);
                return targets;
            }
            foreach (PlayerMovement p in Players)
                if (p != null && !p.IsInvincible && p.state != PlayerState.Stunned) targets.Add(p);
            if (targets.Count == 0)
                foreach (PlayerMovement p in Players) if (p != null) targets.Add(p);
            return targets;
        }

        // minChargeCells 이상 달릴 수 있는 방향 중, 돌진 경로가 플레이어에게 가장 가까이 지나가는 쪽.
        // 기절·무적인 플레이어는 맞힐 수 없으니 제외 (둘 다 그렇면 전부 대상). 그런 방향이 없으면 가장 길게 달릴 수 있는 쪽
        private Vector2Int DirectionToPlayer()
        {
            List<PlayerMovement> targets = GetTargets();
            Vector2Int best = Vector2Int.zero;
            float bestDist = float.MaxValue;
            foreach (Vector2Int d in Directions)
            {
                List<Vector2Int> path = BuildPath(d);
                if (path.Count < minChargeCells) continue;

                float dist = targets.Count == 0 ? Random.value : float.MaxValue;   // 대상이 없으면 랜덤
                foreach (PlayerMovement p in targets) dist = Mathf.Min(dist, DistanceToPath(p.transform.position, path));
                if (dist < bestDist) { bestDist = dist; best = d; }
            }
            return bestDist < float.MaxValue ? best : BestOpenDirection(out _);
        }

        // 플레이어와 돌진 경로(현재 칸 ~ 마지막 칸 선분) 사이의 바닥 거리
        private float DistanceToPath(Vector3 point, List<Vector2Int> path)
        {
            Vector2 a = ToFlat(Grid.CellToWorld(CurrentCell));
            Vector2 b = ToFlat(Grid.CellToWorld(path[path.Count - 1]));
            Vector2 p = ToFlat(point);
            Vector2 ab = b - a;
            float k = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * k);
        }

        private static Vector2 ToFlat(Vector3 v) => new Vector2(v.x, v.z);

        private Vector2Int HitDirection(PlayerMovement attacker) =>
            AnimalFx.SnapToCellDir(transform.position - attacker.transform.position);

        // 지금 칸에서 dir 방향으로 걸을 수 있는 칸들 (장애물·맵 끝 앞까지)
        private List<Vector2Int> BuildPath(Vector2Int dir)
        {
            var path = new List<Vector2Int>();
            Vector2Int c = CurrentCell + dir;
            int limit = Grid.gridSizeX + Grid.gridSizeZ;
            while (Grid.IsWalkable(c) && path.Count < limit)
            {
                path.Add(c);
                c += dir;
            }
            return path;
        }

        private Vector2Int BestOpenDirection(out List<Vector2Int> best)
        {
            Vector2Int bestDir = Directions[0];
            best = new List<Vector2Int>();
            foreach (Vector2Int d in Directions)
            {
                List<Vector2Int> p = BuildPath(d);
                if (p.Count > best.Count) { best = p; bestDir = d; }
            }
            return bestDir;
        }

        // ───────── 경고 (방장·상대 공통) ─────────

        private IEnumerator Warn(List<Vector2Int> path, float time, bool interruptible)
        {
            ClearWarning();
            foreach (Vector2Int c in path)
            {
                GameObject tile = AnimalFx.CreateCube("Warning", fxRoot, warningMaterial, warningColor);
                tile.transform.position = Grid.CellToWorld(c, tileTop + 0.02f);
                tile.transform.localScale = new Vector3(0.9f, 0.03f, 0.9f);
                warningTiles.Add(tile);
            }

            float nextBlink = blinkInterval;
            bool visible = true;
            for (float t = 0f; t < time && !(interruptible && hitBy != null); t += Time.deltaTime)
            {
                if (t >= nextBlink)                                 // 깜빡임
                {
                    nextBlink += blinkInterval;
                    visible = !visible;
                    foreach (GameObject w in warningTiles) if (w != null) w.SetActive(visible);
                }
                if (model != null)                                  // 발 구르며 몸을 낮추고 부르르
                {
                    float k = t / time;
                    model.localPosition = modelPos + new Vector3(Random.Range(-pawShake, pawShake), 0f, Random.Range(-pawShake, pawShake));
                    model.localScale = Vector3.Scale(modelScale, new Vector3(1f + 0.1f * k, 1f - 0.12f * k, 1f));
                }
                yield return null;
            }
            ResetModel();
        }

        private void ClearWarning()
        {
            foreach (GameObject w in warningTiles) if (w != null) Destroy(w);
            warningTiles.Clear();
        }

        // ───────── 돌진 ─────────

        // 방장(로컬): 실제 돌진 (이동 + 들이받기 + 연출)
        private IEnumerator Charge(List<Vector2Int> path)
        {
            var stunned = new HashSet<PlayerMovement>();
            Vector3 start = transform.position;
            Vector3 end = Grid.CellToWorld(path[path.Count - 1], baseY);
            float duration = path.Count / chargeSpeed;
            float nextDust = 0f;

            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                transform.position = Vector3.Lerp(start, end, k);
                ChargePose(t);
                HidePassedWarnings(k, path.Count);
                RamPlayers(stunned);

                if (t >= nextDust)
                {
                    nextDust += 0.05f;
                    StartCoroutine(DustRoutine(transform.position));
                }
                yield return null;
            }
            transform.position = end;
            RamPlayers(stunned);
            ClearWarning();
            yield return StopSquash();
        }

        // 상대 PC: 돌진 연출만 (이동은 방장 위치가 전달됨)
        private IEnumerator ChargeFx(int count)
        {
            float duration = count / chargeSpeed;
            float nextDust = 0f;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                ChargePose(t);
                HidePassedWarnings(t / duration, count);
                if (t >= nextDust)
                {
                    nextDust += 0.05f;
                    StartCoroutine(DustRoutine(transform.position));
                }
                yield return null;
            }
            ClearWarning();
            yield return StopSquash();
        }

        // 달릴 때 들썩 + 앞으로 길쭉 (공통)
        private void ChargePose(float t)
        {
            if (model == null) return;
            model.localPosition = modelPos + Vector3.up * Mathf.Abs(Mathf.Sin(t * 20f)) * runBob;
            model.localScale = Vector3.Scale(modelScale, new Vector3(0.95f, 0.95f, 1.1f));
        }

        // 지나간 칸의 경고선 지우기 (공통)
        private void HidePassedWarnings(float k, int count)
        {
            int passed = Mathf.FloorToInt(k * count);
            for (int i = 0; i < passed && i < warningTiles.Count; i++)
                if (warningTiles[i] != null) warningTiles[i].SetActive(false);
        }

        // 멈추며 찌그러짐 (공통)
        private IEnumerator StopSquash()
        {
            for (float t = 0f; t < 0.15f; t += Time.deltaTime)
            {
                if (model != null)
                    model.localScale = Vector3.Scale(modelScale, Vector3.Lerp(new Vector3(1.1f, 0.85f, 0.9f), Vector3.one, t / 0.15f));
                yield return null;
            }
            ResetModel();
        }

        private void RamPlayers(HashSet<PlayerMovement> stunned)
        {
            foreach (PlayerMovement p in Players)
            {
                if (p == null || stunned.Contains(p)) continue;
                Vector3 d = p.transform.position - transform.position;
                d.y = 0f;
                if (d.magnitude > hitRadius) continue;

                stunned.Add(p);
                StunPlayer(p, stunDuration);                        // 이미 기절 중이거나 무적이면 PlayerMovement가 무시함
            }
        }

        private IEnumerator DustRoutine(Vector3 at)
        {
            GameObject dust = AnimalFx.CreateCube("Dust", fxRoot, warningMaterial, dustColor);
            Vector3 pos = new Vector3(at.x, tileTop + 0.1f, at.z) + new Vector3(Random.Range(-0.3f, 0.3f), 0f, Random.Range(-0.3f, 0.3f));
            const float life = 0.4f;
            for (float t = 0f; t < life; t += Time.deltaTime)
            {
                float k = t / life;
                dust.transform.position = pos + Vector3.up * (0.4f * k);
                dust.transform.localScale = Vector3.one * Mathf.Lerp(0.25f, 0.05f, k);
                yield return null;
            }
            Destroy(dust);
        }

        private void ResetModel()
        {
            if (model == null) return;
            model.localPosition = modelPos;
            model.localScale = modelScale;
        }
    }
}
