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

    public void OnKeyPressed()
    {
        if (!isPressed)
        {
            StartCoroutine(PressedKey());
        }
    }

    IEnumerator PressedKey()
    {
        isPressed = true;

        backhround.color = pressedColor;
        transform.localScale = Vector3.one * .9f;

        yield return new WaitForSeconds(.1f);

        backhround.color = defaultColor;
        transform.localScale = Vector3.one;

        isPressed = false;
    }
}
