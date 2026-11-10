using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    [Header("씬 전환 유지 설정")]
    [SerializeField] private bool useDontDestroyOnLoad = true;

    protected static T _instance;
    private bool _initialized;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();
                if (_instance == null)
                    Debug.LogWarning($"현재 씬에 {typeof(T).Name}가 없습니다.");
            }
            return _instance;
        }
    }

    /// <summary>검색/로그 없이 존재 여부만 확인 (OnDisable 구독 해제용)</summary>
    public static bool HasInstance => _instance != null;

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // _instance == null 이거나, Instance getter가 Awake 전에 미리 찾아둔 경우(_instance == this)
        _instance = (T)this;
        if (_initialized) return;
        _initialized = true;

        if (useDontDestroyOnLoad)
        {
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
            else
                Debug.LogWarning($"{typeof(T).Name}는 부모가 있어 DontDestroyOnLoad가 적용되지 않습니다.");
        }

        OnSingletonAwake();
    }

    /// <summary>인스턴스 설정이 완료된 경우에만 1회 호출</summary>
    protected virtual void OnSingletonAwake() { }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}