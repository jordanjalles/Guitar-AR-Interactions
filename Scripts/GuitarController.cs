using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;



public class GuitarController : MonoBehaviour
{
    
    private List<Transform> guitars = new List<Transform>();
    
    [SerializeField]
    private Transform selectedGuitarLocation;
    private Transform selectedGuitarInitialLocation;

    private bool guitarSelected = false;
    private int selectedGuitarIndex;

    [SerializeField]
    private AnimationCurve curveForGuitarTransitions;


    [SerializeField]
    private Camera arCamera;

    private void Awake()
    {
        SwipeDetector.OnSwipe += SwipeAction_OnSwipe;
        TapDetector.OnTap += TapAction_OnTap;

        selectedGuitarInitialLocation = new GameObject().transform;

        //get the child guitars
        foreach (ARSelectable child in GetComponentsInChildren(typeof(ARSelectable))){
            guitars.Add(child.transform);
            Debug.Log("Guitar found: " + child.name);
        }
    }




    //tap action to select a guitar
    private void TapAction_OnTap(Vector2 tapPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(tapPosition);
        RaycastHit hitObject;
        if(Physics.Raycast(ray, out hitObject))
        {
            if(guitars.Contains(hitObject.transform)){
                SelectGuitar(guitars.IndexOf(hitObject.transform));
            }
        }
        else{
            //deselect selected guitar
            if (guitarSelected){
                DeselectGuitar(selectedGuitarIndex);
            }
        }
    }


    //take the selected guitar and move it to the selected guitar location
    private void SelectGuitar(int index){
        if (guitarSelected){
            if (index != selectedGuitarIndex){
                DeselectGuitar(selectedGuitarIndex);
            }
        }

        //if we are selecting a new guitar
        if (index != selectedGuitarIndex){
            guitarSelected = true;
            selectedGuitarIndex = index;
            Transform guitarBody = guitars[selectedGuitarIndex];
            selectedGuitarInitialLocation.position = guitarBody.position; //save the initial location
            selectedGuitarInitialLocation.rotation = guitarBody.rotation; //save the initial rotation
            
            AnimateTransform animator = guitarBody.gameObject.AddComponent(typeof(AnimateTransform)) as AnimateTransform; //animate the guitar body to the selected guitar location
            animator.Configure(selectedGuitarLocation, 1f, curveForGuitarTransitions);
        }

    }

    private void DeselectGuitar(int index){

        guitarSelected = false;
        AnimateTransform animator = guitars[selectedGuitarIndex].gameObject.AddComponent(typeof(AnimateTransform)) as AnimateTransform; //animate the guitar body to the selected guitar location
        animator.Configure(selectedGuitarInitialLocation, 1f, curveForGuitarTransitions);
    }




    private void SwipeAction_OnSwipe(SwipeData data)
    {
        if (guitarSelected){
            if (data.Direction == SwipeDirection.Left){
                int nextGuitar = selectedGuitarIndex - 1;
                nextGuitar = Mathf.Max(0, nextGuitar);
                SelectGuitar(nextGuitar);
            }


            else if (data.Direction == SwipeDirection.Right){
                int nextGuitar = selectedGuitarIndex + 1;
                nextGuitar = Mathf.Min(guitars.Count -1, nextGuitar);
                SelectGuitar(nextGuitar);
            }
        }
    }


}
