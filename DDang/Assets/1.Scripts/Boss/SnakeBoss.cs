using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeBoss : BossBase
{
    public GameObject wallPrefab;
    public float speed = 3f;
    public float wallDuration = 5f;

    public List<Vector3> pathPoint;

    private bool active = false;

    public override void Activate()
    {
        active = true;
        StartCoroutine(SnakeMove());
        Destroy(gameObject);

    }

    public override void Deactivate()
    {
        active = false;
    }

    IEnumerator SnakeMove()
    {
        foreach (Vector3 point in pathPoint)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                point,
                speed * Time.deltaTime);
        }

        PlaceWall();
        yield return null;
 
    }

    void PlaceWall()
    {
        GameObject wall = Instantiate(wallPrefab, transform.position, Quaternion.identity);
        Destroy(wall, wallDuration);
    }
}
