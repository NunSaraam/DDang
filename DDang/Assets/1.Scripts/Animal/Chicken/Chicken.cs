using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animal
{
    // 닭: 평소엔 바닥을 종종거리며 돌아다님. 맞으면 때린 반대쪽으로 2~3칸 날아가 착지하고,
    //     착지 칸 십자(중심 + 상하좌우)를 때린 사람 색으로 칠함. 날아가는 중에는 다시 맞지 않음
    // 프리팹: 루트에 이 스크립트 + 트리거 콜라이더(Animal 레이어), 모델은 첫 번째 자식
    public class Chicken : AnimalBase
    {
        [Header("돌아다니기")]
        [SerializeField] private float cellsPerSecond = 2.5f;
        [SerializeField, Range(0f, 1f)] private float straightChance = 0.6f;   // 직진을 유지할 확률
        [SerializeField, Range(0f, 1f)] private float peckChance = 0.25f;      // 한 칸 갈 때마다 멈춰서 쪼을 확률
        [SerializeField] private float peckTime = 0.8f;
        [SerializeField] private float stepHop = 0.08f;                         // 걸을 때 통통 튀는 높이

        [Header("맞았을 때")]
        [SerializeField, Min(1)] private int minFly = 2;
        [SerializeField, Min(1)] private int maxFly = 3;
        [SerializeField] private float flyTime = 0.55f;
        [SerializeField] private float flyHeight = 1.6f;
        [SerializeField] private float dazeTime = 0.4f;                         // 착지 후 어지러워하는 시간 (이동 안 함)

        [Header("깃털 연출")]
        [SerializeField] private Material fxMaterial;                           // 비우면 코드로 만듦
        [SerializeField] private Color featherColor = Color.white;
        [SerializeField, Min(0)] private int featherCount = 6;

        private static readonly Vector2Int[] Directions =
            { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        private Transform model;
        private Vector3 modelPos, modelScale;
        private Quaternion modelRot;
        private float baseY;
        private Vector2Int direction;
        private bool flying;
        private Coroutine wanderRoutine;
        private Transform fxRoot;
        private Material runtimeMaterial;

        protected override void OnSpawn()
        {
            model = transform.childCount > 0 ? transform.GetChild(0) : null;
            if (model != null)
            {
                modelPos = model.localPosition;
                modelScale = model.localScale;
                modelRot = model.localRotation;
            }
            if (GetComponentInChildren<Collider>() == null)
                Debug.LogWarning("Chicken: 콜라이더가 없어서 맞지 않습니다. 루트에 Box Collider(Is Trigger)를 붙이고 레이어를 Animal로.", this);

            fxRoot = new GameObject("ChickenFx").transform;
            if (fxMaterial == null) fxMaterial = runtimeMaterial = AnimalFx.CreateTransparentMaterial();

            baseY = transform.position.y;
            transform.position = Grid.CellToWorld(CurrentCell, baseY);
            direction = Directions[Random.Range(0, Directions.Length)];
            wanderRoutine = StartCoroutine(WanderRoutine());
        }

        protected override void OnDespawn()
        {
            if (fxRoot != null) Destroy(fxRoot.gameObject);
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
        }

        protected override void OnHit(PlayerMovement attacker)
        {
            if (flying) return;
            if (wanderRoutine != null) StopCoroutine(wanderRoutine);
            StartCoroutine(FlyRoutine(attacker));
        }

        // ───────── 돌아다니기 ─────────

        private IEnumerator WanderRoutine()
        {
            Vector2Int cell = CurrentCell;
            while (IsActive)
            {
                if (Random.value < peckChance) yield return Peck();

                if (!TryPickNext(cell, out Vector2Int next))
                {
                    yield return new WaitForSeconds(0.3f);
                    cell = CurrentCell;
                    continue;
                }

                direction = next - cell;
                transform.rotation = Quaternion.LookRotation(AnimalFx.ToWorldDir(direction));
                yield return Step(next);
                cell = next;
            }
        }

        private bool TryPickNext(Vector2Int cell, out Vector2Int next)
        {
            if (Random.value < straightChance && Grid.IsWalkable(cell + direction))
            {
                next = cell + direction;
                return true;
            }

            var options = new List<Vector2Int>();
            foreach (Vector2Int d in Directions)
                if (d != -direction && Grid.IsWalkable(cell + d)) options.Add(cell + d);
            if (options.Count == 0 && Grid.IsWalkable(cell - direction)) options.Add(cell - direction);   // 막다른 길이면 되돌아감

            next = options.Count > 0 ? options[Random.Range(0, options.Count)] : cell;
            return options.Count > 0;
        }

        private IEnumerator Step(Vector2Int cell)
        {
            Vector3 start = transform.position;
            Vector3 end = Grid.CellToWorld(cell, baseY);
            float duration = 1f / cellsPerSecond;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                transform.position = Vector3.Lerp(start, end, k);
                SetModel(Mathf.Abs(Mathf.Sin(k * Mathf.PI * 2f)) * stepHop, 0f, 0f);    // 한 칸에 두 번 통통
                yield return null;
            }
            transform.position = end;
            SetModel(0f, 0f, 0f);
        }

        // 고개를 숙였다 들었다 3번
        private IEnumerator Peck()
        {
            for (float t = 0f; t < peckTime; t += Time.deltaTime)
            {
                float k = Mathf.Abs(Mathf.Sin(t / peckTime * Mathf.PI * 3f));
                SetModel(0f, 35f * k, 0f);
                yield return null;
            }
            SetModel(0f, 0f, 0f);
        }

        // ───────── 맞았을 때 ─────────

        private IEnumerator FlyRoutine(PlayerMovement attacker)
        {
            flying = true;
            PlayerType color = attacker.playerType;

            Vector2Int dir = AnimalFx.SnapToCellDir(transform.position - attacker.transform.position);
            Vector2Int from = Grid.IsFloor(CurrentCell) ? CurrentCell : Grid.GetNearestFloor(CurrentCell);
            Vector2Int to = Grid.GetPushDestination(from, dir, Random.Range(minFly, maxFly + 1));

            transform.rotation = Quaternion.LookRotation(AnimalFx.ToWorldDir(dir));
            SpawnFeathers();

            Vector3 start = transform.position;
            Vector3 end = Grid.CellToWorld(to, baseY);
            for (float t = 0f; t < flyTime; t += Time.deltaTime)
            {
                float k = t / flyTime;
                transform.position = Vector3.Lerp(start, end, k) + Vector3.up * (flyHeight * 4f * k * (1f - k));
                SetModel(0f, 360f * k, Mathf.Sin(k * Mathf.PI * 8f) * 0.15f);       // 앞으로 한 바퀴 + 파닥파닥
                yield return null;
            }
            transform.position = end;
            SetModel(0f, 0f, 0.3f);                                                 // 착지하며 납작

            PaintCross(to, color);
            SpawnFeathers();

            // 어지러워서 좌우로 흔들
            for (float t = 0f; t < dazeTime; t += Time.deltaTime)
            {
                float k = t / dazeTime;
                SetModel(0f, 0f, 0.3f * (1f - k));
                if (model != null) model.localRotation *= Quaternion.Euler(0f, 0f, Mathf.Sin(k * Mathf.PI * 4f) * 12f);
                yield return null;
            }
            SetModel(0f, 0f, 0f);

            flying = false;
            wanderRoutine = StartCoroutine(WanderRoutine());
        }

        // height: 띄울 높이, pitch: 앞으로 숙이는 각도, squash: 양수면 납작
        private void SetModel(float height, float pitch, float squash)
        {
            if (model == null) return;
            model.localPosition = modelPos + Vector3.up * height;
            model.localRotation = modelRot * Quaternion.Euler(pitch, 0f, 0f);
            model.localScale = Vector3.Scale(modelScale, new Vector3(1f + squash, 1f - squash, 1f + squash));
        }

        private void SpawnFeathers()
        {
            for (int i = 0; i < featherCount; i++) StartCoroutine(FeatherRoutine());
        }

        private IEnumerator FeatherRoutine()
        {
            GameObject feather = AnimalFx.CreateCube("Feather", fxRoot, fxMaterial, featherColor);
            feather.transform.localScale = new Vector3(0.12f, 0.03f, 0.18f);
            Vector3 pos = transform.position + Vector3.up * 0.5f;
            Vector3 velocity = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(1.5f, 3f), Random.Range(-1.5f, 1.5f));
            Vector3 spin = Random.onUnitSphere * 540f;

            const float life = 0.8f;
            for (float t = 0f; t < life; t += Time.deltaTime)
            {
                velocity += Vector3.down * 4f * Time.deltaTime;                    // 천천히 떨어짐
                velocity *= 1f - 2f * Time.deltaTime;                               // 공기 저항
                pos += velocity * Time.deltaTime;
                feather.transform.position = pos;
                feather.transform.Rotate(spin * Time.deltaTime, Space.World);
                feather.transform.localScale *= 1f - 0.8f * Time.deltaTime;
                yield return null;
            }
            Destroy(feather);
        }
    }
}
