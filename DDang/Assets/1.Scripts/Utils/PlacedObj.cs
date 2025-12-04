using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedObj : MonoBehaviour
{
    public PlayerType owner = PlayerType.None;              //onwer = 현재 땅 주인

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();

    }

    public void SetMaterial(Material mat, PlayerType pT)
    {
        if (rend != null && mat != null)
        {
            rend.material = mat;
            owner = pT;
        }
    }
}
