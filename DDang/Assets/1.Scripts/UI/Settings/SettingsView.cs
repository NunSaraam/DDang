using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // 설정창 (보여 주기만). 값이 바뀌면 이벤트만 내고, 적용·저장은 SettingsPresenter
    public class SettingsView : UIPanel
    {
        [Header("화면")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown frameRateDropdown;
        [SerializeField] private Toggle fullscreenToggle;

        [Header("소리")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeText;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private TextMeshProUGUI bgmVolumeText;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI sfxVolumeText;

        [Header("기타")]
        [SerializeField] private Button closeButton;

        public event Action<int> ResolutionChanged;
        public event Action<int> FrameRateChanged;
        public event Action<bool> FullscreenChanged;
        public event Action<float> MasterVolumeChanged;
        public event Action<float> BGMVolumeChanged;
        public event Action<float> SFXVolumeChanged;
        public event Action Hidden;                                 // 창이 닫힘 (모아 둔 저장을 바로 실행)

        private bool updating;                                      // 코드로 값을 넣는 중에는 이벤트를 내지 않음

        private void Awake()
        {
            resolutionDropdown.onValueChanged.AddListener(v => { if (!updating) ResolutionChanged?.Invoke(v); });
            frameRateDropdown.onValueChanged.AddListener(v => { if (!updating) FrameRateChanged?.Invoke(v); });
            fullscreenToggle.onValueChanged.AddListener(v => { if (!updating) FullscreenChanged?.Invoke(v); });

            masterVolumeSlider.onValueChanged.AddListener(v => { if (!updating) MasterVolumeChanged?.Invoke(v); });
            bgmVolumeSlider.onValueChanged.AddListener(v => { if (!updating) BGMVolumeChanged?.Invoke(v); });
            sfxVolumeSlider.onValueChanged.AddListener(v => { if (!updating) SFXVolumeChanged?.Invoke(v); });

            closeButton.onClick.AddListener(RequestClose);           // 닫는 방법은 창을 연 쪽(Presenter)이 정함
        }

        public override void Hide()
        {
            base.Hide();
            Hidden?.Invoke();
        }

        public void SetResolutionOptions(List<string> options)
        {
            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
        }

        public void SetFrameRateOptions(List<string> options)
        {
            frameRateDropdown.ClearOptions();
            frameRateDropdown.AddOptions(options);
        }

        public void ShowDisplay(int resolutionIndex, int frameRateIndex, bool fullscreen)
        {
            updating = true;
            if (resolutionIndex >= 0) resolutionDropdown.SetValueWithoutNotify(resolutionIndex);
            frameRateDropdown.SetValueWithoutNotify(frameRateIndex);
            fullscreenToggle.SetIsOnWithoutNotify(fullscreen);
            updating = false;
        }

        public void ShowVolumes(float master, float bgm, float sfx)
        {
            updating = true;
            masterVolumeSlider.SetValueWithoutNotify(master);
            bgmVolumeSlider.SetValueWithoutNotify(bgm);
            sfxVolumeSlider.SetValueWithoutNotify(sfx);
            masterVolumeText.text = $"{Mathf.RoundToInt(master * 100)}%";
            bgmVolumeText.text = $"{Mathf.RoundToInt(bgm * 100)}%";
            sfxVolumeText.text = $"{Mathf.RoundToInt(sfx * 100)}%";
            updating = false;
        }
    }
}
