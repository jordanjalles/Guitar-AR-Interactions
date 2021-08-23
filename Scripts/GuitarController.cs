using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class GuitarController : MonoBehaviour
{
    [SerializeField]
    private Transform selectedGuitarLocation;
    
    [SerializeField]
    private AnimationCurve curveForGuitarTransitions;

    [SerializeField]
    private Camera arCamera;

    private List<Transform> guitars = new List<Transform>();
    private bool guitarSelected = false;
    private int selectedGuitarIndex = -1;
    private int newGuitarTouchedIndex;
    private Vector3 selectedGuitarCenterOfVolume;
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
            guitars.Add(child.transform);
            Debug.Log("Guitar found: " + child.name);
        }
    }

    private void OnTouchBegan(Vector2 tapPosition)
    {
        lastTouchPosition = tapPosition;

        Ray ray = arCamera.ScreenPointToRay(tapPosition);
        RaycastHit hitObject;
        if(Physics.Raycast(ray, out hitObject))
        {
            if(guitars.Contains(hitObject.transform)){ 
                newGuitarTouchedIndex = guitars.IndexOf(hitObject.transform);
                if (selectedGuitarIndex == newGuitarTouchedIndex && guitarSelected)
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
            if(guitars.Contains(hitObject.transform) && interactionState == InteractionState.touchingNew)
            {
                if (guitars.IndexOf(hitObject.transform) == newGuitarTouchedIndex) 
                {
                    SelectGuitar(newGuitarTouchedIndex);
                }
            }
        }else{
            if (interactionState == InteractionState.touchingEmpty && guitarSelected)
            {
                //deselect selected guitar
                DeselectGuitar(selectedGuitarIndex);
            }
        }

        interactionState = InteractionState.notTouching;
    }

    private void OnTouchMoved(Vector2 tapPosition){
        Vector2 delta = tapPosition - lastTouchPosition;
        lastTouchPosition = tapPosition;

        if (interactionState == InteractionState.grabbingSelected){
            Transform selectedGuitar = guitars[selectedGuitarIndex];
            selectedGuitar.RotateAround(selectedGuitar.position, Vector3.up, -delta.x/5);
            selectedGuitar.RotateAround(selectedGuitar.position, arCamera.transform.right, delta.y/5);
        }
    }

    //take the selected guitar and move it to the selected guitar location
    private void SelectGuitar(int index){
        if (guitarSelected){
            if (index != selectedGuitarIndex){
                DeselectGuitar(selectedGuitarIndex);
            }
        }

        if ((!guitarSelected) || (index != selectedGuitarIndex)){

            guitarSelected = true;
            selectedGuitarIndex = index;
            
            //move the selected guitar location to 1 meter in front of the camera
            selectedGuitarLocation.position = arCamera.transform.position + (arCamera.transform.forward * 1f);
            //rotate it to look at the camera...had to flip the rotation because of the guitar's rotation
            selectedGuitarLocation.LookAt(arCamera.transform.position); 
            selectedGuitarLocation.Rotate(Vector3.up, 180);

            Transform guitarBody = guitars[selectedGuitarIndex];

            AnimateTransform animator = guitarBody.gameObject.AddComponent(typeof(AnimateTransform)) as AnimateTransform; //animate the guitar body to the selected guitar location
            animator.Configure(selectedGuitarLocation.position, selectedGuitarLocation.rotation.eulerAngles, 1f, curveForGuitarTransitions);

            PlaySelectedGuitarAudio();
        }

    }

    private void PlaySelectedGuitarAudio(){
        AudioSource audioSource = guitars[selectedGuitarIndex].GetComponent<AudioSource>();
        if (audioSource != null){
            audioSource.Play();
        }
    }

    private void DeselectGuitar(int index){
        guitarSelected = false;
        AnimateTransform animator = guitars[selectedGuitarIndex].gameObject.AddComponent(typeof(AnimateTransform)) as AnimateTransform; //animate the guitar body to the selected guitar location
        ARSelectable selectedGuitar = guitars[selectedGuitarIndex].GetComponent<ARSelectable>();
        animator.Configure(selectedGuitar.homePosition, selectedGuitar.homeRotation, 1f, curveForGuitarTransitions);
    }

    private void SwipeAction_OnSwipe(SwipeData data)
    {
        //disabling while we get rotations working
        if (guitarSelected && interactionState != InteractionState.grabbingSelected){
            if (data.Direction == SwipeDirection.Left){
                int nextGuitar = selectedGuitarIndex - 1;
                nextGuitar = Mathf.Max(0, nextGuitar);
                //SelectGuitar(nextGuitar);
            }
            else if (data.Direction == SwipeDirection.Right){
                int nextGuitar = selectedGuitarIndex + 1;
                nextGuitar = Mathf.Min(guitars.Count -1, nextGuitar);
                //SelectGuitar(nextGuitar);
            }
        }
    }


}
