using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : SingletonMonoBehaviour<SceneManager>
{
    [Header("씬 세팅")]
    public string mainMenuSceneName = "Mainmunu";
    public string gameSceneName = "GameScene";
    public string lodingSceneName = "Loding";

    public bool useFadeEffect = true;

    [Header("페이드 세팅")]
    public float fadeSpeed = 2f;
    public Color fadeColor = Color.white;

    private string currentSceneName;
    private string targetSceneName;
    private bool isLoading = false;
    private CanvasGroup fadeCanvasGroup;
    private GameObject fadeObject;

    protected override void Awake()
    {
        base.Awake();

        currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (useFadeEffect)
        {
            CreateFadeUI();
        }
    }

    private void CreateFadeUI()
    {
        fadeObject = new GameObject("FadeCanvas");

        Canvas canvas = fadeObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        fadeCanvasGroup = fadeObject.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;

        GameObject fadeImage = new GameObject("FadeImage");
        fadeImage.transform.SetParent(fadeObject.transform);

        UnityEngine.UI.Image image = fadeImage.AddComponent<UnityEngine.UI.Image>();

        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

    }

    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            Debug.Log("이미 씬 로딩 중입니다.");
            return;
        }

        targetSceneName = sceneName;

        if (useFadeEffect)
        {
            StartCoroutine(LoadSceneWithFade(sceneName));
        }
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        isLoading = true;
        GameManager.Instance.ChangeGameState(GameState.Loading);

        yield return StartCoroutine(FadeIn());

        yield return StartCoroutine(FadeOut());

        isLoading = false;
    }

    private IEnumerator FadeIn()
    {
        fadeCanvasGroup.blocksRaycasts = true;

        while (fadeCanvasGroup.alpha < 1f)
        {
            fadeCanvasGroup.alpha += fadeSpeed * Time.deltaTime;
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        if (fadeCanvasGroup == null) yield break;

        while (fadeCanvasGroup.alpha > 0f)
        {
            fadeCanvasGroup.alpha -= fadeSpeed * Time.deltaTime;
            yield return null;
        }

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }
}
