using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARSelectionController : MonoBehaviour
{
    [SerializeField]
    private Transform selectedTargetLocation;
    
    [SerializeField]
    private AnimationCurve curveForTransitions;

    [SerializeField]
    private Camera arCamera;

    private List<Transform> selectables = new List<Transform>();
    private bool itemSelected = false;
    private int selectedIndex = -1;
    private int newSelectableTouchedIndex;
    private Vector2 lastTouchPosition;
    private float lastTwoFingerDistance;
    private float lastTwoFingerAngle;
    private Vector2 lastTwoFingerAveragePosition;

    enum InteractionState { touchingNew, grabbingSelected, touchingEmpty, notTouching, twoFingersTouching, touchingAnnotation};
    private InteractionState interactionState = InteractionState.notTouching;

    private void Awake()
    {
        SwipeDetector.OnSwipe += SwipeAction_OnSwipe;
        BasicInputDetector.OnTouchBegan += OnTouchBegan;
        BasicInputDetector.OnTouchEnded += OnTouchEnded;
        BasicInputDetector.OnTouchMoved += OnTouchMoved;
        BasicInputDetector.OnTwoTouches += OnTwoTouches;

        //get the child guitars
        foreach (ARSelectable child in GetComponentsInChildren(typeof(ARSelectable))){
            selectables.Add(child.transform);
            Debug.Log("Selectable found: " + child.name);
        }
    }

    private void OnTouchBegan(Vector2 tapPosition)
    {
        //todo: rewrite this as a state machine
        lastTouchPosition = tapPosition;

        Ray ray = arCamera.ScreenPointToRay(tapPosition);
        RaycastHit hitObject;
        if(Physics.Raycast(ray, out hitObject))
        {
            if(selectables.Contains(hitObject.transform)){ 
                newSelectableTouchedIndex = selectables.IndexOf(hitObject.transform);
                if (selectedIndex == newSelectableTouchedIndex && itemSelected)
                {
                    interactionState = InteractionState.grabbingSelected;
                }else
                {
                    interactionState = InteractionState.touchingNew;
                }
            }else if (hitObject.transform.GetComponent<ARAnnotation>() != null){
                interactionState = InteractionState.touchingAnnotation;
            }else{
                interactionState = InteractionState.touchingEmpty;
            }
        }else{
            interactionState = InteractionState.touchingEmpty;
        }
    }

    private void OnTouchEnded(Vector2 tapPosition){
        //todo: rewrite this as a state machine
        lastTouchPosition = tapPosition;

        Ray ray = arCamera.ScreenPointToRay(tapPosition);
        RaycastHit hitObject;
        if(Physics.Raycast(ray, out hitObject))
        {
            if(selectables.Contains(hitObject.transform))
            {
                if (selectables.IndexOf(hitObject.transform) == newSelectableTouchedIndex  && interactionState == InteractionState.touchingNew) 
                {
                    SelectItem(newSelectableTouchedIndex);
                }
            }
            else if(interactionState == InteractionState.touchingAnnotation && hitObject.transform.GetComponent<ARAnnotation>() != null)
            {
                MoveSelectedItemToAnnotationView(hitObject.transform.GetComponent<ARAnnotation>());
            }
            else if(interactionState == InteractionState.touchingEmpty && itemSelected)
            {
                if (ARAnnotation.IsFocused()){
                    MoveSelectedItemToCameraView();
                }else{
                    DeselectItem(selectedIndex);
                }
            }
        }
        else if (interactionState == InteractionState.touchingEmpty && itemSelected)
        {
            if (ARAnnotation.IsFocused()){
                MoveSelectedItemToCameraView();
            }else{
                DeselectItem(selectedIndex);
            }
        }
        

        interactionState = InteractionState.notTouching;
    }

    private void OnTouchMoved(Vector2 tapPosition){
        Vector2 delta = tapPosition - lastTouchPosition;
        lastTouchPosition = tapPosition;

        if (interactionState == InteractionState.grabbingSelected){
            Transform selectedItem = selectables[selectedIndex];
            selectedItem.RotateAround(selectedItem.position, Vector3.up, -delta.x/5);
            selectedItem.RotateAround(selectedItem.position, arCamera.transform.right, delta.y/5);
        }
    }

    private void OnTwoTouches(Vector2 touchOne, Vector2 touchTwo){
        //get the distance, angle, and average position of the two touches
        float newTwoFingerDistance = Vector2.Distance(touchOne, touchTwo);
        float newTwoFingerAngle = Vector2.Angle(new Vector2(0, 1), touchOne-touchTwo)*Mathf.Sign(touchOne.x - touchTwo.x);
        Vector2 newTwoFingerAveragePosition = (touchOne + touchTwo)/2f;

        //if we're just starting, set up
        if (interactionState != InteractionState.twoFingersTouching){
            interactionState = InteractionState.twoFingersTouching;
            lastTwoFingerDistance = newTwoFingerDistance;
            lastTwoFingerAngle = newTwoFingerAngle;
            lastTwoFingerAveragePosition = newTwoFingerAveragePosition;
        //else, the fingers might have moved
        }else{
            //how much have they moved?
            float distanceDelta = lastTwoFingerDistance - newTwoFingerDistance;
            float angleDelta = lastTwoFingerAngle - newTwoFingerAngle;
            Vector2 averagePositionDelta = lastTwoFingerAveragePosition - newTwoFingerAveragePosition;

            //move Z by distanceDelta
            //rotate Z by angleDelta
            //move XY by averagePositionDelta
            if (itemSelected){
                Transform selectedItem = selectables[selectedIndex];
                Vector3 cameraToSelected = (arCamera.transform.position - selectedItem.position);

                //rotate on camera axis
                selectedItem.Rotate(selectedItem.InverseTransformDirection(arCamera.transform.forward), angleDelta);
                //push pull towards and away from camera
                selectedItem.position -= (cameraToSelected)*distanceDelta*0.001f;
                //move xy
                selectedItem.position -= arCamera.transform.right * averagePositionDelta.x * 0.001f;
                selectedItem.position -= Vector3.up * averagePositionDelta.y * 0.001f;
            }

            //store the most recent inputs for next frame
            lastTwoFingerAngle = newTwoFingerAngle;
            lastTwoFingerDistance = newTwoFingerDistance;
            lastTwoFingerAveragePosition = newTwoFingerAveragePosition;
        }
    }

    //take the selected guitar and move it to the selected guitar location
    private void SelectItem(int index){
        //deselect the previous selected guitar
        if (itemSelected){
            if (index != selectedIndex){
                DeselectItem(selectedIndex);
            }
        }

        //if no guitar was selected or a different guitar was selected select the new guitar
        //otherwise do nothing (that means we have selected the same guitar)
        if ((!itemSelected) || (index != selectedIndex)){

            itemSelected = true;
            selectedIndex = index;
            ARSelectable selectedItem = selectables[selectedIndex].GetComponent<ARSelectable>();
            selectedItem.Select();
            
            MoveSelectedItemToCameraView();
            PlaySelectedItemAudio();
        }

    }

    private void PlaySelectedItemAudio(){
        AudioSource audioSource = selectables[selectedIndex].GetComponent<AudioSource>();
        if (audioSource != null){
            audioSource.Play();
        }
    }

    private void DeselectItem(int index){
        itemSelected = false;
        ARSelectable selectedItem = selectables[selectedIndex].GetComponent<ARSelectable>();
        selectedItem.Deselect();

        selectedItem.transform.parent = selectedItem.homeParent;

        Transform homeTarget = new GameObject("homeTarget").transform;
        homeTarget.position = selectedItem.homePosition;
        homeTarget.rotation = Quaternion.Euler(selectedItem.homeRotation);

        AnimateTransform animator = selectables[selectedIndex].gameObject.AddComponent<AnimateTransform>(); //animate the guitar body to the selected guitar location
        animator.Configure (homeTarget, 1f, curveForTransitions);
        animator.OnComplete += () => {Destroy(homeTarget.gameObject);};
    }

    private void MoveSelectedItemToAnnotationView(ARAnnotation annotation){
        Transform selectedItem = selectables[selectedIndex];


        //move the target location to 1 meter in front of the camera
        selectedTargetLocation.position = arCamera.transform.position + (arCamera.transform.forward * 1f);
        //rotate it to look at the camera...had to flip the rotation because of the guitar's rotation
        selectedTargetLocation.rotation = arCamera.transform.rotation;



        //move the target location to within the annotation focus distance - in front of the camera
        selectedTargetLocation.position = arCamera.transform.position + (arCamera.transform.forward * annotation.maxFocusDistance * 0.9f) ;
        //adjust for the annotation's position relative to the item
        selectedTargetLocation.position += selectedTargetLocation.TransformDirection(selectedItem.InverseTransformDirection(selectedItem.position - annotation.transform.position));

        AnimateTransform animator = selectedItem.gameObject.AddComponent<AnimateTransform>(); //animate the guitar body to the focused location
        animator.Configure(selectedTargetLocation, 1f, curveForTransitions);
    }

    private void MoveSelectedItemToCameraView(){
        Transform selectedItem = selectables[selectedIndex];

        //move the selected guitar location to 1 meter in front of the camera
        selectedTargetLocation.position = arCamera.transform.position + (arCamera.transform.forward * 1f);
        //rotate it to look at the camera...had to flip the rotation because of the guitar's rotation
        selectedTargetLocation.rotation = arCamera.transform.rotation;

        Transform itemBody = selectedItem.transform;

        AnimateTransform animator = itemBody.gameObject.AddComponent<AnimateTransform>(); //animate the guitar body to the selected guitar location
        animator.Configure(selectedTargetLocation, 1f, curveForTransitions);
        animator.OnComplete += () => {itemBody.parent = Camera.main.transform;}; //when the animation is complete, parent the guitar body to the camera
        
    }

    private void SwipeAction_OnSwipe(SwipeData data)
    {
        //disabling while I get rotations working
        if (itemSelected && interactionState != InteractionState.grabbingSelected){
            if (data.Direction == SwipeDirection.Left){
                int nextGuitar = selectedIndex - 1;
                nextGuitar = Mathf.Max(0, nextGuitar);
                //SelectGuitar(nextGuitar);
            }
            else if (data.Direction == SwipeDirection.Right){
                int nextGuitar = selectedIndex + 1;
                nextGuitar = Mathf.Min(selectables.Count -1, nextGuitar);
                //SelectGuitar(nextGuitar);
            }
        }
    }


}
