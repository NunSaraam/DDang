using UnityEngine;

namespace UI
{
    // 메인 메뉴 씬: Presenter를 만들고 씬을 나갈 때 정리 (MainMenuView와 같은 오브젝트에 붙임)
    public class MainMenuInstaller : MonoBehaviour
    {
        [SerializeField] private MainMenuView view;                 // 비우면 같은 오브젝트에서 찾음

        private MainMenuPresenter presenter;

        private void Start()                                        // 전역 UI가 Awake에서 준비된 뒤
        {
            if (view == null) view = GetComponent<MainMenuView>();
            if (view != null) presenter = new MainMenuPresenter(view, GlobalUiInstaller.Instance);
        }

        private void OnDestroy() => presenter?.Dispose();
    }
}
