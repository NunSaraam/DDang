using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectButtonUI : MonoBehaviour
{
    public RectTransform selectBar;            // 이동시킬 강조 바

    private RectTransform thisRect;

    void Start()
    {
        thisRect = GetComponent<RectTransform>();
    }

    public void OnPointerEnter()
    {
        // 바가 보이지 않았다면 활성화
        if (!selectBar.gameObject.activeSelf)
            selectBar.gameObject.SetActive(true);

        selectBar.anchoredPosition = thisRect.anchoredPosition - Vector2.right * 10;
        selectBar.sizeDelta = thisRect.sizeDelta + Vector2.right * 30;
    }
}
