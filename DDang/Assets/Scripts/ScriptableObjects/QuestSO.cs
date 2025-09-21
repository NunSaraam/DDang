using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Quest/NewQuest")]
public class QuestSO : ScriptableObject
{
    public string questID;
    public string questName = "퀘스트 이름";
    [TextArea] public string description = "퀘스트 설명";
}
