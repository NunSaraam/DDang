using Rules;
using UnityEngine;

namespace UI
{
    // 토끼 프리팹의 말풍선 Canvas에 붙임. 부모의 PlayerMovement와 이 말풍선을 이어 Presenter를 만듦
    public class PlayerBubbleInstaller : MonoBehaviour
    {
        [SerializeField] private PlayerBubbleView view;             // 비우면 같은 오브젝트에서 찾음

        private PlayerBubblePresenter presenter;

        private void Start()
        {
            if (view == null) view = GetComponent<PlayerBubbleView>();
            PlayerMovement player = GetComponentInParent<PlayerMovement>();
            if (view == null || player == null || !RoundManager.HasInstance) return;
            presenter = new PlayerBubblePresenter(RoundManager.Instance, player, view);
        }

        private void OnDestroy() => presenter?.Dispose();
    }
}
