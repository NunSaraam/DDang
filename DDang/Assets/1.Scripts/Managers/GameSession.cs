using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance;

    public bool needFullReset = false;

    public GameObject SceneLoadManagerPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        if (SceneLoadManager.Instance == null)
        {
            Instantiate(SceneLoadManagerPrefab);
        }
    }
}
