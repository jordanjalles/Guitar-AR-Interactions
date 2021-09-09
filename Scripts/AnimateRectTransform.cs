using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AnimateRectTransform : MonoBehaviour
{

    private Vector2 targetPos;
    private Quaternion targetRot;

    private Vector2 fromPos;
    private Quaternion fromRot;

    //start time
    private float startTime = 0.0f;
    private float duration = 1.0f;    
    private AnimationCurve curve;

    private RectTransform rectTransform;

    

    public void Configure(Vector2 targetPosition, Quaternion targetRotation, float duration, AnimationCurve curve){
        this.curve = curve;
        this.targetPos = targetPosition;
        this.targetRot = targetRotation;
        this.duration = duration;

        //if this gameobject currently has a another AnimateTransform, cancel it
        foreach (AnimateRectTransform anim in GetComponents<AnimateRectTransform>()){
            if (anim != this){
                anim.Complete();
                Debug.Log("Canceled other UI animation");
            }
        }

    }

    void Start()
    {
        this.rectTransform = GetComponent<RectTransform>();

        this.startTime = Time.time;
        this.fromPos = rectTransform.offsetMin; //i.e. lower left corner
        this.fromRot = rectTransform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - startTime > duration){
            SetPosition(targetPos);
            SetRotation(targetRot);
            Complete();
        }
        else
        {  
            float percentage = curve.Evaluate((Time.time - startTime)/duration);
            SetPosition(Vector2.Lerp(fromPos, targetPos, percentage));
            SetRotation(Quaternion.Slerp(fromRot, targetRot, percentage));
        }
    }

    public void Complete(){
        OnComplete();
        Destroy(this);
    }

    public void SetPosition(Vector2 pos){
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, pos.y, rectTransform.rect.height);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, pos.x, rectTransform.rect.width);
    }

    void SetRotation(Quaternion rot){
        //todo: implement
    }

    public delegate void DelegateEvent();
    public DelegateEvent OnComplete = () => {};
}
