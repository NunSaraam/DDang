using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    // 씬이 바뀌어도 남는 전역 UI (UIManager 프리팹 루트에 붙임): 일시정지 메뉴 + 설정창
    public class GlobalUiInstaller : MonoBehaviour
    {
        public static GlobalUiInstaller Instance { get; private set; }

        [SerializeField] private PauseMenuView pauseMenu;
        [SerializeField] private SettingsView settings;
        [SerializeField] private string[] pauseScenes = { "GameScene", "Store" };   // ESC 일시정지를 쓰는 씬

        private SettingsPresenter settingsPresenter;
        private PauseMenuPresenter pausePresenter;

        // ESC를 전역 UI(설정창 닫기·일시정지)가 먼저 처리하고, 처리하지 않았을 때만 씬 화면에 넘김
        public event Action BackUnhandled;
        public bool IsSettingsOpen => settings != null && settings.IsOpen;

        private void Awake()
        {
            if (Instance != null && Instance != this)               // 메인 메뉴를 다시 열어도 하나만
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // GameManager·SoundManager가 Awake에서 준비된 뒤 연결
        private void Start()
        {
            if (Instance != this) return;

            SoundManager sound = SoundManager.Instance;
            if (settings != null && sound != null)
                settingsPresenter = new SettingsPresenter(sound.Settings, settings, sound.Mixer);

            if (GameManager.Instance != null && pauseMenu != null)
            {
                pausePresenter = new PauseMenuPresenter(GameManager.Instance, pauseMenu, settings, IsPauseScene);
                pausePresenter.BackUnhandled += () => BackUnhandled?.Invoke();
            }
        }

        // 메인 메뉴의 설정 버튼에서도 같은 설정창을 엶 (일시정지 없이)
        public void OpenSettings() => pausePresenter?.OpenSettingsOnly();

        private bool IsPauseScene() => Array.IndexOf(pauseScenes, SceneManager.GetActiveScene().name) >= 0;

        private void OnDestroy()
        {
            if (Instance != this) return;
            settingsPresenter?.Dispose();                           // 설정은 Dispose에서 바로 저장
            pausePresenter?.Dispose();
            Instance = null;
        }
    }
}
