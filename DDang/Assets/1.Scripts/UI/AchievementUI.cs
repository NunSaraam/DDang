using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementUIManager : MonoBehaviour
{
    public static AchievementUIManager Instance;

    public Transform p1Content;
    public Transform p2Content;

    public GameObject achievementPrefab;

    private Dictionary<(PlayerType, AchievementSO), AchievementUI> uiItems = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadAllAchievementsUI();
    }

    public void LoadAllAchievementsUI()
    {
        foreach (var ach in AchievementManager.Instance.Achievements)
        {
            CreateUI(PlayerType.Player1, ach);
            CreateUI(PlayerType.Player2, ach);
        }
    }

    void CreateUI(PlayerType p, AchievementSO ach)
    {
        Transform parent = (p == PlayerType.Player1) ? p1Content : p2Content;

        GameObject obj = Instantiate(achievementPrefab, parent);
        var ui = obj.GetComponent<AchievementUI>();

        int current = AchievementManager.Instance.GetValue(p, ach.type);

        ui.Setup(ach, current, p);

        uiItems.Add((p, ach), ui);
    }

    public void UnlockAchievement(PlayerType p, AchievementSO ach)
    {
        if (uiItems.TryGetValue((p, ach), out var ui))
        {
            ui.MarkAsCompleted();
        }
    }


    public void UpdateProgress(PlayerType p, AchievementSO ach)
    {
        if (uiItems.TryGetValue((p, ach), out var ui))
        {
            int current = AchievementManager.Instance.GetValue(p, ach.type);
            int clamped = Mathf.Min(current, ach.goal);
            ui.UpdateProgress(clamped);
        }
    }
}
