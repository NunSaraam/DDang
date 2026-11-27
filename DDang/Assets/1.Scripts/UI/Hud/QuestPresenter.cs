using System;

namespace UI
{
    // 퀘스트: QuestManager(모델)의 이벤트 → 퀘스트 패널
    public class QuestPresenter : IDisposable
    {
        private readonly QuestManager quests;
        private readonly QuestPanelView view;

        public QuestPresenter(QuestManager quests, QuestPanelView view)
        {
            this.quests = quests;
            this.view = view;

            quests.QuestStarted += view.AddQuest;
            quests.QuestProgressed += view.SetProgress;
            quests.QuestLinesChanged += view.SetLines;
            quests.QuestFinished += view.Complete;
        }

        public void Dispose()
        {
            quests.QuestStarted -= view.AddQuest;
            quests.QuestProgressed -= view.SetProgress;
            quests.QuestLinesChanged -= view.SetLines;
            quests.QuestFinished -= view.Complete;
        }
    }
}
