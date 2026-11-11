using Rules;
using TMPro;
using UnityEngine;

namespace UI
{
    // 듀스 / 결투 / 추가 시간 / 서든데스 / 골든골 문구
    public class RoundStatusUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statusText;

        private void OnEnable()
        {
            RoundManager round = RoundManager.Instance;
            if (round == null) return;

            round.PhaseChanged += Refresh;
            Refresh(round.Phase);
        }

        private void OnDisable()
        {
            if (RoundManager.HasInstance) RoundManager.Instance.PhaseChanged -= Refresh;
        }

        private void Refresh(RoundPhase phase)
        {
            string text = GetStatusText(RoundManager.Instance, phase);
            statusText.gameObject.SetActive(!string.IsNullOrEmpty(text));
            statusText.text = text;
        }

        private static string GetStatusText(RoundManager round, RoundPhase phase) => phase switch
        {
            RoundPhase.ExtraTime => "추가 시간",
            RoundPhase.SuddenDeath => "서든데스! 먼저 때리면 승리",
            RoundPhase.GoldenGoal => "골든골! 먼저 앞서면 승리",
            _ when round.IsDuelRound => "결투 라운드",
            _ when round.Match != null && round.Match.IsDeuce => "듀스",
            _ => "",
        };
    }
}
