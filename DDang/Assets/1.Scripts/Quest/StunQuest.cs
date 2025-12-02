using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStunQuest", menuName = "Progress / StunQuest")]
public class StunQuest : QuestSO
{
    public int requiredChain = 3;           //±âº» 3

    private int p1Chain;
    private int p2Chain;

    private bool p1Completed;
    private bool p2Completed;

    public override void ResetQuest()
    {
        p1Chain = 0;
        p2Chain = 0;

        p1Completed = false;
        p2Completed = false;
    }

    public override void OnStunLanded(PlayerType attacker)
    {
        switch (attacker)
        {
            case PlayerType.Player1:
                if (p1Completed) return;
                p1Chain++;
                p2Chain = 0;

                QuestUI.Instance.UpdateQuestUI(this, p1Chain, p2Chain);

                if (p1Chain >= requiredChain)
                {
                    p1Completed = true;
                    QuestManager.Instance.CompleteQuest(this, PlayerType.Player1);
                    QuestUI.Instance.CompleteQuestUI(this, PlayerType.Player1, rewardCoins);
                }
                break;

            case PlayerType.Player2:
                if (p2Completed) return;
                p2Chain++;
                p1Chain = 0;

                QuestUI.Instance.UpdateQuestUI(this, p1Chain, p2Chain);

                if (p2Chain >= requiredChain)
                {
                    p2Completed = true;
                    QuestManager.Instance.CompleteQuest(this, PlayerType.Player2);
                    QuestUI.Instance.CompleteQuestUI(this, PlayerType.Player2, rewardCoins);
                }
                break;
        }
    }

    public override void OnRoundEnd(int p1Score, int p2Score, PlayerType winner)
    {
        ResetQuest();
    }
}
