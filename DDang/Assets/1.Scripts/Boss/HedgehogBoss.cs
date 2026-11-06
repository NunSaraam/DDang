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
            FireAllDirections(10f);
            yield return new WaitForSeconds(fireInterval);
        }
    }

    void FireAllDirections(float lifeTime)
    {
        int count = 12;
        for (int i = 0; i < count; i++)
        {
            float angle = i * (360f / count);           //12 ����
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 spawnPos = transform.position + Vector3.up * .5f;               //�ٴ� Ÿ�� �ݶ��̴� ���� ���� .5 �ø�

            GameObject spike = Instantiate(spikePrefab, spawnPos, Quaternion.identity);
            spike.GetComponent<Rigidbody>().linearVelocity = dir * projectileSpeed;

            
        }

        Destroy(gameObject, lifeTime);
    }
}
