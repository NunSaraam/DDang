using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

namespace UI
{
    // 설정창: View 이벤트 → 모델 변경 → 바로 적용 → 저장
    // 슬라이더를 끄는 동안 디스크에 계속 쓰지 않도록 볼륨은 0.5초 모아서 저장, 창이 닫히면 바로 저장
    public class SettingsPresenter : IDisposable
    {
        private const float VolumeSaveDelay = 0.5f;

        private readonly SettingsModel model;
        private readonly SettingsView view;
        private readonly AudioMixer mixer;
        private Tween pendingSave;

        public SettingsPresenter(SettingsModel model, SettingsView view, AudioMixer mixer)
        {
            this.model = model;
            this.view = view;
            this.mixer = mixer;

            List<Vector2Int> sizes = GameSettings.GetResolutions(out int current);
            if (model.resolutionIndex < 0 || model.resolutionIndex >= sizes.Count) model.resolutionIndex = current;
            view.SetResolutionOptions(sizes.ConvertAll(s => $"{s.x} x {s.y}"));
            view.SetFrameRateOptions(GameSettings.FrameRateLabels);
            view.ShowDisplay(model.resolutionIndex, model.frameRateIndex, model.isFullscreen);
            view.ShowVolumes(model.masterVolume, model.bgmVolume, model.sfxVolume);

            view.ResolutionChanged += OnResolutionChanged;
            view.FrameRateChanged += OnFrameRateChanged;
            view.FullscreenChanged += OnFullscreenChanged;
            view.MasterVolumeChanged += OnMasterChanged;
            view.BGMVolumeChanged += OnBGMChanged;
            view.SFXVolumeChanged += OnSFXChanged;
            view.Hidden += FlushSave;
        }

        public void Dispose()
        {
            FlushSave();
            view.ResolutionChanged -= OnResolutionChanged;
            view.FrameRateChanged -= OnFrameRateChanged;
            view.FullscreenChanged -= OnFullscreenChanged;
            view.MasterVolumeChanged -= OnMasterChanged;
            view.BGMVolumeChanged -= OnBGMChanged;
            view.SFXVolumeChanged -= OnSFXChanged;
            view.Hidden -= FlushSave;
        }

        // ── 화면: 바로 적용 + 바로 저장 ──
        private void OnResolutionChanged(int index)
        {
            model.resolutionIndex = index;
            GameSettings.ApplyDisplay(model);
            SaveNow();
        }

        private void OnFullscreenChanged(bool on)
        {
            model.isFullscreen = on;
            GameSettings.ApplyDisplay(model);
            SaveNow();
        }

        private void OnFrameRateChanged(int index)
        {
            model.frameRateIndex = index;
            GameSettings.ApplyFrameRate(index);
            SaveNow();
        }

        // ── 소리: 바로 적용 + 모아서 저장 ──
        private void OnMasterChanged(float v) { model.masterVolume = v; ApplyVolume(GameSettings.MasterParam, v); }
        private void OnBGMChanged(float v) { model.bgmVolume = v; ApplyVolume(GameSettings.BGMParam, v); }
        private void OnSFXChanged(float v) { model.sfxVolume = v; ApplyVolume(GameSettings.SFXParam, v); }

        private void ApplyVolume(string param, float volume)
        {
            view.ShowVolumes(model.masterVolume, model.bgmVolume, model.sfxVolume);    // % 글자 갱신
            GameSettings.ApplyVolume(mixer, param, volume);
            ScheduleSave();
        }

        // ── 저장 ──
        private void ScheduleSave()
        {
            pendingSave?.Kill();
            pendingSave = DOVirtual.DelayedCall(VolumeSaveDelay, SaveNow).SetUpdate(true);   // 일시정지 중에도 동작
        }

        public void FlushSave()
        {
            if (pendingSave != null) SaveNow();
        }

        private void SaveNow()
        {
            pendingSave?.Kill();
            pendingSave = null;
            model.Save();
        }
    }
}
