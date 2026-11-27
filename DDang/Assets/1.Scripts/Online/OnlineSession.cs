using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Rules;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Online
{
    // 온라인 대전 연결을 들고 다니는 관리자. 로비에서 만들어지고 씬이 바뀌어도 남음
    // 이 오브젝트가 없으면(IsOnline == false) 기존 로컬 2인 모드 그대로 동작
    public class OnlineSession : MonoBehaviour
    {
        public const string WaitingRoomScene = "WaitingRoom";

        public static OnlineSession Instance { get; private set; }
        public static bool IsOnline => Instance != null && Instance.Runner != null && Instance.Runner.IsRunning;
        public static bool InWaitingRoom => SceneManager.GetActiveScene().name == WaitingRoomScene;

        public NetworkRunner Runner { get; private set; }
        public PlayerType MyPlayer { get; private set; } = PlayerType.Player1;   // 대기실에서 정해짐
        public string RoomName { get; private set; }
        public string Nickname { get; private set; }
        public bool OpponentLeft { get; private set; }
        public static string LastMessage;                                   // 로비에 보여 줄 안내 (연결 끊김 등)

        private float backAt;                                               // 상대가 나가면 대기실로 돌아갈 시각
        private bool connected;                                             // 접속에 성공한 뒤 아직 직접 나가지 않았는지
        private static bool quitting;

        public static OnlineSession Create()
        {
            Application.quitting -= OnQuitting;
            Application.quitting += OnQuitting;
            var go = new GameObject("OnlineSession");
            DontDestroyOnLoad(go);                                          // 씬이 바뀌어도 연결 유지
            return Instance = go.AddComponent<OnlineSession>();
        }

        // 성공하면 null, 실패하면 안내 글
        public async Task<string> Connect(string roomName, string nickname)
        {
            RoomName = roomName;
            Nickname = nickname;
            Runner = gameObject.AddComponent<NetworkRunner>();
            StartGameResult result = await Runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Shared,
                SessionName = roomName,
                PlayerCount = 2,
                Scene = SceneRef.FromIndex(BuildIndex(WaitingRoomScene)),   // 먼저 온 사람이 대기실을 열고, 나중에 온 사람은 따라 들어감
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()   // 방장이 씬을 바꾸면 상대도 따라옴
            });
            if (result.Ok)
            {
                connected = true;
                return null;
            }

            Close();
            return result.ShutdownReason switch
            {
                ShutdownReason.GameIsFull => "방이 꽉 찼어요 (2명)",
                ShutdownReason.GameClosed => "게임이 진행 중이라 들어갈 수 없어요",
                _ => $"접속 실패: {result.ShutdownReason}"
            };
        }

        // 방장만: 두 PC를 같은 씬으로 이동
        public void LoadScene(string sceneName)
        {
            if (!Runner.IsSharedModeMasterClient) return;
            Runner.LoadScene(SceneRef.FromIndex(BuildIndex(sceneName)));
        }

        private static int BuildIndex(string sceneName) =>
            SceneUtility.GetBuildIndexByScenePath($"Assets/0.Scenes/{sceneName}.unity");

        private void Update()
        {
            // 접속 중이었는데 Runner가 멈춤 = 인터넷 끊김 등
            if (connected && (Runner == null || !Runner.IsRunning))
            {
                LostConnection();
                return;
            }
            if (!IsOnline) return;

            if (InWaitingRoom)                      // 번호는 대기실에서만 정함 (게임 중에는 고정)
            {
                OpponentLeft = false;
                MyPlayer = CurrentSlot();
                return;
            }
            CheckOpponentLeft();
        }

        // 지금 방 인원 기준 내 번호 (번호가 작은 사람이 1P, 혼자면 1P)
        public PlayerType CurrentSlot()
        {
            int first = Runner.ActivePlayers.Min(p => p.PlayerId);
            return Runner.LocalPlayer.PlayerId == first ? PlayerType.Player1 : PlayerType.Player2;
        }

        // 게임 중 상대가 나감 → 안내 후 3초 뒤 대기실로 (남은 사람이 방장이 됨)
        private void CheckOpponentLeft()
        {
            if (!OpponentLeft && Runner.ActivePlayers.Count() < 2 && SceneManager.GetActiveScene().name != "GameResult")
            {
                OpponentLeft = true;
                backAt = Time.unscaledTime + 3f;
            }
            if (OpponentLeft && backAt > 0f && Time.unscaledTime >= backAt)
            {
                backAt = 0f;
                BackToWaitingRoom();
            }
        }

        // 방장만 실제로 이동. 상대는 자동으로 따라옴
        public void BackToWaitingRoom()
        {
            if (RoundManager.HasInstance) RoundManager.Instance.StopMatch();
            LoadScene(WaitingRoomScene);
        }

        public async void Leave()
        {
            connected = false;                                                  // 직접 나가는 것은 끊김이 아님
            if (RoundManager.HasInstance) RoundManager.Instance.StopMatch();    // 진행 중인 라운드 정리
            GameSession.Instance.needFullReset = true;                          // 다음 게임은 처음부터
            if (Runner != null) await Runner.Shutdown();
            Close();
            SceneLoadManager.Instance.LoadScene("Mainmenu");
        }

        // 연결이 끊김: 라운드 정리 → 로비로 이동해서 안내 (다시 입장할 수 있게)
        private void LostConnection()
        {
            connected = false;
            if (RoundManager.HasInstance) RoundManager.Instance.StopMatch();
            if (GameSession.Instance != null) GameSession.Instance.needFullReset = true;
            LastMessage = "연결이 끊겼습니다";
            Close();
            SceneLoadManager.Instance.LoadScene("Lobby");
        }

        // Fusion이 끊김을 처리하면서 이 오브젝트를 지운 경우에도 같은 처리 (게임 종료 중은 제외)
        private void OnDestroy()
        {
            if (connected && !quitting) LostConnection();
            if (Instance == this) Instance = null;
        }

        private static void OnQuitting() => quitting = true;

        private void Close()
        {
            if (Instance == this) Instance = null;
            Destroy(gameObject);
        }

        private void OnGUI()
        {
            // 방 나가기는 ESC 일시정지 메뉴(UI.PauseMenuPresenter)로 옮김. 여기는 상대가 나갔을 때 안내만
            if (OpponentLeft)
            {
                var style = new GUIStyle(GUI.skin.label) { fontSize = 32, alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "상대가 나갔습니다. 대기실로 돌아갑니다", style);
            }
        }
    }
}
