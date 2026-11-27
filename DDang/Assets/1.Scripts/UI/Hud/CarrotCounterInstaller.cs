using System.Collections.Generic;
using Store;
using UnityEngine;

namespace UI
{
    // Carrot 프리팹(게임·상점 씬 공통) 안의 당근 View마다 Presenter를 만들고 정리
    public class CarrotCounterInstaller : MonoBehaviour
    {
        private readonly List<CarrotCounterPresenter> presenters = new();

        private void Start()                                        // CoinManager가 먼저 준비되도록 Start에서
        {
            if (!CoinManager.HasInstance) return;
            foreach (CarrotCounterView view in GetComponentsInChildren<CarrotCounterView>(true))
                presenters.Add(new CarrotCounterPresenter(CoinManager.Instance, view));
        }

        private void OnDestroy()
        {
            foreach (CarrotCounterPresenter p in presenters) p.Dispose();
            presenters.Clear();
        }
    }
}
