using System.Collections;
using Animal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // 27초 : 동물 아이콘과 이름을 3초 동안 표시
    public class AnimalWarningUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private float showTime = 3f;

        private AnimalManager manager;

        private void Start()
        {
            panel.SetActive(false);
            manager = AnimalManager.Instance;
            if (manager == null) return;
            manager.Warned += Show;
            manager.Spawned += OnSpawned;
        }

        private void OnDestroy()
        {
            if (manager == null) return;
            manager.Warned -= Show;
            manager.Spawned -= OnSpawned;
        }

        private void Show(AnimalData data)
        {
            icon.sprite = data.icon;
            nameText.text = $"{data.displayName} 등장!";
            StopAllCoroutines();
            StartCoroutine(ShowRoutine());
        }

        private void OnSpawned(AnimalBase _) => panel.SetActive(false);

        private IEnumerator ShowRoutine()
        {
            panel.SetActive(true);
            yield return new WaitForSeconds(showTime);
            panel.SetActive(false);
        }
    }
}
