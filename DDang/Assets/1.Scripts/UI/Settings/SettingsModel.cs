using UnityEngine;

namespace UI
{
    // 설정 값과 저장·불러오기 (PlayerPrefs)
    public class SettingsModel
    {
        public float masterVolume;
        public float bgmVolume;
        public float sfxVolume;

        public int resolutionIndex;         // 해상도 목록 번호 (-1 = 저장된 값 없음)
        public int frameRateIndex = 1;      // 프레임 제한 (0: 30, 1: 60, 2: 120, 3: 무제한)
        public bool isFullscreen;

        public void Load()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

            resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", -1);
            frameRateIndex = PlayerPrefs.GetInt("FrameRateIndex", 1);
            isFullscreen = PlayerPrefs.GetInt("IsFullscreen", Screen.fullScreen ? 1 : 0) == 1;
        }

        public void Save()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);

            PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
            PlayerPrefs.SetInt("FrameRateIndex", frameRateIndex);
            PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);

            PlayerPrefs.Save();
        }
    }
}
