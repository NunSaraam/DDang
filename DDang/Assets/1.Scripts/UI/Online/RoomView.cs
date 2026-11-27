using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // 대기실 화면 (보여 주기만): 준비 버튼, 방 나가기, 안내 문구
    public class RoomView : MonoBehaviour
    {
        [SerializeField] private Button readyButton;
        [SerializeField] private TextMeshProUGUI readyButtonText;
        [SerializeField] private Button leaveButton;
        [SerializeField] private TextMeshProUGUI statusText;

        public event Action ReadyClicked;
        public event Action LeaveClicked;

        private void Awake()
        {
            readyButton.onClick.AddListener(() => ReadyClicked?.Invoke());
            leaveButton.onClick.AddListener(() => LeaveClicked?.Invoke());
        }

        public void SetReady(bool ready) => readyButtonText.text = ready ? "준비 취소" : "준비";
        public void SetStatus(string text) => statusText.text = text;
    }
}
