using System.Collections;
using UnityEngine;

namespace Rules
{
    [RequireComponent(typeof(RoundManager))]
    public class RoundBridge : MonoBehaviour
    {
        private RoundManager round;

        private void Awake() => round = GetComponent<RoundManager>();

        // 라운드 HUD(카운트다운·시간·승리 아이콘)는 UI.RoundHudPresenter가 담당. 여기는 퀘스트·보상 같은 게임 로직만
        private void OnEnable()
        {
            round.PhaseChanged += OnPhaseChanged;
            round.QuestTimeReached += OnQuestTime;
            round.RoundEnded += OnRoundEnded;
        }

        private void OnDisable()
        {
            round.PhaseChanged -= OnPhaseChanged;
            round.QuestTimeReached -= OnQuestTime;
            round.RoundEnded -= OnRoundEnded;
        }

        private void OnPhaseChanged(RoundPhase phase)
        {
            if (phase == RoundPhase.ExtraTime) StartReverseQuest();
        }

        // 연장전 시작 시 뒤지는 쪽에게 역전 퀘스트
        private void StartReverseQuest()
        {
            if (QuestManager.Instance == null) return;

            int p1 = round.GetTiles(PlayerType.Player1);
            int p2 = round.GetTiles(PlayerType.Player2);
            if (p1 == p2) return;                               // 동점이면 역전할 사람이 없음

            QuestManager.Instance.ActivateQuest(QuestType.Reverse);
            QuestManager.Instance.OnExtraTImeStarted(p1, p2);
        }

        private void OnQuestTime() => QuestManager.Instance.ActivateQuest(QuestType.StunChain);


        private void OnRoundEnded(RoundResult result)
        {
            QuestManager.Instance?.OnRoundEnd(result.P1Tiles, result.P2Tiles, result.Winner);
            QuestManager.Instance?.ResetAllQuest();         // 미완료 퀘스트 정리 -> 다음 라운드에 새로 제시
        }
    }
}
