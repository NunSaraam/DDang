using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "Quest/NewQuest")]
public class QuestSO : ScriptableObject
{
    public string questID;
    public string questName = "����Ʈ �̸�";
    [TextArea] public string description = "����Ʈ ����";
}
