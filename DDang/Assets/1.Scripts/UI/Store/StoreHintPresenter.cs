using System;
using Store;

namespace UI
{
    // 머리 위 타일 안내: 올라선 타일·상점 상태가 바뀔 때마다 문구를 정해 말풍선에
    public class StoreHintPresenter : IDisposable
    {
        private readonly StoreManager store;
        private readonly PlayerMovement player;
        private readonly PlayerBubbleView bubble;

        public StoreHintPresenter(StoreManager store, PlayerMovement player, PlayerBubbleView bubble)
        {
            this.store = store;
            this.player = player;
            this.bubble = bubble;

            player.StoreTileChanged += OnTileChanged;
            store.StateChanged += Refresh;
            Refresh();
        }

        public void Dispose()
        {
            player.StoreTileChanged -= OnTileChanged;
            store.StateChanged -= Refresh;
        }

        private void OnTileChanged(StoreTile tile) => Refresh();

        private void Refresh()
        {
            StoreTile tile = player.currentStoreTile;
            bubble.ShowStoreHint(tile == null ? "" : HintFor(player.playerType, tile));
        }

        private string HintFor(PlayerType p, StoreTile tile)
        {
            if (tile.Owner != p) return "상대 구역";
            return tile.Type switch
            {
                StoreTileType.Augment => "증강 카드 보기",
                StoreTileType.Refresh => $"새로고침\n<size=70%>{store.Settings.refreshCost} 당근</size>",
                StoreTileType.Ready => store.IsReady(p) ? "준비 완료!\n<size=70%>다시 누르면 취소</size>" : "준비",
                _ => "",
            };
        }
    }
}
