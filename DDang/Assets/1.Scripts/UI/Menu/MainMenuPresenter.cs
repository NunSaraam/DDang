using System;
using UnityEngine;

namespace UI
{
    // 메인 메뉴: 페이지 이동, 씬 시작, 설정창, 종료
    public class MainMenuPresenter : IDisposable
    {
        private const string GameScene = "GameScene";
        private const string LobbyScene = "Lobby";

        private readonly MainMenuView view;
        private readonly GlobalUiInstaller globalUi;                // 설정창·ESC (없으면 설정·ESC만 안 됨)

        public MainMenuPresenter(MainMenuView view, GlobalUiInstaller globalUi)
        {
            this.view = view;
            this.globalUi = globalUi;

            view.PlayClicked += OnPlay;
            view.BackClicked += OnBack;
            view.StartClicked += OnStart;
            view.OnlineClicked += OnOnline;
            view.SettingsClicked += OnSettings;
            view.QuitClicked += OnQuit;
            if (globalUi != null) globalUi.BackUnhandled += OnBack;   // ESC: 설정창이 닫혀 있을 때만 여기로 옴
        }

        public void Dispose()
        {
            view.PlayClicked -= OnPlay;
            view.BackClicked -= OnBack;
            view.StartClicked -= OnStart;
            view.OnlineClicked -= OnOnline;
            view.SettingsClicked -= OnSettings;
            view.QuitClicked -= OnQuit;
            if (globalUi != null) globalUi.BackUnhandled -= OnBack;
        }

        private void OnPlay() => view.ShowPage(MainMenuView.Page.Tutorial, forward: true);

        private void OnBack()
        {
            if (view.Current == MainMenuView.Page.Tutorial) view.ShowPage(MainMenuView.Page.Main, forward: false);
        }

        private void OnStart() => LoadScene(GameScene);
        private void OnOnline() => LoadScene(LobbyScene);
        private void OnSettings() => globalUi?.OpenSettings();

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;            // 에디터에서도 종료 확인 가능
#else
            Application.Quit();
#endif
        }

        private void LoadScene(string scene)
        {
            view.SetInteractable(false);                            // 연타 방지
            SceneLoadManager.Instance.LoadSceneWithLoading(scene);
        }
    }
}
