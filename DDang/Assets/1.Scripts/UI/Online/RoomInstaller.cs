using Online;
using UnityEngine;

namespace UI
{
    // 대기실 씬: 대기실 상태(OnlineRoom)가 생기면 Presenter를 만듦 (RoomView와 같은 오브젝트에 붙임)
    // OnlineRoom은 씬이 열린 뒤 네트워크로 생기므로, 이미 있으면 바로·없으면 Opened를 기다림
    public class RoomInstaller : MonoBehaviour
    {
        [SerializeField] private RoomView view;                     // 비우면 같은 오브젝트에서 찾음
        [SerializeField] private ReadyBubbleView p1Bubble;
        [SerializeField] private ReadyBubbleView p2Bubble;

        private RoomPresenter presenter;

        private void Start()
        {
            if (view == null) view = GetComponent<RoomView>();
            if (OnlineRoom.Instance != null) Build(OnlineRoom.Instance);
            else OnlineRoom.Opened += Build;
        }

        private void Build(OnlineRoom room)
        {
            OnlineRoom.Opened -= Build;
            if (presenter != null || view == null) return;
            presenter = new RoomPresenter(room, view, new[] { p1Bubble, p2Bubble });
        }

        private void OnDestroy()
        {
            OnlineRoom.Opened -= Build;
            presenter?.Dispose();
        }
    }
}
