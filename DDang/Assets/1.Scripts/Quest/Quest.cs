using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Quest : MonoBehaviour
{
    public TextMeshProUGUI titleText;

    public TextMeshProUGUI p1_ProgressText;
    public TextMeshProUGUI p2_ProgressText;

    private int goal;

    public void SetQuestInfo(string title, int goal)
    {
        titleText.text = title;
        this.goal = goal;
        UpdateProgress(0, 0);
    }

    public void UpdateProgress(int p1, int p2)
    {
        p1_ProgressText.text = goal > 1 ? $"P1 : {p1} / {goal}" : "P1 : 도전 중";
        p2_ProgressText.text = goal > 1 ? $"P2 : {p2} / {goal}" : "P2 : 도전 중";
    }

    // 퀘스트별 안내 문구 (예: 역전 퀘스트)
    public void SetLines(string p1, string p2)
    {
        p1_ProgressText.text = p1;
        p2_ProgressText.text = p2;
    }

    public void CompleteQuest(PlayerType player, int reward)
    {
        if (player == PlayerType.Player1)
        {
            p1_ProgressText.text = $"P1 : 완료! +{reward} 당근";
        }
        else if (player == PlayerType.Player2)
        {
            p2_ProgressText.text = $"P2 : 완료! +{reward} 당근";
        }
    }
}
