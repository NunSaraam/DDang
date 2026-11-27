using System;
using DG.Tweening;
using Online;
using Rules;

namespace UI
{
    // 결과 화면: 매치 승자를 보여 주고, 잠시 뒤 온라인은 대기실·로컬은 메인 메뉴로
    public class GameResultPresenter : IDisposable
    {
        private const float StayTime = 5f;

        private readonly Tween leaveCall;

        public GameResultPresenter(RoundManager round, GameResultView view)
        {
            if (round.Match == null) return;                        // 결과 화면을 바로 열었을 때 (테스트)
            view.PlayResult(round.Match.Winner);
            leaveCall = DOVirtual.DelayedCall(StayTime, Leave);
        }

        public void Dispose() => leaveCall?.Kill();

        private static void Leave()
        {
            if (OnlineSession.IsOnline)                             // 같은 방 대기실로 (방장이 이동, 상대는 따라옴)
            {
                OnlineSession.Instance.BackToWaitingRoom();
                return;
            }
            GameSession.Instance.needFullReset = true;              // 다음 게임은 처음부터
            SceneLoadManager.Instance.LoadScene("Mainmenu");
        }
    }
}
