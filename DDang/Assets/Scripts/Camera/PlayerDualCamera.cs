using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDualCamera : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 5f;

    private Vector3 offset = new Vector3(4f, 3f, 0f);
    

    private void LateUpdate()
    {
        Vector3 targetPosition = target.position + offset;     
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(30f, -90f, 0f);
    }
}
