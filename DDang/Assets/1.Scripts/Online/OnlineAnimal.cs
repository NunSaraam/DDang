using Animal;
using Fusion;
using UnityEngine;

namespace Online
{
    // 난입 동물 프리팹에 붙임 (NetworkObject와 함께. NetworkTransform은 쓰지 않음)
    // 방장: 동물이 평소처럼 생각하고 움직임 → 위치는 여기서 매 틱 적기만 (방장 화면은 건드리지 않음)
    // 상대: 동물 행동은 끄고, 받은 위치로 부드럽게 따라가며 연출만 보여 줌
    [RequireComponent(typeof(AnimalBase))]
    public class OnlineAnimal : NetworkBehaviour
    {
        [SerializeField] private float followSharpness = 20f;          // 상대 PC가 받은 위치를 따라가는 빠르기 (클수록 딱 붙음)

        [Networked] public byte DataIndex { get; set; }                 // AnimalManager 등장 목록 번호 (방장이 등장 전에 기록)
        [Networked] private Vector3 NetPosition { get; set; }           // 방장이 매 틱 적는 위치
        [Networked] private Quaternion NetRotation { get; set; }
        [Networked] private byte TeleportCount { get; set; }            // 바뀌면 상대 PC는 따라가지 않고 바로 이동

        private AnimalBase animal;
        private bool follower;
        private byte shownTeleport;

        public override void Spawned()
        {
            animal = GetComponent<AnimalBase>();
            follower = !HasStateAuthority;

            if (follower)
            {
                transform.SetPositionAndRotation(NetPosition, NetRotation);   // 등장 위치에 바로
                shownTeleport = TeleportCount;
                if (AnimalManager.HasInstance) AnimalManager.Instance.FollowSpawn(animal, DataIndex);   // 상대 PC: 등장
            }
            else WriteTransform();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (follower) animal.FinishRemote();                         // 상대 PC: 퇴장 정리 (얼음·벽·어둠 등)
        }

        // 방장: 움직임은 동물 코드가 평소처럼 하고, 여기서는 위치를 적기만 함
        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority) WriteTransform();
        }

        private void WriteTransform()
        {
            NetPosition = transform.position;
            NetRotation = transform.rotation;
        }

        // 상대 PC: 받은 위치로 부드럽게 따라감. 순간이동이면 바로
        public override void Render()
        {
            if (!follower) return;

            if (TeleportCount != shownTeleport)
            {
                shownTeleport = TeleportCount;
                transform.SetPositionAndRotation(NetPosition, NetRotation);
                return;
            }

            float k = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, NetPosition, k);
            transform.rotation = Quaternion.Slerp(transform.rotation, NetRotation, k);
        }

        // 방장: 순간이동 (상대 PC에서도 미끄러지지 않게)
        public void Teleport(Vector3 position)
        {
            transform.position = position;
            NetPosition = position;
            TeleportCount++;
        }

        // 방장 → 상대 PC: 연출 신호 (번호와 값 2개는 동물마다 뜻을 정함)
        public void SendEvent(byte id, int a, int b) => RpcEvent(id, a, b);

        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void RpcEvent(byte id, int a, int b) => animal.ReceiveEvent(id, a, b);

        // 상대 PC → 방장: 내 토끼가 이 동물을 때림
        public void RequestHit(PlayerType attacker) => RpcHit((byte)attacker);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcHit(byte attacker) => animal.HandleHitBy((PlayerType)attacker);
    }
}
