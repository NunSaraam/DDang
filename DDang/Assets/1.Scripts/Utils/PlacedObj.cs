using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedObj : MonoBehaviour
{
    public PlayerType owner = PlayerType.None;              //onwer = «ˆ¿Á ∂• ¡÷¿Œ
    public KeyCheckPlayerType kowner = KeyCheckPlayerType.None;
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

    public void KeyCheckSetMaterial(Material mat, KeyCheckPlayerType kpT)
    {
        if (rend != null && mat != null)
        {
            rend.material = mat;
            kowner = kpT;
        }
    }
}
