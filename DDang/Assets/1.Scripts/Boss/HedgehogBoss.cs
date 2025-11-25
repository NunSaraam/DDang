using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HedgehogBoss : BossBase
{
    public float fireInterval = 2f;
    public float projectileSpeed = 10f;
    public GameObject spikePrefab;

    private bool active = false;

    public override void Activate()
    {
         active = true;
        StartCoroutine(FireLoop());
    }

    public override void Deactivate()
    {
        active = false;
    }

    IEnumerator FireLoop()
    {
        while (active)
        {
            FireAllDirections();
            yield return new WaitForSeconds(fireInterval);
        }
    }

    void FireAllDirections()
    {
        int count = 12;
        for (int i = 0; i < count; i++)
        {
            float angle = i * (360f / count);           //12 방향
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 spawnPos = transform.position + Vector3.up * .5f;               //바닥 타일 콜라이더 닿음 방지 .5 올림

            GameObject spike = Instantiate(spikePrefab, spawnPos, Quaternion.identity);
            spike.GetComponent<Rigidbody>().velocity = dir * projectileSpeed;

            
        }
    }
}
