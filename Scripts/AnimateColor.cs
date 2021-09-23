using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimateColor : MonoBehaviour
{

    private Color startColor;
    private Color endColor;

    //start time
    private float startTime = 0.0f;
    private float duration = 1.0f;    
    private AnimationCurve curve;

    

    public void Configure(Color targetColor, float duration, AnimationCurve curve){
        this.curve = curve;
        this.duration = duration;
        this.endColor = targetColor;

        //if this gameobject currently has a another animate color, cancel it
        foreach (AnimateColor anim in GetComponents<AnimateColor>()){
            if (anim != this){
                anim.Complete();
                Debug.Log("Canceled other animation");
            }
        }

    }

    void Start()
    {
        this.startTime = Time.time;
        this.startColor = GetComponent<Image>().color;

    }

    public void Complete(){
        OnComplete();
        Destroy(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - startTime > duration){
            //set the color to the end color
            GetComponent<Image>().color = endColor;
            Complete();
        }
        else
        {  
            float percentage = curve.Evaluate((Time.time - startTime)/duration);
            //lerp the color by percentage
            GetComponent<Image>().color = Color.Lerp(startColor, endColor, percentage);
        }
    }

    public delegate void DelegateEvent();
    public DelegateEvent OnComplete = () => {};
}
