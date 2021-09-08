using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateUITransform : MonoBehaviour
{

    private Vector2 targetPos;
    private Quaternion targetRot;

    private Vector2 fromPos;
    private Quaternion fromRot;

    //start time
    private float startTime = 0.0f;
    private float duration = 1.0f;    
    private AnimationCurve curve;

    

    public void Configure(Vector2 targetPosition, Quaternion targetRotation, float duration, AnimationCurve curve){
        this.curve = curve;
        this.targetPos = targetPosition;
        this.targetRot = targetRotation;
        this.duration = duration;

        //if this gameobject currently has a another AnimateTransform, cancel it
        foreach (AnimateUITransform anim in GetComponents<AnimateUITransform>()){
            if (anim != this){
                Destroy(anim);
                Debug.Log("Canceled other UI animation");
            }
        }

    }

    void Start()
    {
        this.startTime = Time.time;
        this.fromPos = GetComponent<RectTransform>().anchoredPosition;
        this.fromRot = GetComponent<RectTransform>().rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - startTime > duration){
            SetPosition(targetPos);
            SetRotation(targetRot);
            OnComplete();
            Destroy(this);
        }
        else
        {  
            float percentage = curve.Evaluate((Time.time - startTime)/duration);
            SetPosition(Vector2.Lerp(fromPos, targetPos, percentage));
            SetRotation(Quaternion.Slerp(fromRot, targetRot, percentage));
        }
    }

    void SetPosition(Vector2 pos){

    }

    void SetRotation(Quaternion rot){

    }

    public delegate void DelegateEvent();
    public DelegateEvent OnComplete = () => {};
}
