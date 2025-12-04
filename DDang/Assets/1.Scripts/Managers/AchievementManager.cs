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
        int current = PlayerPrefs.GetInt(key , 0);
        int updated = current + amount;
        AchievementSO data = Achievements.Find(a => a.type == type);

        if (data != null)
        {
            updated = Mathf.Min(updated, data.goal); 
        }

        PlayerPrefs.SetInt(key , updated);
        PlayerPrefs.Save();

        AchievementUIManager.Instance?.UpdateProgress(player, data);

        CheckAchievemests(player);
    }

    private void CheckAchievemests(PlayerType player)
    {
        foreach (var ach in Achievements)
        {
            int val = GetValue(player, ach.type);

            if (val >= ach.goal)
            {
                AchievementUIManager.Instance.UnlockAchievement(player, ach);
            }
        }
    }
}
