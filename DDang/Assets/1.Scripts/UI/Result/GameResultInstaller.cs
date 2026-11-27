using Rules;
using UnityEngine;

namespace UI
{
    // 결과 씬: Presenter를 만들고 씬을 나갈 때 정리 (GameResultView와 같은 오브젝트에 붙임)
    public class GameResultInstaller : MonoBehaviour
    {
        [SerializeField] private GameResultView view;               // 비우면 같은 오브젝트에서 찾음

        private GameResultPresenter presenter;

        private void Start()
        {
            if (view == null) view = GetComponent<GameResultView>();
            if (view != null && RoundManager.HasInstance) presenter = new GameResultPresenter(RoundManager.Instance, view);
        }

        private void OnDestroy() => presenter?.Dispose();
    }
}
