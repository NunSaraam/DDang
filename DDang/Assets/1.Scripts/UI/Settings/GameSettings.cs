using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace UI
{
    // 설정을 실제로 적용 (부팅할 때 SoundManager가, 설정창에서는 SettingsPresenter가 같은 코드를 씀)
    public static class GameSettings
    {
        public const string MasterParam = "MasterVolume";           // AudioMixer에서 Expose한 이름
        public const string BGMParam = "BGMVolume";
        public const string SFXParam = "SFXVolume";

        private static readonly int[] FrameRates = { 30, 60, 120, -1 };     // -1 = 무제한
        public static readonly List<string> FrameRateLabels = new() { "30 FPS", "60 FPS", "120 FPS", "무제한" };

        // 모니터가 지원하는 해상도 (주사율만 다른 같은 크기는 하나로). currentIndex = 지금 해상도
        public static List<Vector2Int> GetResolutions(out int currentIndex)
        {
            var list = new List<Vector2Int>();
            currentIndex = 0;
            foreach (Resolution r in Screen.resolutions)
            {
                var size = new Vector2Int(r.width, r.height);
                if (list.Contains(size)) continue;
                list.Add(size);
                if (size.x == Screen.currentResolution.width && size.y == Screen.currentResolution.height)
                    currentIndex = list.Count - 1;
            }
            return list;
        }

        public static void ApplyAll(SettingsModel model, AudioMixer mixer)
        {
            if (model.resolutionIndex >= 0) ApplyDisplay(model);    // 처음 실행(저장값 없음)이면 지금 해상도 그대로
            ApplyFrameRate(model.frameRateIndex);
            ApplyVolume(mixer, MasterParam, model.masterVolume);
            ApplyVolume(mixer, BGMParam, model.bgmVolume);
            ApplyVolume(mixer, SFXParam, model.sfxVolume);
        }

        public static void ApplyDisplay(SettingsModel model)
        {
            List<Vector2Int> sizes = GetResolutions(out int current);
            if (model.resolutionIndex < 0 || model.resolutionIndex >= sizes.Count)
                model.resolutionIndex = current;                    // 모니터가 바뀌어 저장된 해상도가 없으면 지금 해상도

            Vector2Int size = sizes[model.resolutionIndex];
            Screen.SetResolution(size.x, size.y, model.isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }

        public static void ApplyFrameRate(int index)
        {
            QualitySettings.vSyncCount = 0;                         // 수직동기를 꺼야 프레임 제한이 적용됨
            Application.targetFrameRate = FrameRates[Mathf.Clamp(index, 0, FrameRates.Length - 1)];
        }

        // 0~1 → dB (0이면 사실상 무음)
        public static void ApplyVolume(AudioMixer mixer, string param, float volume)
        {
            if (mixer == null) return;
            mixer.SetFloat(param, Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
        }
    }
}
