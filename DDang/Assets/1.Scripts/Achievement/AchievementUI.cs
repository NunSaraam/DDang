using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI progress;

    private int goal;
    private PlayerType player;
    private AchievementSO data;

    public void Setup(AchievementSO so, int current, PlayerType p)
    {
        data = so;
        player = p;
        goal = so.goal;

        icon.sprite = so.icon;
        title.text = so.achievementName;
        description.text = so.description;

        UpdateProgress(current);
    }

    public void UpdateProgress(int current)
    {
        int clamped = Mathf.Min(current, goal);
        progress.text = $"{current} / {goal}";
    }

    public void MarkAsCompleted()
    {
        progress.text = "¿Ï·áµÊ!";
        progress.color = Color.yellow;
    }
}
