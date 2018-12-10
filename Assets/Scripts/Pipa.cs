using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipa : MonoBehaviour
{

    public float speedMult = 10;
    Vector3 lastmouse;

    private void Start()
    {
        lastmouse = Input.mousePosition;
    }

    void Update()
    {
        /*
        Vector3 mouseDiff = lastmouse - Input.mousePosition;
        transform.position -= new Vector3(-mouseDiff.x/2 + mouseDiff.x, 0, -mouseDiff.y / 2 + mouseDiff.y) * 0.1f;
        lastmouse = Input.mousePosition;
        */

        transform.position += new Vector3(Input.GetAxis("RHorizontal"), 0, Input.GetAxis("RVertical")) * Time.deltaTime * speedMult;

        if (Input.GetButton("Fire2"))
        {
            for (int i = 0; i < 7; i++)
            {
                MicrobeSpawner.SpawnEntity(transform.position, MicrobeSpawner.EntityType.Antibody);
            }
        }
    }
}
