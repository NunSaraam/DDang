using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // 온라인 로비 화면 (보여 주기만): 닉네임·방 코드 입력, 입장·뒤로 버튼, 안내 문구
    public class LobbyView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nicknameInput;
        [SerializeField] private TMP_InputField roomInput;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private int nicknameLimit = 8;
        [SerializeField] private int roomLimit = 12;

        public event Action<string, string> JoinClicked;            // (닉네임, 방 코드) 앞뒤 공백 제거
        public event Action BackClicked;

        private void Awake()
        {
            nicknameInput.characterLimit = nicknameLimit;
            roomInput.characterLimit = roomLimit;
            messageText.text = "";
            joinButton.onClick.AddListener(() => JoinClicked?.Invoke(nicknameInput.text.Trim(), roomInput.text.Trim()));
            backButton.onClick.AddListener(() => BackClicked?.Invoke());
        }

        public void SetNickname(string nickname) => nicknameInput.text = nickname;
        public void SetMessage(string text) => messageText.text = text ?? "";

        public void SetInteractable(bool on)
        {
            nicknameInput.interactable = roomInput.interactable = on;
            joinButton.interactable = backButton.interactable = on;
        }
    }
}
