using UnityEngine;

namespace UI
{
    // 로비 씬: Presenter를 만들고 씬을 나갈 때 정리 (LobbyView와 같은 오브젝트에 붙임)
    public class LobbyInstaller : MonoBehaviour
    {
        [SerializeField] private LobbyView view;                    // 비우면 같은 오브젝트에서 찾음

        private LobbyPresenter presenter;

        private void Start()
        {
            if (view == null) view = GetComponent<LobbyView>();
            if (view != null) presenter = new LobbyPresenter(view, GlobalUiInstaller.Instance);
        }

        private void OnDestroy() => presenter?.Dispose();
    }
}
