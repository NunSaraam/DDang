using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    public List<AchievementSO> Achievements;

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

    public int GetValue(PlayerType player, AchievementType type)
    {
        return PlayerPrefs.GetInt($"{player}_{type}", 0);
    }

    public void AddValue(PlayerType player, AchievementType type, int amount = 1)
    {
        string key = $"{player}_{type}";
        int current = PlayerPrefs.GetInt(key, 0);
        AchievementSO data = Achievements.Find(a => a.type == type);

        int updated = current + amount;
        if (data != null) updated = Mathf.Min(updated, data.goal);
        if (updated == current) return;                         // 이미 달성 → 할 일 없음
        PlayerPrefs.SetInt(key, updated);

        AchievementUIManager ui = AchievementUIManager.Instance;
        if (ui != null && data != null)                         // 메뉴 씬에서만 UI 갱신
        {
            ui.UpdateProgress(player, data);
            if (updated >= data.goal) ui.UnlockAchievement(player, data);
        }
    }

    private void OnApplicationQuit() => PlayerPrefs.Save();    // 저장은 종료할 때 한 번
}
