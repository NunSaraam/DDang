using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlacedObj : MonoBehaviour
{
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();

    }

    public void SetMaterial(Material mat)
    {
        if (rend != null && mat != null)
        {
            rend.material = mat;
        }
    }
}
