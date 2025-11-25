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

        if (pM != null)
        {
            pM.Stun(1.0f);
        }

        Destroy(gameObject);
    }
}
