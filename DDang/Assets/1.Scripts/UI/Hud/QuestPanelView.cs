using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    // 화면 위쪽 퀘스트 패널 (보여 주기만). 퀘스트가 생기면 위에서 내려오고, 모두 끝나면 올라감
    public class QuestPanelView : MonoBehaviour
    {
        [SerializeField] private RectTransform questPanel;
        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject questPrefab;            // 한 줄 (Quest.cs)
        [SerializeField] private float slideDuration = .4f;
        [SerializeField] private float hideDelay = 1.5f;            // 마지막 퀘스트가 끝난 뒤 패널이 올라가기까지

        private static readonly Vector2 ShownPos = new(0, 2);
        private static readonly Vector2 HiddenPos = new(0, 200);

        private readonly Dictionary<QuestSO, Quest> items = new();

        private void Awake() => questPanel.gameObject.SetActive(false);

        public void AddQuest(QuestSO qs)
        {
            if (items.ContainsKey(qs)) return;

            Quest item = Instantiate(questPrefab, content).GetComponent<Quest>();
            item.SetQuestInfo(qs.questName, qs.Goal);               // 한 줄에 들어가는 짧은 이름
            items.Add(qs, item);

            questPanel.DOKill();
            if (!questPanel.gameObject.activeSelf)                  // 숨어 있으면 위에서 내려옴
            {
                questPanel.gameObject.SetActive(true);
                questPanel.anchoredPosition = HiddenPos;
            }
            questPanel.DOAnchorPos(ShownPos, slideDuration);
        }

        public void SetProgress(QuestSO qs, int p1, int p2)
        {
            if (items.TryGetValue(qs, out Quest item)) item.UpdateProgress(p1, p2);
        }

        public void SetLines(QuestSO qs, string p1, string p2)
        {
            if (items.TryGetValue(qs, out Quest item)) item.SetLines(p1, p2);
        }

        // 완료(player가 None이면 정리만): 결과를 잠깐 보여 준 뒤 줄 삭제
        public void Complete(QuestSO qs, PlayerType player, int reward)
        {
            if (items.TryGetValue(qs, out Quest item))
            {
                item.CompleteQuest(player, reward);
                Destroy(item.gameObject, 1.5f);
                items.Remove(qs);
            }
            HideIfEmpty(hideDelay);
        }

        private void HideIfEmpty(float delay)
        {
            if (items.Count > 0 || !questPanel.gameObject.activeSelf) return;
            questPanel.DOKill();
            questPanel.DOAnchorPos(HiddenPos, slideDuration).SetDelay(delay)
                      .OnComplete(() => questPanel.gameObject.SetActive(false));
        }

        private void OnDestroy()
        {
            if (questPanel != null) questPanel.DOKill();
        }
    }
}
