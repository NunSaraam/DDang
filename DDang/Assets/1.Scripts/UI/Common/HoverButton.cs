using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    // 마우스를 올리면(또는 키보드로 선택하면) 이미지를 바꾸고 살짝 커짐. 버튼마다 붙임
    [RequireComponent(typeof(Image))]
    public class HoverButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Sprite hoverSprite;                // 비우면 이미지는 그대로, 크기만 바뀜
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float time = 0.12f;

        private Image image;
        private Sprite normalSprite;

        private void Awake()
        {
            image = GetComponent<Image>();
            normalSprite = image.sprite;
        }

        public void OnPointerEnter(PointerEventData e) => SetHover(true);
        public void OnPointerExit(PointerEventData e) => SetHover(false);
        public void OnSelect(BaseEventData e) => SetHover(true);
        public void OnDeselect(BaseEventData e) => SetHover(false);

        private void SetHover(bool on)
        {
            if (hoverSprite != null) image.sprite = on ? hoverSprite : normalSprite;
            transform.DOKill();
            transform.DOScale(on ? hoverScale : 1f, time).SetEase(Ease.OutBack).SetUpdate(true);   // 일시정지 중에도
        }

        // 페이지가 넘어가거나 창이 닫히면 원래대로
        private void OnDisable()
        {
            transform.DOKill();
            transform.localScale = Vector3.one;
            if (image != null) image.sprite = normalSprite;
        }
    }
}
