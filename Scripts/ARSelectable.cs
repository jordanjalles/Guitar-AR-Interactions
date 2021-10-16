using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARSelectable : MonoBehaviour
{
    public bool isSelected = false;
    
    public Vector3 homePosition;
    public Vector3 homeRotation;
    public Transform homeParent;
    public Color backgroundColor;

    [SerializeField]
    private AnimationCurve curveForTransitions;

    private static List<ARSelectable> selectables = new List<ARSelectable>();
    
    // Start is called before the first frame update
    void Start()
    {
        selectables.Add(this);
        this.homePosition = this.transform.position;
        this.homeRotation = this.transform.rotation.eulerAngles;
        this.homeParent = this.transform.parent;
        Deselect();
    }

    // remove from selectables list when destroyed
    private void OnDestroy()
    {
        selectables.Remove(this);
    }

    public void Select(){
        this.isSelected = true;
    
        RemoveInteractionTriggers(); // remove all interaction triggers to start fresh
        ChangeToLayer("OnTop");
        MoveToCameraView();
        
        //add interaction trigger component for deselecting self on tap empty
        InteractionTrigger trigger = this.gameObject.AddComponent<InteractionTrigger>();
        trigger.interactionTarget = InteractionTrigger.InteractionTarget.Empty;
        trigger.OnTap += () => { Deselect(); };

        //todo: add interaction triggers for drag, rotate, and scale
        
        //remove selection from the other ar selectables
        foreach (ARSelectable s in selectables){
            if (s != this){
                s.RemoveInteractionTriggers();
            }
        }

        //turn on arannotations
        foreach (ARAnnotation a in GetComponentsInChildren<ARAnnotation>(includeInactive : true)){
            a.gameObject.SetActive(true);
        }

        //todo: how to animate the ar background color from here?
    }

    public void Deselect(){
        this.isSelected = false;

        //turn off annotations
        foreach (ARAnnotation a in GetComponentsInChildren<ARAnnotation>()){
            a.Hide(); //calling hide so that it removes any detail display that may be active
            a.gameObject.SetActive(false);
        }

        //todo: how to animate the ar background color?
        
        RemoveInteractionTriggers();
        MoveToHomeLocation();

        //add selection to the other all ar selectables
        foreach (ARSelectable s in selectables){
            s.AddTapToSelect();
        }
    }

    public void AddTapToSelect(){
        //remove others so there aren't duplicates
        RemoveInteractionTriggers();

        //add interaction trigger component to select self on tap
        InteractionTrigger trigger = this.gameObject.AddComponent<InteractionTrigger>();
        trigger.OnTap += () => { Select(); };
    }

    public void AddTapEmptyToDeselect(){
        //remove others so there aren't duplicates
        RemoveInteractionTriggers();

        //add interaction trigger component to deselect self on tap
        InteractionTrigger trigger = this.gameObject.AddComponent<InteractionTrigger>();
        trigger.OnTap += () => { Deselect(); };
        trigger.interactionTarget = InteractionTrigger.InteractionTarget.Empty;
    }

    public void RemoveInteractionTriggers(){
        foreach (InteractionTrigger t in GetComponentsInChildren<InteractionTrigger>()){
            Destroy(t.gameObject);
        }
    }

    public void ChangeToLayer(string layerName){
        foreach (Transform child in GetComponentsInChildren<Transform>(includeInactive : true)){
            child.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }

    public void PlayAudio(){
        if (this.GetComponent<AudioSource>() != null){
            this.GetComponent<AudioSource>().Play();
        }
    }

    private void MoveToCameraView(){
        Transform targetTransform = new GameObject("Animation target transform").transform;
        targetTransform.parent = Camera.main.transform;

        //move location to 1.2 meters in front of the camera
        targetTransform.position = Camera.main.transform.position + (Camera.main.transform.forward * 1.2f);
        //rotate to look at the camera.
        targetTransform.rotation = Camera.main.transform.rotation;

        AnimateTransform animator = gameObject.AddComponent<AnimateTransform>(); //animate the guitar body to the selected guitar location
        animator.Configure(targetTransform, 1f, curveForTransitions);
        animator.OnComplete += () => {transform.parent = Camera.main.transform;}; //when the animation is complete, parent the guitar body to the camera
        animator.OnComplete += () => {Destroy(targetTransform.gameObject);}; //destroy the target transform
    }

    private void MoveToHomeLocation(){
        Transform targetTransform = new GameObject("Animation target transform").transform;
        targetTransform.position = this.homePosition;
        targetTransform.rotation = Quaternion.Euler(this.homeRotation);

        AnimateTransform animator = gameObject.AddComponent<AnimateTransform>(); //animate the guitar body to the selected guitar location
        animator.Configure(targetTransform, 1f, curveForTransitions);
        animator.OnComplete += () => {ChangeToLayer("Default");};
        animator.OnComplete += () => {transform.parent = homeParent;}; 
        animator.OnComplete += () => {Destroy(targetTransform.gameObject);}; //destroy the target transform

        
    }
}
