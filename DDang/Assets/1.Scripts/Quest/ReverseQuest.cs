using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewReverseQuest", menuName = "Progress / ReverseQuest")]

public class ReverseQuest : QuestSO
{
    private bool extraTimeStarted;

    private PlayerType behindPlayer = PlayerType.None;

    private bool p1Completed;
    private bool p2Completed;

    public override void ResetQuest()
    {
        extraTimeStarted = false;
        behindPlayer = PlayerType.None;

        p1Completed = false;
        p2Completed = false;
    }

    public override void OnExtraTimeStarted(int p1Score, int p2Score)
    {
        extraTimeStarted = true;

        if (p1Score < p2Score)
        {
            behindPlayer = PlayerType.Player1;
        }
        else if (p1Score > p2Score)
        {
            behindPlayer = PlayerType.Player2;
        }
        else
        {
            behindPlayer= PlayerType.None;
        }

        if (behindPlayer == PlayerType.Player1)
            QuestUI.Instance.SetQuestLines(this, "P1 : 역전하면 성공!", "P2 : 지켜내기!");
        else if (behindPlayer == PlayerType.Player2)
            QuestUI.Instance.SetQuestLines(this, "P1 : 지켜내기!", "P2 : 역전하면 성공!");
    }

    public override void OnRoundEnd(int p1Score, int p2Score, PlayerType winner)
    {
        if (!extraTimeStarted) return;

        if (behindPlayer == PlayerType.None) return;

        if (winner == PlayerType.None) return;

        if (winner == behindPlayer)
        {
            if (winner == PlayerType.Player1 && !p1Completed)
            {
                p1Completed = true;
                QuestManager.Instance.CompleteQuest(this, PlayerType.Player1);
            }
            else if (winner == PlayerType.Player2 && !p2Completed)
            {
                p2Completed = true;
                QuestManager.Instance.CompleteQuest(this, PlayerType.Player2);
            }
        }
    }
}
