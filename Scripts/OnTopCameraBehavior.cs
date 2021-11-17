using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTopCameraBehavior : MonoBehaviour
{
    private Camera myCamera;
    // Start is called before the first frame update
    void Start()
    {
        myCamera  = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Camera.main.fieldOfView != myCamera.fieldOfView)
        {
            myCamera.fieldOfView = Camera.main.fieldOfView;
        }
        //Debug.Log("Camera fov difference:" + (Camera.main.fieldOfView - myCamera.fieldOfView));
        //Debug.Log("Camera position difference: " + (Camera.main.transform.position - myCamera.transform.position).magnitude);

    }
}
