using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Product", menuName = "Store/Store")]
public class StoreData : ScriptableObject
{
    public string productName;
    public string description;
    public StoreType type;

    [Header("업그레이드 스텟")]
    public float moveSpeed = 1f;
    public float stunReduce = .5f;
}
