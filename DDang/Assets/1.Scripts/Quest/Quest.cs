using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Quest : MonoBehaviour
{
    public TextMeshProUGUI titleText;

    public TextMeshProUGUI p1_ProgressText;
    public TextMeshProUGUI p2_ProgressText;

    public void SetQuestInfo(string title)
    {
        titleText.text = title;

        p1_ProgressText.text = "P1 : 0 / 3";
        p2_ProgressText.text = "P2 : 0 / 3";
    }

    public void UpdateProgress(int p1, int p2)
    {
        p1_ProgressText.text = $"P1 : {p1} / 3";
        p2_ProgressText.text = $"P2 : {p2} / 3";
    }

    public void CompleteQuest(PlayerType player, int reward)
    {
        if (player == PlayerType.Player1)
        {
            p1_ProgressText.text = $"P1 : 완료! +{reward}포인트 지급!";
        }
        else if (player == PlayerType.Player2)
        {
            p2_ProgressText.text = $"P2 : 완료! +{reward}포인트 지급!";
        }
    }
}
