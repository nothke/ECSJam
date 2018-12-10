using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputComponent : MonoBehaviour {

    public bool W;
    public bool A;
    public bool S;
    public bool D;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.W))
        {
            W = true;
        }
        else
        {
            W = false;
        }

        if (Input.GetKey(KeyCode.A))
        {
            A = true;
        }
        else
        {
            A = false;
        }

        if (Input.GetKey(KeyCode.S))
        {
            S = true;
        }
        else
        {
            S = false;
        }

        if (Input.GetKey(KeyCode.D))
        {
            D = true;
        }
        else
        {
            D = false;
        }
    }
}
