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
        HideAnnotations();
        AddTapToSelect();
    }

    // remove from selectables list when destroyed
    private void OnDestroy()
    {
        selectables.Remove(this);
    }

    public void Select(){
        this.isSelected = true;
    

        ChangeToLayer("OnTop");
        MoveToCameraView();
        SetUpSelectedInteractions();
        
        
        //remove selection from the other ar selectables
        foreach (ARSelectable s in selectables){
            if (s != this){
                s.RemoveTriggersAndActions();
            }
        }

        //todo: how to animate the ar background color from here?
        //todo: add interaction triggers for drag, rotate, and scale
    }

    public void SetUpSelectedInteractions(){
        RemoveTriggersAndActions(); // remove all interaction triggers to start fresh
        AddTapNotSelfToDeselect();
        AddDragToRotate();
        AddPinchToZoom();
        ShowAnnotations();
    }

    public void SetUpAnnotationViewInteractions(){
        RemoveTriggersAndActions(); // remove all interaction triggers to start fresh
        AddTapToResetSelectedView();
    }

    public void Deselect(){
        if (!this.isSelected) return; 

        this.isSelected = false;

        //todo: how to animate the ar background color?
        HideAnnotations();
        RemoveTriggersAndActions();
        MoveToHomeLocation();

        //add selection to the other all ar selectables
        foreach (ARSelectable s in selectables){
            s.AddTapToSelect();
        }
    }

    public void HideAnnotations(){
        //turn off annotations
        foreach (ARAnnotation a in GetComponentsInChildren<ARAnnotation>()){
            a.Hide(); //calling hide so that it removes any detail display that may be active
            a.gameObject.SetActive(false);
        }
    }

    public void ShowAnnotations(){
        //turn on arannotations
        foreach (ARAnnotation a in GetComponentsInChildren<ARAnnotation>(includeInactive : true)){
            a.gameObject.SetActive(true);
        }
    }

    public static ARSelectable GetSelected(){
        foreach (ARSelectable s in selectables){
            if (s.isSelected){
                return s;
            }
        }
        return null;
    }

    public void AddTapToSelect(){
        //add interaction trigger component to select self on tap
        InteractionTrigger trigger = this.gameObject.AddComponent<InteractionTrigger>();
        trigger.interactionType = InteractionTrigger.InteractionType.Tap;
        trigger.interactionTarget = InteractionTrigger.InteractionTarget.Self;
        trigger.name = "Tap to select";
        trigger.OnTap += () => { Select(); };
    }

    public void AddTapToResetSelectedView(){
        //add interaction trigger component to go to selected view on tap
        InteractionTrigger trigger = this.gameObject.AddComponent<InteractionTrigger>();
        trigger.interactionType = InteractionTrigger.InteractionType.Tap;
        trigger.interactionTarget = InteractionTrigger.InteractionTarget.NotSelf;
        trigger.name = "Tap to reset selected view";
        trigger.OnTap += () => { 
            MoveToCameraView();
            SetUpSelectedInteractions();
        };

    }

    public void AddDragToRotate(){
        //add interaction trigger component to rotate on grab
        BasicTransformAction rotateY = this.gameObject.AddComponent<BasicTransformAction>();
        rotateY.type = BasicTransformAction.Type.Rotate;
        rotateY.axis = BasicTransformAction.Axis.y;
        rotateY.space = BasicTransformAction.Space.Local;
        rotateY.invertInput = true;
        rotateY.multiplier = 0.3f;

        BasicTransformAction rotateX = this.gameObject.AddComponent<BasicTransformAction>();
        rotateX.type = BasicTransformAction.Type.Rotate;
        rotateX.axis = BasicTransformAction.Axis.x;
        rotateX.space = BasicTransformAction.Space.World;
        rotateX.invertInput = false;
        rotateX.multiplier = 0.3f;

        InteractionTrigger dragTrigger = this.gameObject.AddComponent<InteractionTrigger>();
        dragTrigger.interactionType = InteractionTrigger.InteractionType.Drag;
        dragTrigger.interactionTarget = InteractionTrigger.InteractionTarget.Self;
        dragTrigger.OnDrag += (input) => {rotateY.Activate(input.x);};
        dragTrigger.OnDrag += (input) => {rotateX.Activate(input.y);};
        dragTrigger.name = "Drag to rotate";
    }

    public void AddPinchToZoom(){
        //add interaction trigger component to zoom on pinch
        BasicTransformAction zoom = this.gameObject.AddComponent<BasicTransformAction>();
        zoom.type = BasicTransformAction.Type.Translate;
        zoom.axis = BasicTransformAction.Axis.z;
        zoom.space = BasicTransformAction.Space.Camera;
        zoom.multiplier = 0.001f;

        InteractionTrigger pinchTrigger = this.gameObject.AddComponent<InteractionTrigger>();
        pinchTrigger.interactionType = InteractionTrigger.InteractionType.Pinch;
        pinchTrigger.interactionTarget = InteractionTrigger.InteractionTarget.Any;
        pinchTrigger.OnPinch += (input) => {zoom.Activate(input);};
        pinchTrigger.name = "Pinch to zoom";
    }

    public void AddTapNotSelfToDeselect(){
        //add interaction trigger component to deselect self on tap
        InteractionTrigger trigger = this.gameObject.AddComponent<InteractionTrigger>();
        trigger.interactionTarget = InteractionTrigger.InteractionTarget.NotSelf;
        trigger.OnTap += Deselect;
        trigger.name = "Deselect";
    }

    public void RemoveTriggersAndActions(){
        RemoveInteractionTriggers();
        RemoveTransformActions();
    }

    public void RemoveInteractionTriggers(){
        foreach (InteractionTrigger trigger in GetComponentsInChildren<InteractionTrigger>()){
            //destroy trigger, ignoring annotations
            if (!trigger.GetComponent<ARAnnotation>()) Destroy(trigger);
        }
    }

    public void RemoveTransformActions(){
        foreach (BasicTransformAction action in GetComponentsInChildren<BasicTransformAction>()){
            //destroy action, ignoring annotations  
            if (!action.GetComponent<ARAnnotation>()) Destroy(action);
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
