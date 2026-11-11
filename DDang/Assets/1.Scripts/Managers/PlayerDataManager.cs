using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    public float baseMoveSpeed = 5f;

    public float p1MoveSpeed = 5f;
    public float p2MoveSpeed = 5f;

    public float baseStunTime = 1.5f;

    public float p1StunTime = 1.5f;
    public float p2StunTime = 1.5f;

    [Header("스탯 하한")]
    public float minStunTime = 0.3f;
    [Range(0f, 1f)] public float minMoveSpeedRatio = 0.5f;     // 기본 이동속도 대비

    public float ClampMoveSpeed(float speed) => Mathf.Max(speed, baseMoveSpeed * minMoveSpeedRatio);
    public float ClampStunTime(float time) => Mathf.Max(time, minStunTime);

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
}
