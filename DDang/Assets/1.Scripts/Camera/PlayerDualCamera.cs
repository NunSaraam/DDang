using UnityEngine;

namespace Cameras
{
    public class PlayerDualCamera : MonoBehaviour
    {
        public Transform target;

        [SerializeField] private Vector3 offset = new Vector3(4f, 4f, 0f);
        [SerializeField] private Vector3 eulerAngles = new Vector3(30f, -90f, 0f);
        [SerializeField, Min(0f)] private float smoothTime = 0.15f;    // 목표 위치에 도달하는시간

        private Vector3 velocity;

        private void Start()
        {
            transform.rotation = Quaternion.Euler(eulerAngles);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 goal = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
        }

        public void SetTarget(Transform newTarget)                    // 대상 지정 시 즉시 위치 맞춤
        {
            target = newTarget;
            if (target == null) return;

            transform.position = target.position + offset;
            velocity = Vector3.zero;
        }
    }
}
