using System;
using System.Collections.Generic;
using Store;
using UnityEngine;

namespace UI
{
    // 상점 씬 UI의 Presenter를 한곳에서 만들고 정리 (씬의 조립 담당)
    // StoreManager가 Start에서 디버프 창 이벤트를 보내므로, 그보다 먼저 Awake에서 구독
    public class StoreUiInstaller : MonoBehaviour
    {
        [SerializeField] private StoreManager store;                // StoreManager.Current는 Awake 순서에 따라 아직 없을 수 있어 직접 연결
        [SerializeField] private StoreHudView hud;
        [SerializeField] private AugmentCardPanelView[] cardPanels; // 1P, 2P 카드 창

        private readonly List<IDisposable> presenters = new();

        private void Awake()
        {
            if (store == null) return;

            if (hud != null) presenters.Add(new StoreHudPresenter(store, hud));

            PlayerMovement[] players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
            foreach (PlayerMovement p in players)                   // 토끼마다 머리 위 타일 안내
            {
                PlayerBubbleView bubble = p.GetComponentInChildren<PlayerBubbleView>(true);
                if (bubble != null) presenters.Add(new StoreHintPresenter(store, p, bubble));
            }

            foreach (AugmentCardPanelView panel in cardPanels)      // 카드 창마다 (주인 토끼와 연결)
            {
                if (panel == null) continue;
                PlayerMovement owner = Array.Find(players, p => p.playerType == panel.Owner);
                presenters.Add(new AugmentCardPresenter(store, owner, panel));
            }
        }

        private void OnDestroy()
        {
            foreach (IDisposable p in presenters) p.Dispose();
            presenters.Clear();
        }
    }
}
