using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Transform))]
public class RotateConstant : MonoBehaviour
{
    [SerializeField]
    private float xRotation = 0;
    [SerializeField]
    private float yRotation = 0;
    [SerializeField]
    private float zRotation = 0;

    // Update is called once per frame
    void Update()
    {
        //rotate based on time elapsed
        transform.Rotate(new Vector3(xRotation * Time.deltaTime, yRotation * Time.deltaTime, zRotation * Time.deltaTime));
    }

    public void RotateLeft(float speed = 0.1f){
        yRotation = Mathf.Abs(speed);
    }

    public void RotateRight(float speed = 0.1f){
        yRotation = -Mathf.Abs(speed);
    }
}
