using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeHit : MonoBehaviour
{
    private bool isHit = false;

    private void Start()
    {
        StartCoroutine(EnableCollision());    
        Destroy(gameObject, 3f);
    }

    IEnumerator EnableCollision()
    {
        yield return new WaitForSeconds(.1f);
        isHit = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isHit) return;

        PlayerMovement pM = other.GetComponent<PlayerMovement>();

        if (pM != null && CanStun(pM))
        {
            pM.Stun(1.0f);
        }

        Destroy(gameObject);
    }

    // 온라인: 각 PC가 가시를 따로 날리므로 내 토끼일 때만 기절 (상대 토끼는 그 사람 PC가 판정). 로컬은 둘 다
    private static bool CanStun(PlayerMovement pM) =>
        !Online.OnlineSession.IsOnline || (pM.TryGetComponent(out Online.OnlineRabbit net) && net.IsMine);
}
