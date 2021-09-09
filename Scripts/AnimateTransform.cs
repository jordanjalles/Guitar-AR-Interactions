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

    

    public void Configure(Vector3 targetPosition, Vector3 targetRotation, float duration, AnimationCurve curve){
        this.curve = curve;
        this.targetPos = targetPosition;
        this.targetRot = Quaternion.Euler(targetRotation);
        this.duration = duration;

        //if this gameobject currently has a another AnimateTransform, cancel it
        foreach (AnimateTransform anim in GetComponents<AnimateTransform>()){
            if (anim != this){
                anim.Complete();
                Debug.Log("Canceled other animation");
            }
        }

    }

    void Start()
    {
        this.startTime = Time.time;
        this.fromPos = transform.position;
        this.fromRot = transform.rotation;
    }

    public void Complete(){
        OnComplete();
        Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - startTime > duration){
            transform.position = targetPos;
            transform.rotation = targetRot;
            Complete();
        }
        else
        {  
            float percentage = curve.Evaluate((Time.time - startTime)/duration);
            transform.position = Vector3.Lerp(fromPos, targetPos, percentage);
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, percentage);
        }
    }

    public delegate void DelegateEvent();
    public DelegateEvent OnComplete = () => {};
}
