using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;



public class GuitarController : MonoBehaviour
{
    
    private List<GameObject> guitars = new List<GameObject>();
    
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
        foreach (Transform child in transform){
            if (child.name.Split(' ')[0] == "Guitar"){
                guitars.Add(child.gameObject);
            }
        }
    }




    //tap action to select a guitar
    private void TapAction_OnTap(Vector2 tapPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(tapPosition);
        RaycastHit hitObject;
        if(Physics.Raycast(ray, out hitObject))
        {
            if(guitars.Contains(hitObject.transform.parent.gameObject)){
                SelectGuitar(guitars.IndexOf(hitObject.transform.parent.gameObject));
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
            GameObject guitarBody = GetGuitarBody(selectedGuitarIndex);
            selectedGuitarInitialLocation.position = guitarBody.transform.position; //save the initial location
            selectedGuitarInitialLocation.rotation = guitarBody.transform.rotation; //save the initial rotation
            
            AnimateTransform animator = guitarBody.AddComponent(typeof(AnimateTransform)) as AnimateTransform; //animate the guitar body to the selected guitar location
            animator.Configure(selectedGuitarLocation, 1f, curveForGuitarTransitions);
            //todo animate this ^
        }

    }

    private void DeselectGuitar(int index){

        guitarSelected = false;
        AnimateTransform animator = GetGuitarBody(selectedGuitarIndex).AddComponent(typeof(AnimateTransform)) as AnimateTransform; //animate the guitar body to the selected guitar location
        animator.Configure(selectedGuitarInitialLocation, 1f, curveForGuitarTransitions);
    }

    private GameObject GetGuitarBody(int index){
        //guitar is the child of the guitar transform not named stand
        foreach (Transform child in guitars[index].transform){
            if (child.name != "Stand"){
                return child.gameObject;
            }
        }
        Debug.LogError("Could not find guitar body");
        return null;

    }




    private void SwipeAction_OnSwipe(SwipeData data)
    {
        if (data.Direction == SwipeDirection.Left){

 
        }


        else if (data.Direction == SwipeDirection.Right){


        }
    }

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

}
