using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // ESC 일시정지 메뉴 (보여 주기만): 계속하기 / 설정 / 나가기
    public class PauseMenuView : UIPanel
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button leaveButton;
        [SerializeField] private TextMeshProUGUI leaveLabel;        // "방 나가기" / "메인 메뉴로"

        public event Action ResumeClicked;
        public event Action SettingsClicked;
        public event Action LeaveClicked;

        private void Awake()
        {
            resumeButton.onClick.AddListener(() => ResumeClicked?.Invoke());
            settingsButton.onClick.AddListener(() => SettingsClicked?.Invoke());
            leaveButton.onClick.AddListener(() => LeaveClicked?.Invoke());
        }

        public void SetLeaveLabel(string text)
        {
            if (leaveLabel != null) leaveLabel.text = text;
        }
    }
}
