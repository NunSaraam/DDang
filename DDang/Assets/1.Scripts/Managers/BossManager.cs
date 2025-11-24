using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;

    public GameObject[] Boss;

    public Transform spawnPoint;
    private BossBase currentBoss;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SpawnRandomBoss()
    {
        int rand = Random.Range(0, Boss.Length);
        GameObject prefab = Boss[rand];

        currentBoss = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation).GetComponent<BossBase>();
        currentBoss.Activate();
    }
}
