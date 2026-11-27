using System;
using Online;
using UnityEngine;

namespace UI
{
    // 로비: 닉네임 불러오기·저장, 빈칸 검사, 접속, 실패 안내, 뒤로(버튼·ESC) → 메인 메뉴
    public class LobbyPresenter : IDisposable
    {
        private const string NicknameKey = "Online_Nickname";

        private readonly LobbyView view;
        private readonly GlobalUiInstaller globalUi;
        private bool connecting;

        public LobbyPresenter(LobbyView view, GlobalUiInstaller globalUi)
        {
            this.view = view;
            this.globalUi = globalUi;

            view.SetNickname(PlayerPrefs.GetString(NicknameKey, ""));   // 지난번 닉네임
            view.SetMessage(OnlineSession.LastMessage);                 // 연결이 끊겨 돌아왔으면 안내
            OnlineSession.LastMessage = null;

            view.JoinClicked += OnJoin;
            view.BackClicked += OnBack;
            if (globalUi != null) globalUi.BackUnhandled += OnBack;
        }

        public void Dispose()
        {
            view.JoinClicked -= OnJoin;
            view.BackClicked -= OnBack;
            if (globalUi != null) globalUi.BackUnhandled -= OnBack;
        }

        private async void OnJoin(string nickname, string room)
        {
            if (connecting) return;
            if (nickname.Length == 0) { view.SetMessage("닉네임을 입력해 주세요"); return; }
            if (room.Length == 0) { view.SetMessage("방 코드를 입력해 주세요"); return; }

            PlayerPrefs.SetString(NicknameKey, nickname);
            connecting = true;
            view.SetInteractable(false);
            view.SetMessage("접속 중...");

            string error = await OnlineSession.Create().Connect(room, nickname);
            connecting = false;
            if (error == null) return;                              // 성공하면 대기실로 넘어감 (이 화면은 사라짐)

            view.SetMessage(error);
            view.SetInteractable(true);
        }

        private void OnBack()
        {
            if (connecting) return;
            SceneLoadManager.Instance.LoadScene("Mainmenu");
        }
    }
}
