using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    public float horizontalAxis;
    public float verticalAxis;
    public float rHorizontalAxis;
    public float rVerticalAxis;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
        horizontalAxis = Input.GetAxis("Horizontal");
        verticalAxis = Input.GetAxis("Vertical");
        rHorizontalAxis = Input.GetAxis("RHorizontal");
        rVerticalAxis = Input.GetAxis("RVertical");

        if (horizontalAxis != 0 || verticalAxis != 0) {
            //Do
            print(horizontalAxis + " vert " + verticalAxis);
        }

        if (rHorizontalAxis != 0 || rVerticalAxis != 0)
        {
            //Do 
            print(horizontalAxis + " vert " + verticalAxis);
        }

        if (Input.GetButtonDown("Fire1")) {
            // button
            print("button");
        }

    }
}
