using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public static QuestUI Instance;

    public RectTransform questPanel;
    public RectTransform content;

    public GameObject questPrefab;

    public float slideDuration = .4f;

    [SerializeField] private float hideDelay = 1.5f;

    private Dictionary<QuestSO, Quest> activeQuest = new();
    private Coroutine hideRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        questPanel.gameObject.SetActive(false);
    }

    public void CreateQuest(QuestSO qs)
    {
        if (hideRoutine != null) { StopCoroutine(hideRoutine); hideRoutine = null; }
        questPanel.gameObject.SetActive(true);
        GameObject obj = Instantiate(questPrefab, content);
        var pf = obj.GetComponent<Quest>();

        pf.SetQuestInfo(qs.description);

        activeQuest.Add(qs, pf);

        StartCoroutine(SlideDownRouTine());
    }

    public void ClearAllQuestUI()
    {
        foreach (var kv in activeQuest)
        {
            Destroy(kv.Value.gameObject);
        }

        activeQuest.Clear();
        HideIfEmpty(0f);
    }

    public void UpdateQuestUI(QuestSO qs, int p1, int p2)
    {
        if (activeQuest.TryGetValue(qs, out Quest pf))
        {
            pf.UpdateProgress(p1, p2);
        }
    }

    public void CompleteQuestUI(QuestSO qs, PlayerType player, int reward)
    {
        if (activeQuest.TryGetValue(qs, out Quest pf))
        {
            pf.CompleteQuest(player, reward);
            Destroy(pf.gameObject, 1.5f);
            activeQuest.Remove(qs);
        }
        HideIfEmpty(hideDelay);
    }

    public void SlideDown()
    {
        StartCoroutine(SlideDownRouTine());
    }

    IEnumerator SlideDownRouTine()
    {
        Vector2 start = new Vector2(0, 200);
        Vector2 end = new Vector2(0, 2);

        questPanel.gameObject.SetActive(true);
        questPanel.anchoredPosition = start;

        float t = 0;
        while (t < slideDuration)
        {
            t += Time.deltaTime;
            questPanel.anchoredPosition = Vector2.Lerp(start, end, t / slideDuration);
            yield return null;
        }

        questPanel.anchoredPosition = end;
    }

    public void RemoveQuest(QuestSO qs)
    {
        if (activeQuest.TryGetValue(qs, out Quest pf))
        {
            Destroy(pf.gameObject);
            activeQuest.Remove(qs);
        }
        HideIfEmpty(0f);
    }

    private void HideIfEmpty(float delay)
    {
        if (activeQuest.Count > 0) return;
        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(HidePanelRoutine(delay));
    }

    IEnumerator HidePanelRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector2 start = questPanel.anchoredPosition;
        Vector2 end = new Vector2(0, 200);
        for (float t = 0; t < slideDuration; t += Time.deltaTime)
        {
            questPanel.anchoredPosition = Vector2.Lerp(start, end, t / slideDuration);
            yield return null;
        }

        questPanel.gameObject.SetActive(false);
        hideRoutine = null;
    }
}
