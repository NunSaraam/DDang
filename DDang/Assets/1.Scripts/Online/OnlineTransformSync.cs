using Fusion;
using UnityEngine;

namespace Online
{
    // 주인 PC가 움직이는 물체의 위치를 직접 주고받음 (NetworkTransform 대신)
    // 주인: 평소처럼 움직이고 매 틱 위치를 적기만 → 내 화면은 건드리지 않아 부드러움
    // 상대: 받은 위치로 부드럽게 따라감
    public class OnlineTransformSync : NetworkBehaviour
    {
        [SerializeField] private float followSharpness = 20f;          // 상대 PC가 받은 위치를 따라가는 빠르기 (클수록 딱 붙음)

        [Networked] private Vector3 NetPosition { get; set; }
        [Networked] private Quaternion NetRotation { get; set; }
        [Networked] private byte TeleportCount { get; set; }            // 바뀌면 상대 PC는 따라가지 않고 바로 이동

        private byte shownTeleport;

        public override void Spawned()
        {
            if (HasStateAuthority) Write();
            else
            {
                if (NetPosition != Vector3.zero) transform.SetPositionAndRotation(NetPosition, NetRotation);   // 아직 안 적혔으면 씬 위치 그대로
                shownTeleport = TeleportCount;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority) Write();
        }

        private void Write()
        {
            NetPosition = transform.position;
            NetRotation = transform.rotation;
        }

        // 상대 PC: 받은 위치로 부드럽게 따라감. 순간이동이면 바로
        public override void Render()
        {
            if (HasStateAuthority || NetPosition == Vector3.zero) return;

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

        // 주인: 순간이동 (상대 화면에서도 미끄러지지 않게)
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!HasStateAuthority) return;
            transform.SetPositionAndRotation(position, rotation);
            NetPosition = position;
            NetRotation = rotation;
            TeleportCount++;
        }
    }
}
