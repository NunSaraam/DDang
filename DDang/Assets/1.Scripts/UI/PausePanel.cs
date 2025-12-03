using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    public Image bgmFill;

    public Button bgmPlus;
    public Button bgmMinus;

    private float step = 0.1f;

    private void Awake()
    {
        bgmPlus.onClick.AddListener(() => ChangeBGM(+step));
        bgmMinus.onClick.AddListener(() => ChangeBGM(-step));
    }

    public void RefreshUI()
    {
        bgmFill.fillAmount = SoundManager.Instance.GetBGMVolume();
    }

    void ChangeBGM(float amount)
    {
        float newVol = Mathf.Clamp01(SoundManager.Instance.GetBGMVolume() + amount);
        SoundManager.Instance.SetBGMVolume(newVol);
        bgmFill.fillAmount = newVol;
    }
}
