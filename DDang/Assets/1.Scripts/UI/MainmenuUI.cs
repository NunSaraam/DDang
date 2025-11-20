using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainmenuUI : MonoBehaviour
{
    public RectTransform selectBar;

    public void SelectButton(RectTransform newTransform)
    {
        selectBar.gameObject.SetActive(true);
        selectBar.anchoredPosition = newTransform.anchoredPosition - Vector2.right * 10;
        selectBar.sizeDelta = newTransform.sizeDelta + Vector2.right * 30;

    }
}
