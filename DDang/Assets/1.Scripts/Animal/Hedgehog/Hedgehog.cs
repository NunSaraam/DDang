using System.Collections;
using UnityEngine;

namespace Animal
{
    // 고슴도치: 포탑형. 2초마다 12방향으로 가시 발사.
    public class Hedgehog : AnimalBase
    {
        public float fireInterval = 2f;
        public float projectileSpeed = 10f;
        public GameObject spikePrefab;

        protected override void OnSpawn() => StartCoroutine(FireLoop());

        private IEnumerator FireLoop()
        {
            while (IsActive)
            {
                FireAllDirections();
                yield return new WaitForSeconds(fireInterval);
            }
        }

        private void FireAllDirections()
        {
            const int count = 12;
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = Quaternion.Euler(0, i * (360f / count), 0) * Vector3.forward;
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;     // 바닥 블록 콜라이더 위로
                GameObject spike = Instantiate(spikePrefab, spawnPos, Quaternion.identity);
                spike.GetComponent<Rigidbody>().linearVelocity = dir * projectileSpeed;
            }
        }
    }
}
