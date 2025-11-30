using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class QuestSO : ScriptableObject
{
    public string questName = "QuestName";
    public string description;

    public int rewardCoins = 10;

    public abstract void ResetQuest();

    public virtual void OnStunLanded(PlayerType attacker)
    {

    }

    public virtual void OnExtraTimeStarted(int p1Score, int p2Score)
    {

    }

    public virtual void OnRoundEnd(int p1Score, int p2Score, PlayerType winner)
    {

    }


}
