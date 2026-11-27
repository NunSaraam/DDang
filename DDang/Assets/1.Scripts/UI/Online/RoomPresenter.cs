using System;
using Fusion;
using Online;

namespace UI
{
    // 대기실: 방 상태(닉네임·준비)가 바뀔 때 준비 버튼 글자, 안내 문구, 말풍선 2개를 갱신
    public class RoomPresenter : IDisposable
    {
        private readonly OnlineRoom room;
        private readonly RoomView view;
        private readonly ReadyBubbleView[] bubbles;

        public RoomPresenter(OnlineRoom room, RoomView view, ReadyBubbleView[] bubbles)
        {
            this.room = room;
            this.view = view;
            this.bubbles = bubbles;

            room.Changed += Refresh;
            view.ReadyClicked += room.ToggleReady;
            view.LeaveClicked += OnLeave;
            Refresh();
        }

        public void Dispose()
        {
            room.Changed -= Refresh;
            view.ReadyClicked -= room.ToggleReady;
            view.LeaveClicked -= OnLeave;
        }

        private void Refresh()
        {
            if (room == null || !room.Object || !room.Object.IsValid) return;

            view.SetReady(room.IsReady(room.Runner.LocalPlayer));

            PlayerType other = OnlineSession.Instance.CurrentSlot() == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1;
            PlayerRef opponent = room.SlotPlayer(other);
            view.SetStatus(opponent == PlayerRef.None
                ? $"방 코드: {OnlineSession.Instance.RoomName} · 상대를 기다리는 중"
                : $"상대: {room.NameOf(opponent)} · 둘 다 준비 완료 시 시작");

            foreach (ReadyBubbleView bubble in bubbles)             // 그 자리에 사람이 없으면 숨김
            {
                if (bubble == null) continue;
                PlayerRef player = room.SlotPlayer(bubble.Slot);
                if (player == PlayerRef.None) bubble.Hide();
                else bubble.Show(room.NameOf(player), room.IsReady(player));
            }
        }

        private static void OnLeave() => OnlineSession.Instance.Leave();
    }
}
