using System;
using Online;
using Rules;
using UnityEngine.SceneManagement;

namespace UI
{
    // 일시정지 메뉴: ESC(뒤로)·버튼 → 일시정지/계속하기, 설정창 열고 닫기, 나가기
    // ESC 순서: 설정창이 열려 있으면 닫기 → 일시정지 중이면 계속하기 → 일시정지 씬이면 일시정지
    public class PauseMenuPresenter : IDisposable
    {
        private readonly GameManager game;
        private readonly PauseMenuView menu;
        private readonly SettingsView settings;
        private readonly Func<bool> canPauseHere;                   // 지금 씬에서 일시정지를 쓰는지 (게임·상점)

        public event Action BackUnhandled;                          // 전역 UI가 처리하지 않은 ESC → 씬의 화면(메인 메뉴 페이지 뒤로 등)

        public PauseMenuPresenter(GameManager game, PauseMenuView menu, SettingsView settings, Func<bool> canPauseHere)
        {
            this.game = game;
            this.menu = menu;
            this.settings = settings;
            this.canPauseHere = canPauseHere;

            game.BackPressed += OnBack;
            game.PauseChanged += OnPauseChanged;
            menu.ResumeClicked += game.ResumeGame;
            menu.SettingsClicked += OpenSettings;
            menu.LeaveClicked += Leave;
            if (settings != null) settings.CloseRequested += CloseSettings;
            SceneManager.sceneLoaded += OnSceneLoaded;

            HideAll();
        }

        public void Dispose()
        {
            game.BackPressed -= OnBack;
            game.PauseChanged -= OnPauseChanged;
            menu.ResumeClicked -= game.ResumeGame;
            menu.SettingsClicked -= OpenSettings;
            menu.LeaveClicked -= Leave;
            if (settings != null) settings.CloseRequested -= CloseSettings;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // 메인 메뉴 등에서 일시정지 없이 설정창만
        public void OpenSettingsOnly()
        {
            if (settings != null) settings.Show();
        }

        private void OnBack()
        {
            if (settings != null && settings.IsOpen) CloseSettings();
            else if (game.IsPaused) game.ResumeGame();
            else if (canPauseHere()) game.PauseGame();
            else BackUnhandled?.Invoke();
        }

        private void OnPauseChanged(bool paused)
        {
            if (paused)
            {
                menu.SetLeaveLabel(OnlineSession.IsOnline ? "방 나가기" : "메인 메뉴로");   // 열 때마다 (온라인 여부가 바뀔 수 있음)
                menu.Show();
                return;
            }
            if (settings != null && settings.IsOpen) settings.Hide();
            if (menu.IsOpen) menu.Hide();
        }

        private void OpenSettings()
        {
            if (settings == null) return;
            menu.Hide();
            settings.Show();
        }

        private void CloseSettings()
        {
            settings.Hide();
            if (game.IsPaused) menu.Show();                         // 일시정지 중이었으면 메뉴로 돌아감
        }

        // 씬이 바뀌면 열려 있던 창을 바로 닫음 (GameManager가 시간·상태는 풀어 줌)
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => HideAll();

        private void HideAll()
        {
            menu.HideImmediate();
            if (settings != null) settings.HideImmediate();
        }

        // 온라인: 방 나가기 / 로컬: 진행 중인 매치를 정리하고 메인 메뉴로
        private void Leave()
        {
            game.ResumeGame();                                      // 시간 정지·BGM 일시정지 해제
            if (OnlineSession.IsOnline)
            {
                OnlineSession.Instance.Leave();
                return;
            }
            if (RoundManager.HasInstance) RoundManager.Instance.StopMatch();
            if (GameSession.Instance != null) GameSession.Instance.needFullReset = true;
            SceneLoadManager.Instance.LoadScene("Mainmenu");
        }
    }
}
