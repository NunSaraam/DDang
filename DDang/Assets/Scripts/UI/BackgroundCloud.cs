using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundCloud : MonoBehaviour
{

     public float scrollSpeed = 0.1f;
    private RawImage rawImage;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
    }
    void Update()
    {
        if (rawImage != null)
        {
            Rect uvRect = rawImage.uvRect;
            uvRect.x += scrollSpeed * Time.deltaTime;
            rawImage.uvRect = uvRect;
        }
    }
    
}
