using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animal
{
    // 펭귄: 점프하며 얼음 조각을 쏘아 랜덤 바닥 칸 10~15개를 얼림. 얼린 칸은 현재 색으로 고정되어 덮어쓸 수 없고, 칸마다 6~12초 뒤 랜덤 해제
    public class Penguin : AnimalBase
    {
        [Header("얼리기")]
        [SerializeField, Min(1)] private int minFreeze = 10;
        [SerializeField, Min(1)] private int maxFreeze = 15;
        [SerializeField] private float minThaw = 6f;                // 얼린 뒤 녹을 때까지 최소 시간
        [SerializeField] private float maxThaw = 12f;               // 최대 시간
        [SerializeField, Min(1)] private int waves = 2;             // 얼리기 횟수 (1이면 등장할 때 한 번만)
        [SerializeField] private float waveInterval = 10f;          // 얼리기 사이 간격
        [SerializeField] private float freezeStagger = 0.05f;       // 얼음 조각을 하나씩 발사하는 간격 (연출용)

        [Header("점프 + 얼음 발사 연출")]
        [SerializeField] private float jumpHeight = 0.8f;           // 얼리기 직전 펭귄 점프 높이
        [SerializeField] private float jumpTime = 0.45f;            // 뛰어올라 착지할 때까지 시간
        [SerializeField, Range(0f, 0.5f)] private float squash = 0.2f;   // 웅크리기·착지 때 납작해지는 정도
        [SerializeField] private float launchHeight = 1.5f;         // 얼음 조각이 나가는 높이 (펭귄 부리 쯤)
        [SerializeField] private float shardSize = 0.3f;            // 날아가는 얼음 조각 크기
        [SerializeField] private float shardSpeed = 9f;             // 얼음 조각 속도 (칸/초)
        [SerializeField] private float shardArc = 1.5f;             // 포물선 높이

        [Header("얼음 표시")]
        [SerializeField] private Material iceMaterial;              // 비우면 코드로 반투명 머티리얼을 만듦
        [SerializeField] private Color iceColor = new Color(0.25f, 0.65f, 1f, 0.85f);    // 머티리얼 색 대신 이 색을 씀
        [SerializeField] private Color shineColor = new Color(1f, 1f, 1f, 0.95f);       // 얼음 위 반짝이 줄
        [SerializeField] private float tileTop = 0.5f;              // 바닥 블럭 윗면 높이
        [SerializeField] private float iceThickness = 0.08f;
        [SerializeField] private float iceSize = 0.96f;             // 칸 크기 대비 얼음 크기
        [SerializeField] private float popTime = 0.15f;             // 얼음이 생길 때 커지는 시간
        [SerializeField] private float blinkBefore = 1f;            // 녹기 전 이 시간 동안 깜빡임
        [SerializeField] private float blinkInterval = 0.12f;

        [Header("모델 뒤뚱거림")]
        [SerializeField] private Transform model;                   // 비우면 첫 번째 자식
        [SerializeField] private bool faceCamera = true;            // 등장할 때 카메라 쪽으로 얼굴을 돌림
        [SerializeField] private float waddleAngle = 8f;
        [SerializeField] private float waddleSpeed = 6f;

        [Header("등장 위치")]
        [SerializeField] private bool standAtFarEdge = true;        // 화면 위쪽(카메라에서 먼 쪽) 끝 가운데에 섬. 끄면 AnimalData의 Spawn Area대로
        [SerializeField, Min(0)] private int edgeInset = 1;         // 맵 끝에서 안쪽으로 몇 칸 들어올지

        private readonly Dictionary<Vector2Int, GameObject> frozen = new();
        private readonly HashSet<Vector2Int> incoming = new();      // 얼음 조각이 날아가는 중인 칸 (중복 선택 방지)
        private Transform iceRoot;
        private Material runtimeMaterial;
        private Quaternion modelBaseRotation;
        private Vector3 modelBasePosition;
        private Vector3 modelBaseScale;

        protected override void OnSpawn()
        {
            Vector3 toCamera = DirectionToCamera();
            if (standAtFarEdge) MoveToFarEdge(-toCamera);
            if (faceCamera) transform.rotation = Quaternion.LookRotation(toCamera);
            if (model == null && transform.childCount > 0) model = transform.GetChild(0);
            if (model != null)
            {
                modelBaseRotation = model.localRotation;
                modelBasePosition = model.localPosition;
                modelBaseScale = model.localScale;
            }

            iceRoot = new GameObject("PenguinIce").transform;
            if (iceMaterial == null)
            {
                runtimeMaterial = CreateIceMaterial();
                iceMaterial = runtimeMaterial;
            }

            StartCoroutine(FreezeRoutine());
        }

        protected override void OnDespawn()
        {
            // 남은 얼음 전부 해제 (안 하면 다음 라운드까지 얼어 있음)
            foreach (Vector2Int cell in new List<Vector2Int>(frozen.Keys)) Thaw(cell);
            if (iceRoot != null) Destroy(iceRoot.gameObject);
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
        }

        private void Update()
        {
            if (!IsActive || model == null) return;
            float angle = Mathf.Sin(Time.time * waddleSpeed) * waddleAngle;
            model.localRotation = modelBaseRotation * Quaternion.Euler(0f, 0f, angle);
        }

        private IEnumerator FreezeRoutine()
        {
            for (int w = 0; w < waves; w++)
            {
                List<Vector2Int> cells = PickCells(Random.Range(minFreeze, maxFreeze + 1));
                foreach (Vector2Int cell in cells) incoming.Add(cell);
                yield return JumpAndLaunch(cells);

                if (w < waves - 1) yield return new WaitForSeconds(waveInterval);
            }
        }

        // 웅크림 → 점프 (가장 높을 때 얼음 발사) → 착지 찌그러짐
        private IEnumerator JumpAndLaunch(List<Vector2Int> cells)
        {
            const float squashTime = 0.1f;
            for (float t = 0f; t < squashTime; t += Time.deltaTime)
            {
                SetPose(0f, squash * t / squashTime);
                yield return null;
            }

            bool launched = false;
            for (float t = 0f; t < jumpTime; t += Time.deltaTime)
            {
                float k = t / jumpTime;
                SetPose(jumpHeight * 4f * k * (1f - k), -squash * 0.5f * (1f - k));   // 올라갈 땐 살짝 길쭉하게
                if (!launched && k >= 0.5f)
                {
                    launched = true;
                    StartCoroutine(LaunchAll(cells));
                }
                yield return null;
            }
            if (!launched) StartCoroutine(LaunchAll(cells));

            for (float t = 0f; t < squashTime; t += Time.deltaTime)
            {
                SetPose(0f, squash * (1f - t / squashTime));
                yield return null;
            }
            SetPose(0f, 0f);
        }

        // height: 모델을 띄울 높이, s: 양수면 납작, 음수면 길쭉
        private void SetPose(float height, float s)
        {
            if (model == null) return;
            model.localPosition = modelBasePosition + Vector3.up * height;
            model.localScale = Vector3.Scale(modelBaseScale, new Vector3(1f + s, 1f - s, 1f + s));
        }

        private IEnumerator LaunchAll(List<Vector2Int> cells)
        {
            foreach (Vector2Int cell in cells)
            {
                StartCoroutine(ShardRoutine(cell));
                if (freezeStagger > 0f) yield return new WaitForSeconds(freezeStagger);
            }
        }

        // 펭귄 부리 쯤에서 얼음 조각이 포물선으로 날아가 칸에 떨어지면 그 칸이 얼어붙음
        private IEnumerator ShardRoutine(Vector2Int cell)
        {
            GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(shard.GetComponent<Collider>());
            shard.name = "IceShard";
            shard.transform.SetParent(iceRoot, false);
            shard.transform.localScale = Vector3.one * shardSize;
            Tint(shard.GetComponent<Renderer>(), iceColor);

            Vector3 start = transform.position + Vector3.up * launchHeight + transform.forward * 0.6f;
            if (model != null) start += model.localPosition - modelBasePosition;     // 점프 높이만큼 위에서 출발
            Vector3 end = Grid.CellToWorld(cell, tileTop + iceThickness * 0.5f);
            float distance = Vector2.Distance(new Vector2(start.x, start.z), new Vector2(end.x, end.z));
            float duration = Mathf.Clamp(distance / shardSpeed, 0.25f, 1.2f);
            float arc = shardArc + distance * 0.1f;
            Vector3 spin = Random.onUnitSphere * 720f;

            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float k = t / duration;
                shard.transform.position = Vector3.Lerp(start, end, k) + Vector3.up * (arc * 4f * k * (1f - k));
                shard.transform.Rotate(spin * Time.deltaTime, Space.World);
                yield return null;
            }

            Destroy(shard);
            incoming.Remove(cell);
            if (Grid.IsLocked(cell)) yield break;
            Freeze(cell);
            StartCoroutine(ThawRoutine(cell, Random.Range(minThaw, maxThaw)));
        }

        // 얼릴 수 있는 칸(바닥, 막히지 않음, 아직 안 얼었고 얼음 조각이 날아가는 중도 아님) 중 count개를 랜덤으로
        private List<Vector2Int> PickCells(int count)
        {
            var candidates = new List<Vector2Int>();
            foreach (Vector2Int c in Grid.FloorCells)
                if (Grid.IsWalkable(c) && !Grid.IsLocked(c) && !incoming.Contains(c)) candidates.Add(c);

            count = Mathf.Min(count, candidates.Count);
            for (int i = 0; i < count; i++)                         // 앞쪽 count개만 섞기
            {
                int j = Random.Range(i, candidates.Count);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }
            return candidates.GetRange(0, count);
        }

        private void Freeze(Vector2Int cell)
        {
            Grid.SetLocked(cell, true);                             // 현재 색 그대로 고정

            GameObject ice = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(ice.GetComponent<Collider>());         // 플레이어 이동·공격 판정에 걸리지 않게
            ice.name = $"Ice {cell}";
            ice.transform.SetParent(iceRoot, false);
            ice.transform.position = Grid.CellToWorld(cell, tileTop + iceThickness * 0.5f);
            Tint(ice.GetComponent<Renderer>(), iceColor);

            // 반짝이 줄 두 개 (얼음처럼 보이게)
            AddShine(ice.transform, new Vector3(-0.12f, 0f, 0.28f), new Vector3(0.5f, 0.1f));
            AddShine(ice.transform, new Vector3(0.3f, 0f, 0.28f), new Vector3(0.14f, 0.1f));

            frozen[cell] = ice;
            StartCoroutine(PopRoutine(ice.transform));
        }

        private void Thaw(Vector2Int cell)
        {
            if (!frozen.Remove(cell, out GameObject ice)) return;
            Grid.SetLocked(cell, false);
            if (ice != null) Destroy(ice);
        }

        private IEnumerator PopRoutine(Transform ice)
        {
            Vector3 full = new Vector3(iceSize, iceThickness, iceSize);
            for (float t = 0f; t < popTime; t += Time.deltaTime)
            {
                if (ice == null) yield break;
                float k = t / popTime;
                ice.localScale = Vector3.Scale(full, new Vector3(k, 1f, k));
                yield return null;
            }
            if (ice != null) ice.localScale = full;
        }

        private IEnumerator ThawRoutine(Vector2Int cell, float time)
        {
            float blink = Mathf.Min(blinkBefore, time);
            yield return new WaitForSeconds(time - blink);

            if (frozen.TryGetValue(cell, out GameObject ice) && ice != null)
            {
                Renderer[] rends = ice.GetComponentsInChildren<Renderer>();
                bool visible = true;
                for (float t = 0f; t < blink; t += blinkInterval)
                {
                    visible = !visible;
                    foreach (Renderer r in rends) r.enabled = visible;
                    yield return new WaitForSeconds(blinkInterval);
                }
            }
            Thaw(cell);
        }

        private void AddShine(Transform ice, Vector3 localPos, Vector2 size)
        {
            GameObject shine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(shine.GetComponent<Collider>());
            shine.name = "Shine";
            shine.transform.SetParent(ice, false);
            shine.transform.localPosition = new Vector3(localPos.x, 0.6f, localPos.z);   // 얼음 윗면 위로 살짝
            shine.transform.localScale = new Vector3(size.x, 0.3f, size.y);
            Tint(shine.GetComponent<Renderer>(), shineColor);
        }

        private void Tint(Renderer rend, Color color)
        {
            rend.sharedMaterial = iceMaterial;
            var block = new MaterialPropertyBlock();
            rend.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);                    // URP
            block.SetColor("_Color", color);                        // 기본 셰이더
            block.SetFloat("_Smoothness", 0.2f);                    // 하얗게 번쩍이는 반사 줄이기
            rend.SetPropertyBlock(block);
        }

        // 바닥 기준으로 카메라가 있는 쪽 방향. 두 화면 카메라 각도가 같으므로 하나만 보면 됨
        private static Vector3 DirectionToCamera()
        {
            Vector3 dir = Vector3.right;                            // 카메라 기본 각도 (30, -90, 0) 기준
            var cam = FindFirstObjectByType<Cameras.PlayerDualCamera>();   // 플레이어를 따라가는 화면 카메라
            if (cam != null)
            {
                Vector3 f = -cam.transform.forward;
                f.y = 0f;
                if (f.sqrMagnitude > 0.01f) dir = f.normalized;
            }
            return dir;
        }

        // 맵 가운데에서 away 방향으로 끝까지 간 칸(edgeInset만큼 안쪽)에 섬. 그 칸이 장애물이면 가장 가까운 바닥으로
        private void MoveToFarEdge(Vector3 away)
        {
            int halfX = Grid.gridSizeX / 2, halfZ = Grid.gridSizeZ / 2;
            var target = new Vector2Int(
                Mathf.Clamp(halfX + Mathf.RoundToInt(away.x * (halfX - edgeInset)), 0, Grid.gridSizeX - 1),
                Mathf.Clamp(halfZ + Mathf.RoundToInt(away.z * (halfZ - edgeInset)), 0, Grid.gridSizeZ - 1));

            Vector2Int cell = Grid.GetNearestFloor(target);
            transform.position = Grid.CellToWorld(cell, transform.position.y);
        }

        // 머티리얼을 인스펙터에서 넣지 않았을 때 쓰는 반투명 하늘색 (URP Unlit)
        private static Material CreateIceMaterial()
        {
            Color color = new Color(0.75f, 0.93f, 1f, 0.55f);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) return new Material(Shader.Find("Sprites/Default")) { color = color };

            var mat = new Material(shader);
            mat.SetFloat("_Surface", 1f);                           // Transparent
            mat.SetFloat("_Blend", 0f);                             // Alpha
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.SetColor("_BaseColor", color);
            return mat;
        }
    }
}
