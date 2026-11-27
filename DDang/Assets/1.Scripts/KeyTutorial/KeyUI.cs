using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyUI : MonoBehaviour
{
    public Image backhround;
    public TextMeshProUGUI keyText;

    public Color defaultColor = Color.white;
    public Color pressedColor = new Color(.8f, .8f, .8f);

    private bool isPressed = false;

    private void Awake()
    {
        backhround.color = defaultColor;
    }

    public void SetKeyText(string key)
    {
        keyText.text = key;
    }

    // 누르고 있는 동안 눌린 모습 유지 (KeyUIController가 매 프레임 호출)
    public void SetHeld(bool held)
    {
        if (held == isPressed) return;                  // 바뀔 때만 적용
        isPressed = held;
        backhround.color = held ? pressedColor : defaultColor;
        transform.localScale = Vector3.one * (held ? .9f : 1f);
    }
}
