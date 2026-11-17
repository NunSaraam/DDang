using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animal
{
    // 고릴라: 전체 타일 주인을 섞음. 5초 간격 3회, 장애물·얼린 칸 제외
    // 연출: 가슴 치기(예고) → 점프 → 착지 찌그러짐 → 가까운 칸부터 물결처럼 섞임
    public class Gorilla : AnimalBase
    {
        public float shuffleInterval = 5f;
        public int shuffleCount = 3;

        [Header("연출")]
        public float beatTime = 1f;                 // 가슴 치기 (예고)
        public int beatCount = 4;
        public float jumpHeight = 1.2f;
        public float jumpTime = 0.5f;
        public float rippleDelayPerCell = 0.04f;    // 고릴라에서 한 칸 멀어질 때마다 늦게 바뀜

        private Transform model;                    // 첫 번째 자식. 콜라이더가 있는 루트는 그대로
        private Vector3 modelScale, modelPos;

        protected override void OnSpawn()
        {
            model = transform.childCount > 0 ? transform.GetChild(0) : transform;
            modelScale = model.localScale;
            modelPos = model.localPosition;

            StartCoroutine(ShuffleRoutine());
        }

        private IEnumerator ShuffleRoutine()
        {
            for (int i = 0; i < shuffleCount; i++)
            {
                yield return ChestBeat();
                yield return Hop(jumpHeight, jumpTime);
                StartCoroutine(Slam());
                StartCoroutine(ShuffleRipple());

                if (i < shuffleCount - 1)
                    yield return new WaitForSeconds(Mathf.Max(0f, shuffleInterval - beatTime - jumpTime));
            }
            yield return new WaitForSeconds(1f);    // 마지막 물결이 끝날 때까지
            Finish();
        }

        // 통통 부풀었다 줄었다 반복
        private IEnumerator ChestBeat()
        {
            float half = beatTime / (Mathf.Max(1, beatCount) * 2);
            for (int b = 0; b < beatCount; b++)
            {
                yield return ScaleTo(new Vector3(1.12f, 0.9f, 1.12f), half);
                yield return ScaleTo(Vector3.one, half);
            }
        }

        // 착지: 납작하게 찌그러졌다 복원
        private IEnumerator Slam()
        {
            yield return ScaleTo(new Vector3(1.3f, 0.6f, 1.3f), 0.06f);
            yield return ScaleTo(Vector3.one, 0.15f);
        }

        // 섞을 주인을 먼저 정하고, 고릴라에서 가까운 칸부터 칠함
        private IEnumerator ShuffleRipple()
        {
            var cells = new List<Vector2Int>();
            foreach (Vector2Int c in Grid.FloorCells)
                if (!Grid.IsLocked(c)) cells.Add(c);

            var owners = cells.ConvertAll(c => Grid.GetBlock(c).owner);
            for (int i = owners.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (owners[i], owners[j]) = (owners[j], owners[i]);
            }

            Vector2Int center = CurrentCell;
            var order = new List<int>();
            for (int i = 0; i < cells.Count; i++) order.Add(i);
            order.Sort((a, b) => Distance(cells[a], center).CompareTo(Distance(cells[b], center)));

            float start = Time.time;
            foreach (int i in order)
            {
                float due = Distance(cells[i], center) * rippleDelayPerCell;
                while (Time.time - start < due) yield return null;
                PaintCell(cells[i], owners[i]);             // 그 사이 얼린 칸은 PaintCell이 알아서 건너뜀
            }
        }

        private static float Distance(Vector2Int a, Vector2Int b) => Vector2Int.Distance(a, b);

        private IEnumerator ScaleTo(Vector3 ratio, float time)
        {
            Vector3 from = model.localScale;
            Vector3 to = Vector3.Scale(modelScale, ratio);
            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                model.localScale = Vector3.Lerp(from, to, t / time);
                yield return null;
            }
            model.localScale = to;
        }

        private IEnumerator Hop(float height, float time)
        {
            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                float k = t / time;
                model.localPosition = modelPos + Vector3.up * (4f * height * k * (1f - k));
                yield return null;
            }
            model.localPosition = modelPos;
        }
    }
}
