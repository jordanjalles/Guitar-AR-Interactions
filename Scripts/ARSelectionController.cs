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
    private bool selected = false;
    private int selectedIndex = -1;
    private int newSelectableTouchedIndex;
    private Vector2 lastTouchPosition;

    enum InteractionState { touchingNew, grabbingSelected, touchingEmpty, notTouching, twoFingersTouching};
    private InteractionState interactionState = InteractionState.notTouching;

    private void Awake()
    {
        SwipeDetector.OnSwipe += SwipeAction_OnSwipe;
        BasicInputDetector.OnTouchBegan += OnTouchBegan;
        BasicInputDetector.OnTouchEnded += OnTouchEnded;
        BasicInputDetector.OnTouchMoved += OnTouchMoved;

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
                if (selectedIndex == newSelectableTouchedIndex && selected)
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
                    SelectGuitar(newSelectableTouchedIndex);
                }
            }
        }else{
            //if the touch began and ended on empty space, deselect the previously selected guitar
            if (interactionState == InteractionState.touchingEmpty && selected)
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

    //take the selected guitar and move it to the selected guitar location
    private void SelectGuitar(int index){
        //deselect the previous selected guitar
        if (selected){
            if (index != selectedIndex){
                DeselectItem(selectedIndex);
            }
        }

        //if no guitar was selected or a different guitar was selected select the new guitar
        //otherwise do nothing (that means we have selected the same guitar)
        if ((!selected) || (index != selectedIndex)){

            selected = true;
            selectedIndex = index;
            
            //move the selected guitar location to 1 meter in front of the camera
            selectedTargetLocation.position = arCamera.transform.position + (arCamera.transform.forward * 1f);
            //rotate it to look at the camera...had to flip the rotation because of the guitar's rotation
            selectedTargetLocation.LookAt(arCamera.transform.position); 
            selectedTargetLocation.Rotate(Vector3.up, 180);

            Transform itemBody = selectables[selectedIndex];

            AnimateTransform animator = itemBody.gameObject.AddComponent(typeof(AnimateTransform)) as AnimateTransform; //animate the guitar body to the selected guitar location
            animator.Configure(selectedTargetLocation.position, selectedTargetLocation.rotation.eulerAngles, 1f, curveForTransitions);

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
        selected = false;
        AnimateTransform animator = selectables[selectedIndex].gameObject.AddComponent(typeof(AnimateTransform)) as AnimateTransform; //animate the guitar body to the selected guitar location
        ARSelectable selectedItem = selectables[selectedIndex].GetComponent<ARSelectable>();
        animator.Configure (selectedItem.homePosition, selectedItem.homeRotation, 1f, curveForTransitions);
    }

    private void SwipeAction_OnSwipe(SwipeData data)
    {
        //disabling while I get rotations working
        if (selected && interactionState != InteractionState.grabbingSelected){
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
