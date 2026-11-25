using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Animal
{
    // 부엉이: 하늘에서 날아와 맵 위쪽 끝 가운데에 앉고, "부엉" 하면 양쪽 화면이 어두워짐.
    //         각 화면은 자기 플레이어 주변 반경 3칸만 보임. 끝나기 직전 어둠이 걷히며 날아감
    // 어둠: 카메라마다 Screen Space - Camera 캔버스 + 화면 전체 RawImage(가운데 구멍 텍스처, Clamp).
    //       uvRect를 매 프레임 바꿔서 구멍을 플레이어 위치·반경 3칸 크기에 맞춤 (구멍 밖은 텍스처 가장자리 = 검정)
    // 프리팹: 루트에 이 스크립트, 모델은 첫 번째 자식. 콜라이더 불필요
    public class Owl : AnimalBase
    {
        [Header("어둠")]
        [SerializeField] private float visibleCells = 1.5f;         // 보이는 반경 (칸). 카메라가 가까워 3칸이면 화면 거의 전부라 1.5
        [SerializeField, Range(0f, 1f)] private float darkness = 1f;              // 1 = 완전 암전. Linear 색 공간이라 0.98만 돼도 바깥이 어둑하게 다 보임
        [SerializeField, Range(0.05f, 0.9f)] private float edgeSoftness = 0.25f;   // 구멍 가장자리 흐린 정도 (반경 대비)
        [SerializeField] private float fadeInTime = 0.6f;
        [SerializeField] private float fadeOutTime = 0.6f;

        [Header("부엉이")]
        [SerializeField] private bool standAtFarEdge = true;        // 화면 위쪽(카메라에서 먼 쪽) 끝 가운데에 앉음
        [SerializeField, Min(0)] private int edgeInset = 1;
        [SerializeField] private float flyHeight = 6f;              // 날아올 때 시작 높이
        [SerializeField] private float flyTime = 0.9f;
        [SerializeField] private float lookAngle = 40f;             // 고개(몸) 두리번 각도
        [SerializeField] private float lookInterval = 1.1f;

        private const int TexSize = 512;
        private const float HoleUv = 0.3f;                          // 텍스처 안 구멍 반경 (uv)

        private class Overlay
        {
            public Camera cam;
            public Transform target;
            public GameObject root;
            public RectTransform canvasRect;
            public RawImage image;
        }

        private readonly List<Overlay> overlays = new();
        private Texture2D holeTexture;
        private float overlayAlpha;
        private Transform model;
        private Vector3 modelPos, modelScale;
        private Quaternion modelRot;
        private bool leaving;

        protected override void OnSpawn()
        {
            Vector3 toCamera = AnimalFx.DirectionToCamera();
            if (standAtFarEdge)
            {
                Vector2Int cell = AnimalFx.FarEdgeCell(Grid, -toCamera, edgeInset);
                transform.position = Grid.CellToWorld(cell, transform.position.y);
            }
            transform.rotation = Quaternion.LookRotation(toCamera);

            model = transform.childCount > 0 ? transform.GetChild(0) : null;
            if (model != null)
            {
                modelPos = model.localPosition;
                modelScale = model.localScale;
                modelRot = model.localRotation;
            }

            StartCoroutine(MainRoutine());
        }

        protected override void OnDespawn() => Cleanup();
        private void OnDestroy() => Cleanup();                      // 씬이 바뀌어 Finish 없이 사라질 때도

        private void Cleanup()
        {
            foreach (Overlay o in overlays) if (o.root != null) Destroy(o.root);
            overlays.Clear();
            if (holeTexture != null) Destroy(holeTexture);
            holeTexture = null;
        }

        private IEnumerator MainRoutine()
        {
            yield return FlyIn();
            yield return Hoot();

            CreateOverlays();
            StartCoroutine(LookAround());

            for (float t = 0f; t < fadeInTime; t += Time.deltaTime)
            {
                overlayAlpha = darkness * (t / fadeInTime);
                yield return null;
            }
            overlayAlpha = darkness;

            // 지속 시간이 끝나기 직전에 어둠을 걷고 날아감
            float spawnedFor = flyTime + 0.5f + fadeInTime;
            float wait = Data.duration - spawnedFor - fadeOutTime - 0.05f;
            if (wait > 0f) yield return new WaitForSeconds(wait);

            leaving = true;
            StartCoroutine(FlyOut());
            for (float t = 0f; t < fadeOutTime; t += Time.deltaTime)
            {
                overlayAlpha = darkness * (1f - t / fadeOutTime);
                yield return null;
            }
            overlayAlpha = 0f;
        }

        // ───────── 어둠 ─────────

        private void CreateOverlays()
        {
            holeTexture = CreateHoleTexture();

            var manager = FindFirstObjectByType<DualCameraManager>();
            if (manager != null)
            {
                AddOverlay(manager.cam1, manager.PlayCam1 != null && manager.PlayCam1.target != null ? manager.PlayCam1.target : manager.player1);
                AddOverlay(manager.cam2, manager.PlayCam2 != null && manager.PlayCam2.target != null ? manager.PlayCam2.target : manager.player2);
            }
            else                                                    // 매니저가 없으면 플레이어를 따라가는 카메라를 직접 찾음
            {
                foreach (var follow in FindObjectsByType<Cameras.PlayerDualCamera>(FindObjectsSortMode.None))
                    AddOverlay(follow.GetComponent<Camera>(), follow.target);
            }

            if (overlays.Count == 0) Debug.LogWarning("Owl: 화면 카메라를 찾지 못해 어둠을 만들 수 없습니다.", this);
        }

        private void AddOverlay(Camera cam, Transform target)
        {
            if (cam == null) return;

            var root = new GameObject($"OwlDarkness ({cam.name})");
            root.layer = 5;                                         // UI
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;       // 이 카메라 화면에만 그려짐
            canvas.worldCamera = cam;
            canvas.planeDistance = cam.nearClipPlane + 0.05f;       // 월드 물체보다 앞

            var imageObj = new GameObject("Dark");
            imageObj.layer = 5;
            imageObj.transform.SetParent(root.transform, false);
            var image = imageObj.AddComponent<RawImage>();
            image.texture = holeTexture;
            image.raycastTarget = false;
            image.color = new Color(1f, 1f, 1f, 0f);
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            overlays.Add(new Overlay { cam = cam, target = target, root = root, canvasRect = (RectTransform)root.transform, image = image });
        }

        private void LateUpdate()
        {
            foreach (Overlay o in overlays)
            {
                if (o.image == null || o.cam == null) continue;
                o.image.color = new Color(1f, 1f, 1f, overlayAlpha);
                if (o.target == null) { o.image.uvRect = new Rect(-10f, -10f, 1f, 1f); continue; }   // 대상이 없으면 전부 어둡게

                Vector3 center = o.target.position;
                Vector3 edge = center + o.cam.transform.right * visibleCells;
                Vector3 sc = o.cam.WorldToScreenPoint(center);
                Vector3 se = o.cam.WorldToScreenPoint(edge);
                if (sc.z <= 0f) continue;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(o.canvasRect, sc, o.cam, out Vector2 lc);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(o.canvasRect, se, o.cam, out Vector2 le);
                float radius = Mathf.Max(1f, Vector2.Distance(lc, le));

                Vector2 size = o.canvasRect.rect.size;
                float w = HoleUv * size.x / radius;
                float h = HoleUv * size.y / radius;
                // 화면 왼쪽 아래(0) ~ 오른쪽 위(1) 기준 위치
                float px = (lc.x - o.canvasRect.rect.xMin) / size.x;
                float py = (lc.y - o.canvasRect.rect.yMin) / size.y;
                o.image.uvRect = new Rect(0.5f - px * w, 0.5f - py * h, w, h);
            }
        }

        // 가운데가 투명하고 가장자리로 갈수록 검게 되는 텍스처. 바깥은 완전히 검정 (Clamp라 텍스처 밖도 검정)
        private Texture2D CreateHoleTexture()
        {
            var tex = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "OwlHole"
            };
            var pixels = new Color32[TexSize * TexSize];
            float inner = HoleUv * (1f - edgeSoftness);
            for (int y = 0; y < TexSize; y++)
                for (int x = 0; x < TexSize; x++)
                {
                    float dx = (x + 0.5f) / TexSize - 0.5f, dy = (y + 0.5f) / TexSize - 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(inner, HoleUv, d));
                    pixels[y * TexSize + x] = new Color32(0, 0, 0, (byte)(a * 255f));
                }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return tex;
        }

        // ───────── 부엉이 연출 ─────────

        private IEnumerator FlyIn()
        {
            Vector3 land = transform.position;
            Vector3 from = land + Vector3.up * flyHeight - transform.forward * 3f;   // 뒤쪽 하늘에서
            for (float t = 0f; t < flyTime; t += Time.deltaTime)
            {
                float k = t / flyTime;
                float ease = 1f - (1f - k) * (1f - k);
                transform.position = Vector3.Lerp(from, land, ease);
                Flap(t, 1f - k);
                yield return null;
            }
            transform.position = land;
            yield return Squash(0.25f, 0.15f);
        }

        private IEnumerator FlyOut()
        {
            Vector3 from = transform.position;
            Vector3 to = from + Vector3.up * flyHeight - transform.forward * 3f;
            for (float t = 0f; t < fadeOutTime; t += Time.deltaTime)
            {
                float k = t / fadeOutTime;
                transform.position = Vector3.Lerp(from, to, k * k);
                Flap(t, 1f);
                yield return null;
            }
        }

        // 날갯짓: 옆으로 넓어졌다 좁아짐
        private void Flap(float time, float strength)
        {
            if (model == null) return;
            float s = Mathf.Sin(time * 30f) * 0.18f * strength;
            model.localScale = Vector3.Scale(modelScale, new Vector3(1f + s, 1f - s * 0.5f, 1f));
        }

        // "부엉!" 몸을 부풀렸다가 돌아옴
        private IEnumerator Hoot()
        {
            for (int i = 0; i < 2; i++)
            {
                yield return Squash(-0.15f, 0.12f);                // 위로 늘어남
                yield return Squash(0.1f, 0.12f);
            }
            SetPose(0f, 0f);
        }

        private IEnumerator Squash(float amount, float time)
        {
            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                SetPose(0f, amount * Mathf.Sin(t / time * Mathf.PI));
                yield return null;
            }
            SetPose(0f, 0f);
        }

        // 부엉이처럼 몸(고개)을 홱 돌렸다 멈췄다
        private IEnumerator LookAround()
        {
            float current = 0f;
            while (IsActive && !leaving)
            {
                float target = Random.Range(-lookAngle, lookAngle);
                if (Mathf.Abs(target - current) < lookAngle * 0.4f) target = -Mathf.Sign(current + 0.01f) * lookAngle * Random.Range(0.6f, 1f);
                const float turn = 0.12f;
                for (float t = 0f; t < turn; t += Time.deltaTime)
                {
                    SetPose(Mathf.Lerp(current, target, t / turn), 0f);
                    yield return null;
                }
                current = target;
                SetPose(current, 0f);
                yield return new WaitForSeconds(lookInterval * Random.Range(0.7f, 1.3f));
            }
        }

        // yaw: 좌우로 돌린 각도, squash: 양수면 납작
        private void SetPose(float yaw, float squash)
        {
            if (model == null) return;
            model.localPosition = modelPos;
            model.localRotation = modelRot * Quaternion.Euler(0f, yaw, 0f);
            model.localScale = Vector3.Scale(modelScale, new Vector3(1f + squash, 1f - squash, 1f + squash));
        }
    }
}
