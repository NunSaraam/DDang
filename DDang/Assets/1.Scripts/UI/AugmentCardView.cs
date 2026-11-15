using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Store
{
    // 카드 한 장: 등급 테두리, 아이콘, 이름, 효과, 태그, 가격
    public class AugmentCardView : MonoBehaviour
    {
        [SerializeField] private Image border;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI effectText;
        [SerializeField] private TextMeshProUGUI tagText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private GameObject selectedMark;
        [SerializeField] private CanvasGroup group;            // 구매 완료면 흐리게
        [SerializeField] private float selectedScale = 1.08f;

        public void Show(string title, string effect, string tag, string price, Color color, Sprite sprite, bool sold)
        {
            border.color = color;
            nameText.text = title;
            effectText.text = effect;
            tagText.text = tag;
            priceText.text = price;
            icon.sprite = sprite;
            icon.enabled = sprite != null;
            if (group != null) group.alpha = sold ? 0.4f : 1f;
        }

        public void SetSelected(bool selected)
        {
            if (selectedMark != null) selectedMark.SetActive(selected);
            transform.localScale = Vector3.one * (selected ? selectedScale : 1f);
        }
    }
}
