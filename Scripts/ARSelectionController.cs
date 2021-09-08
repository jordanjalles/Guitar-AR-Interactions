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

    enum InteractionState { touchingNew, grabbingSelected, touchingEmpty, notTouching, twoFingersTouching};
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
            }else{
                interactionState = InteractionState.touchingEmpty;
            }
        }else{
            interactionState = InteractionState.touchingEmpty;
        }
    }

    private void OnTouchEnded(Vector2 tapPosition){
        lastTouchPosition = tapPosition;

        Ray ray = arCamera.ScreenPointToRay(tapPosition);
        RaycastHit hitObject;
        if(Physics.Raycast(ray, out hitObject))
        {
            //if the touch began and ended on a guitar, select it 
            if(selectables.Contains(hitObject.transform) && interactionState == InteractionState.touchingNew)
            {
                if (selectables.IndexOf(hitObject.transform) == newSelectableTouchedIndex) 
                {
                    SelectItem(newSelectableTouchedIndex);
                }
            }
        }else{
            //if the touch began and ended on empty space, deselect the previously selected guitar
            if (interactionState == InteractionState.touchingEmpty && itemSelected)
            {
                //deselect selected guitar
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
            
            //move the selected guitar location to 0.75 meter in front of the camera
            selectedTargetLocation.position = arCamera.transform.position + (arCamera.transform.forward * 0.75f);
            //rotate it to look at the camera...had to flip the rotation because of the guitar's rotation
            selectedTargetLocation.LookAt(arCamera.transform.position); 
            selectedTargetLocation.Rotate(Vector3.up, 180);

            Transform itemBody = selectedItem.transform;

            AnimateTransform animator = itemBody.gameObject.AddComponent<AnimateTransform>(); //animate the guitar body to the selected guitar location
            animator.Configure(selectedTargetLocation.position, selectedTargetLocation.rotation.eulerAngles, 1f, curveForTransitions);
            
            //might use this to change selected item parent to the camera, and back to the previous parent on deselect
            animator.OnComplete = () => {Debug.Log("Delegate function called!");};
            
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

        AnimateTransform animator = selectables[selectedIndex].gameObject.AddComponent<AnimateTransform>(); //animate the guitar body to the selected guitar location
        animator.Configure (selectedItem.homePosition, selectedItem.homeRotation, 1f, curveForTransitions);
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
