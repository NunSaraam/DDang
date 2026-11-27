using System;
using Store;

namespace UI
{
    // 상점 시간: StoreManager의 남은 시간 → View
    public class StoreHudPresenter : IDisposable
    {
        private readonly StoreManager store;
        private readonly StoreHudView view;

        public StoreHudPresenter(StoreManager store, StoreHudView view)
        {
            this.store = store;
            this.view = view;
            store.TimeChanged += view.SetTime;
        }

        public void Dispose() => store.TimeChanged -= view.SetTime;
    }
}
