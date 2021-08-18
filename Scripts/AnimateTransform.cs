using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateTransform : MonoBehaviour
{

    private Vector3 targetPos;
    private Quaternion targetRot;

    private Vector3 fromPos;
    private Quaternion fromRot;

    //start time
    private float startTime = 0.0f;
    private float duration = 1.0f;    
    private AnimationCurve curve;

    public void Configure(Transform target, float duration, AnimationCurve curve){
        this.curve = curve;
        this.targetPos = target.position;
        this.targetRot = target.rotation;
        this.duration = duration;
    }

    void Start()
    {
        this.startTime = Time.time;
        this.fromPos = transform.position;
        this.fromRot = transform.rotation;

    }

    // Update is called once per frame
    void Update()
    {
        
        if (Time.time - startTime > duration){
            Destroy(this);
        }
        else
        {
            
            float percentage = curve.Evaluate((Time.time - startTime)/duration);
            transform.position = Vector3.Lerp(fromPos, targetPos, percentage);
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, percentage);
        }
    }
}
