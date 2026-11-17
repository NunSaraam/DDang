using System.Collections;
using UnityEngine;

namespace Animal
{
    // 고슴도치: 포탑형. 2초마다 12방향으로 가시 발사.
    // 연출: 웅크림 → 부르르(예고) → 발사하며 위로 튀어오름
    public class Hedgehog : AnimalBase
    {
        public float fireInterval = 2f;
        public float projectileSpeed = 10f;
        public GameObject spikePrefab;

        [Header("연출")]
        public float windupTime = 0.5f;             // 발사 전 예고 시간 (발사 간격 안에 포함)
        public float shakeAmount = 0.05f;

        private Transform model;                    // 첫 번째 자식. 콜라이더가 있는 루트는 그대로
        private Vector3 modelScale, modelPos;

        protected override void OnSpawn()
        {
            model = transform.childCount > 0 ? transform.GetChild(0) : transform;
            modelScale = model.localScale;
            modelPos = model.localPosition;

            StartCoroutine(FireLoop());
        }

        private IEnumerator FireLoop()
        {
            const float popTime = 0.08f, recoverTime = 0.15f;

            while (IsActive)
            {
                yield return ScaleTo(new Vector3(1.25f, 0.7f, 1.25f), windupTime * 0.6f);   // 웅크림
                yield return Shake(shakeAmount, windupTime * 0.4f);                        // 부르르
                FireAllDirections();
                yield return ScaleTo(new Vector3(0.85f, 1.3f, 0.85f), popTime);            // 튀어오름
                yield return ScaleTo(Vector3.one, recoverTime);
                yield return new WaitForSeconds(Mathf.Max(0f, fireInterval - windupTime - popTime - recoverTime));
            }
        }

        private void FireAllDirections()
        {
            const int count = 12;
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = Quaternion.Euler(0, i * (360f / count), 0) * Vector3.forward;
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;     // 바닥 블록 콜라이더 위로
                GameObject spike = Instantiate(spikePrefab, spawnPos, Quaternion.LookRotation(dir));   // 날아가는 방향을 바라봄
                spike.GetComponent<Rigidbody>().linearVelocity = dir * projectileSpeed;
            }
        }

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

        private IEnumerator Shake(float amount, float time)
        {
            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                model.localPosition = modelPos + new Vector3(Random.Range(-amount, amount), 0f, Random.Range(-amount, amount));
                yield return null;
            }
            model.localPosition = modelPos;
        }
    }
}
