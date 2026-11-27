using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Rules;
using UnityEngine;

namespace Online
{
    // 대기실 상태. 씬에 놓인 NetworkObject라서 주인은 방장
    // 각자 닉네임·준비를 방장에게 알리고, 둘 다 준비되면 방장이 게임 씬으로 이동
    public class OnlineRoom : NetworkBehaviour
    {
        public static OnlineRoom Instance { get; private set; }

        [Networked, Capacity(2)] private NetworkDictionary<PlayerRef, NetworkString<_16>> Names => default;
        [Networked, Capacity(2)] private NetworkDictionary<PlayerRef, NetworkBool> Ready => default;

        public static event Action<OnlineRoom> Opened;              // 대기실 상태가 준비됨 (씬보다 늦게 생겨서 따로 알림)
        public event Action Changed;                                // 닉네임·준비 상태가 바뀜 → 대기실 화면 갱신

        private float nextJoinSend;
        private bool starting;
        private ChangeDetector changes;

        public override void Spawned()
        {
            Instance = this;
            changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
            if (RoundManager.HasInstance) RoundManager.Instance.StopMatch();   // 게임에서 돌아왔을 때 정리
            GameSession.Instance.needFullReset = true;                         // 다음 게임은 1라운드부터
            if (HasStateAuthority) Runner.SessionInfo.IsOpen = true;           // 대기실에서는 들어올 수 있음

            Opened?.Invoke(this);
            Changed?.Invoke();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Instance == this) Instance = null;
        }

        // 내 닉네임이 목록에 없으면 방장에게 알림 (놓칠 수 있어서 들어갈 때까지 0.5초마다)
        public override void Render()
        {
            bool changed = false;
            foreach (string name in changes.DetectChanges(this))   // 닉네임·준비가 바뀌었으면 화면에 알림
                if (name == nameof(Names) || name == nameof(Ready)) changed = true;
            if (changed) Changed?.Invoke();

            if (!Names.ContainsKey(Runner.LocalPlayer) && Time.time >= nextJoinSend)
            {
                nextJoinSend = Time.time + 0.5f;
                RpcJoin(Runner.LocalPlayer, OnlineSession.Instance.Nickname);
            }
        }

        // 방장: 나간 사람을 정리하고, 둘 다 준비되면 시작
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || starting) return;

            var left = new List<PlayerRef>();
            foreach (var pair in Names)
                if (!Runner.ActivePlayers.Contains(pair.Key)) left.Add(pair.Key);
            foreach (PlayerRef p in left)
            {
                Names.Remove(p);
                Ready.Remove(p);
            }

            if (Names.Count < 2) return;
            foreach (var pair in Ready)
                if (!pair.Value) return;

            starting = true;
            Runner.SessionInfo.IsOpen = false;                                 // 게임 중에는 못 들어옴
            OnlineSession.Instance.LoadScene("GameScene");
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcJoin(PlayerRef player, string nickname)
        {
            Names.Set(player, nickname);
            if (!Ready.ContainsKey(player)) Ready.Set(player, false);
        }

        public void ToggleReady() => RpcSetReady(Runner.LocalPlayer, !IsReady(Runner.LocalPlayer));

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcSetReady(PlayerRef player, NetworkBool ready)
        {
            if (Names.ContainsKey(player)) Ready.Set(player, ready);
        }

        // ── 화면 표시용 ──
        // 번호가 작은 사람이 1P 자리 (OnlineSession.MyPlayer와 같은 규칙)
        public PlayerRef SlotPlayer(PlayerType slot)
        {
            List<PlayerRef> players = Names.Select(p => p.Key).OrderBy(p => p.PlayerId).ToList();
            int i = slot == PlayerType.Player1 ? 0 : 1;
            return i < players.Count ? players[i] : PlayerRef.None;
        }

        public string NameOf(PlayerRef p) => Names.TryGet(p, out var n) ? n.ToString() : "";
        public bool IsReady(PlayerRef p) => Ready.TryGet(p, out var r) && r;
        public bool IsMe(PlayerRef p) => p == Runner.LocalPlayer;
    }
}
