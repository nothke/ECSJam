using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Rendering.PostProcessing;

public class ScopePanner : MonoBehaviour
{
    new public Camera camera;

    public Transform lensCover;
    public Transform restPoint;
    public Transform centerPoint;

    public PostProcessVolume volume;

    DepthOfField dof;

    void Start()
    {
        x = transform.position.x;
        y = transform.position.z;
        z = transform.position.y;

        dof = volume.profile.GetSetting<DepthOfField>();
        //Debug.Log(dof);
        startF = dof.focusDistance.value;
    }

    public float speedMult = 1;

    float x, y, z;

    public float[] fovs;
    int current;

    int puttingstate = 0;
    float stateTime = 1;

    float time = 0.1f;
    float mult = 3;

    public float focusMult = 10;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
            SwitchLens();

        x += Input.GetAxis("Horizontal") * speedMult * Time.deltaTime;
        y += Input.GetAxis("Vertical") * speedMult * Time.deltaTime;

        transform.position = new Vector3(x, z, y);

        if (puttingstate == 1)
        {
            lensCover.position = Vector3.Lerp(restPoint.position, centerPoint.position, 1 - stateTime);
            stateTime -= Time.deltaTime * mult;

            if (stateTime <= 0)
            {
                puttingstate = 2;
                stateTime = 1;

                camera.fieldOfView = fovs[current];
            }
        }

        if (puttingstate == 2)
        {
            lensCover.position = Vector3.Lerp(centerPoint.position, restPoint.position, 1 - stateTime);
            stateTime -= Time.deltaTime * mult;

            if (stateTime <= 0)
            {
                puttingstate = 0;
                stateTime = 1;

                lensCover.position = restPoint.position;
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
            MoveFocus(scroll);
    }

    float startF;
    void MoveFocus(float by)
    {
        startF += by * 10;
        dof.focusDistance.Override(startF + by * focusMult);
        //Debug.Log(startF);
    }

    void SwitchLens()
    {
        current++;

        if (current >= fovs.Length)
            current = 0;

        puttingstate = 1;
    }
}
