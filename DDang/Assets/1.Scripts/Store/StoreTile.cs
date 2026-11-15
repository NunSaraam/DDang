using UnityEngine;
using UnityEngine.UI;

namespace Store
{
    public class StoreTile : MonoBehaviour
    {
        [SerializeField] private Image tileImage;

        public PlayerType Owner { get; private set; }
        public StoreTileType Type { get; private set; }

        public void Setup(PlayerType owner, StoreTileType type, Sprite icon)
        {
            Owner = owner;
            Type = type;
            name = $"{owner}_{type}Tile";
            if (tileImage != null && icon != null) tileImage.sprite = icon;
        }

        // 타일 위에서 공격키
        public void Interact(PlayerMovement player)
        {
            if (StoreManager.Current != null) StoreManager.Current.Interact(player, this);
        }
    }
}
