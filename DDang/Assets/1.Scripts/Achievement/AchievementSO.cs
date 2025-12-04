using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AchievementType
{
    TotalPaintedTiles,          //색칠한 타일의 총 개수
    TotalWins,                  //총 승리한 횟수
    TotalStoreUsed,             //상점을 이용한 총 횟수
}

[CreateAssetMenu(fileName = "NewAchievement", menuName = "Progress / Achievement")]
public class AchievementSO : ScriptableObject
{
    public string achievementName;
    public string description;

    public AchievementType type;
    public int goal;            //달성 횟수 ex : TotalPaintedTiles = 1000개

    public Sprite icon;         //업적 아이콘
}
