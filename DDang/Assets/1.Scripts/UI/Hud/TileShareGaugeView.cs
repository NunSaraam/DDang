using UnityEngine;

namespace UI
{
    // 플레이어별 점유율 게이지 (보여 주기만). 목표 비율은 TileShareGaugePresenter가 칸 수가 바뀔 때 넣어 줌
    public class TileShareGaugeView : MonoBehaviour
    {
        [System.Serializable]
        private class Gauge
        {
            public RectTransform bar;
            public RectTransform runner;
            public bool fromRight;              // 오른쪽부터 차오르면 체크

            [HideInInspector] public float shown;       // 화면에 보이는 값
            [HideInInspector] public float velocity;
        }

        [SerializeField] private Gauge p1;
        [SerializeField] private Gauge p2;

        [Header("연출")]
        [SerializeField, Min(0f)] private float fillSmoothTime = 0.25f;  // 목표치까지 차오르는 시간
        [SerializeField] private float hopHeight = 6f;
        [SerializeField] private float hopSpeed = 14f;
        [SerializeField] private float movingSpeedThreshold = 0.05f;    // 초당 이 비율 이상 변하면 뛰는 중

        private float p1Target, p2Target;

        public void SetTargets(float p1Ratio, float p2Ratio)
        {
            p1Target = p1Ratio;
            p2Target = p2Ratio;
        }

        // 연출만: 목표치까지 부드럽게 차오르고, 변하는 동안 끝의 토끼가 뜀
        private void Update()
        {
            UpdateGauge(p1, p1Target);
            UpdateGauge(p2, p2Target);
        }

        private void UpdateGauge(Gauge g, float target)
        {
            g.shown = Mathf.SmoothDamp(g.shown, target, ref g.velocity, fillSmoothTime);

            // 앵커로 막대 길이 조절 -> 트랙 크기와 해상도에 관계없이 비율대로 늘어나고 둥근 끝이 유지됨
            float start = g.fromRight ? 1f - g.shown : 0f;
            float end = g.fromRight ? 1f : g.shown;
            g.bar.anchorMin = new Vector2(start, 0f);
            g.bar.anchorMax = new Vector2(end, 1f);
            g.bar.offsetMin = g.bar.offsetMax = Vector2.zero;
            g.bar.gameObject.SetActive(g.shown > 0.001f);

            if (g.runner == null) return;

            float tip = g.fromRight ? start : end;                       // 막대의 안쪽 끝
            g.runner.anchorMin = g.runner.anchorMax = new Vector2(tip, 0.5f);

            bool moving = Mathf.Abs(g.velocity) > movingSpeedThreshold;
            float hop = moving ? Mathf.Abs(Mathf.Sin(Time.time * hopSpeed)) * hopHeight : 0f;
            g.runner.anchoredPosition = new Vector2(0f, hop);
        }
    }
}
