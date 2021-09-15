using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateTransform : MonoBehaviour
{

    private Vector3 targetPos;
    private Quaternion targetRot;
    private Transform targetTransform;

    private Vector3 fromPos;
    private Quaternion fromRot;

    //start time
    private float startTime = 0.0f;
    private float duration = 1.0f;    
    private AnimationCurve curve;

    

    public void Configure(Transform targetTransform, float duration, AnimationCurve curve){
        this.curve = curve;
        this.targetTransform = targetTransform;
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
            transform.position = targetTransform.position;
            transform.rotation = targetTransform.rotation;
            Complete();
        }
        else
        {  
            float percentage = curve.Evaluate((Time.time - startTime)/duration);
            transform.position = Vector3.Lerp(fromPos, targetTransform.position, percentage);
            transform.rotation = Quaternion.Slerp(fromRot, targetTransform.rotation, percentage);
        }
    }

    public delegate void DelegateEvent();
    public DelegateEvent OnComplete = () => {};
}
