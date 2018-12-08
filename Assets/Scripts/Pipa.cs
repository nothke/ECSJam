using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipa : MonoBehaviour
{

    Vector3 lastmouse;

    private void Start()
    {
        lastmouse = Input.mousePosition;
    }

    void Update()
    {
        Vector3 mouseDiff = lastmouse - Input.mousePosition;
        transform.position += new Vector3(-mouseDiff.x/2 + mouseDiff.x, 0, -mouseDiff.y / 2 + mouseDiff.y) * 0.1f;
        lastmouse = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            EnemySpawner.SpawnEnemyAtPosition(transform.position);
        }
    }
}
