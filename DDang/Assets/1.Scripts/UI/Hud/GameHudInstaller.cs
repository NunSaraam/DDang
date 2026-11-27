using System;
using System.Collections.Generic;
using Animal;
using Rules;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    // 게임 씬 HUD의 Presenter를 한곳에서 만들고 정리 (씬의 조립 담당)
    public class GameHudInstaller : MonoBehaviour
    {
        [FormerlySerializedAs("view")]
        [SerializeField] private RoundHudView roundHud;
        [SerializeField] private RoundStatusView roundStatus;
        [SerializeField] private TileShareGaugeView tileShare;
        [SerializeField] private QuestPanelView questPanel;
        [SerializeField] private AnimalWarningView animalWarning;

        private readonly List<IDisposable> presenters = new();

        private void Awake()
        {
            RoundManager round = RoundManager.Instance;
            if (round != null)
            {
                if (roundHud != null) presenters.Add(new RoundHudPresenter(round, roundHud));
                if (roundStatus != null) presenters.Add(new RoundStatusPresenter(round, roundStatus));
                if (tileShare != null) presenters.Add(new TileShareGaugePresenter(round, tileShare));
            }

            if (questPanel != null && QuestManager.Instance != null)
                presenters.Add(new QuestPresenter(QuestManager.Instance, questPanel));

            if (animalWarning != null && AnimalManager.Instance != null)
                presenters.Add(new AnimalWarningPresenter(AnimalManager.Instance, animalWarning));
        }

        private void OnDestroy()
        {
            foreach (IDisposable p in presenters) p.Dispose();
            presenters.Clear();
        }
    }
}
