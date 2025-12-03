using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource bgmSource;       
    private float bgmVolume = 1f;

    public float GetBGMVolume() => bgmVolume;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = value;
        bgmSource.volume = value;
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }
}
