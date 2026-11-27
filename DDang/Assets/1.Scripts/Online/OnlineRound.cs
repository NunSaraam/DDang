using Fusion;
using Rules;
using UnityEngine;

namespace Online
{
    // 온라인 라운드 공유. 씬에 놓인 NetworkObject라서 주인은 방장
    // 방장: RoundManager가 평소처럼 진행 → 단계·남은 시간·결과를 여기 적어 보냄
    // 상대: RoundManager는 직접 진행하지 않고 받은 값을 따라감 (UI·퀘스트·보상은 같은 이벤트로 동작)
    public class OnlineRound : NetworkBehaviour
    {
        public static OnlineRound Instance { get; private set; }

        [Networked] private RoundPhase Phase { get; set; }
        [Networked] private float RemainingTime { get; set; }
        [Networked] public NetworkBool GuestReady { get; private set; }   // 상대 PC도 맵 준비가 끝났는지

        private RoundManager round;
        private RoundPhase shownPhase;
        private float nextReadySend;

        public override void Spawned()
        {
            Instance = this;
            round = RoundManager.Instance;
            if (HasStateAuthority)
            {
                round.QuestTimeReached += SendQuestTime;
                round.RoundEnded += SendRoundEnded;
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (round != null)
            {
                round.QuestTimeReached -= SendQuestTime;
                round.RoundEnded -= SendRoundEnded;
            }
            if (Instance == this) Instance = null;
        }

        // 방장: 진행 상태를 매 틱 적음 → 상대 PC로 자동 전달
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority) return;
            Phase = round.Phase;
            RemainingTime = round.RemainingTime;
        }

        // 상대 PC: 받은 값으로 따라감
        public override void Render()
        {
            if (HasStateAuthority) return;

            // 맵이 다 만들어지면 방장에게 "준비 끝". 놓칠 수 있어서 받았다고 확인될 때까지 0.5초마다 보냄
            if (!GuestReady && round.PaintableCount > 0 && Time.time >= nextReadySend)
            {
                nextReadySend = Time.time + 0.5f;
                RpcReady();
            }

            if (Phase != shownPhase && Phase != RoundPhase.RoundEnd)    // 라운드 종료는 RpcRoundEnded가 처리
            {
                shownPhase = Phase;
                round.FollowPhase(Phase);
            }
            round.FollowTime(RemainingTime);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcReady() => GuestReady = true;

        // 방장: 난입 동물 경고를 상대 PC에도 (AnimalManager가 호출)
        public void SendAnimalWarning(int index) => RpcAnimalWarning((byte)index);

        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void RpcAnimalWarning(byte index)
        {
            if (Animal.AnimalManager.HasInstance) Animal.AnimalManager.Instance.FollowWarning(index);
        }

        private void SendQuestTime() => RpcQuestTime();

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcQuestTime()
        {
            if (!HasStateAuthority) round.FollowQuestTime();
        }

        private void SendRoundEnded(RoundResult r) => RpcRoundEnded((byte)r.Winner, r.P1Tiles, r.P2Tiles, r.WasDuel);

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcRoundEnded(byte winner, int p1Tiles, int p2Tiles, NetworkBool wasDuel)
        {
            if (HasStateAuthority) return;                              // 방장은 이미 처리함
            shownPhase = RoundPhase.RoundEnd;
            round.FollowRoundEnd(new RoundResult((PlayerType)winner, p1Tiles, p2Tiles, wasDuel));
        }
    }
}
