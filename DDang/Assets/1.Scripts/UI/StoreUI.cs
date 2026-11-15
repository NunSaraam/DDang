using TMPro;
using UnityEngine;

namespace Store
{
    // 상점 시간과 타일 안내 문구
    public class StoreUI : MonoBehaviour
    {
        [SerializeField] private PlayerChatUI player1CchatUI;
        [SerializeField] private PlayerChatUI player2CchatUI;
        public TextMeshProUGUI shopingTimeText;

        private void Start()
        {
            foreach (PlayerChatUI chat in new[] { player1CchatUI, player2CchatUI })
            {
                chat.storeText.enabled = true;
                chat.storeText.text = "";
            }
        }

        public void ShopingTime(float time) => shopingTimeText.text = $"{time:F2}";

        // 타일 위에 올라섰을 때 안내 문구
        public void UpdateStoreUI(PlayerMovement pM)
        {
            if (pM.currentStoreTile == null)
            {
                ExitTileUI(pM);
                return;
            }
            ChatOf(pM.playerType).storeText.text = HintFor(pM.playerType, pM.currentStoreTile);
        }

        public void ExitTileUI(PlayerMovement pM) => ChatOf(pM.playerType).storeText.text = "";

        private static string HintFor(PlayerType p, StoreTile tile)
        {
            if (tile.Owner != p) return "상대 구역";
            StoreManager store = StoreManager.Current;
            return tile.Type switch
            {
                StoreTileType.Augment => "증강 카드 보기",
                StoreTileType.Refresh => $"새로고침 ({store.Settings.refreshCost} 당근)",
                StoreTileType.Ready => store.IsReady(p) ? "준비 완료! (다시 누르면 취소)" : "준비",
                _ => "",
            };
        }

        private PlayerChatUI ChatOf(PlayerType p) => p == PlayerType.Player1 ? player1CchatUI : player2CchatUI;
    }
}
