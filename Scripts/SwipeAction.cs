using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeAction : MonoBehaviour
{
    private List<GameObject> guitars;
    
    [SerializeField]
    private Transform SelectedLocation;

    private void Awake()
    {
        //add our swipe action to the swipe detector
        SwipeDetector.OnSwipe += SwipeAction_OnSwipe;



    }

    private void SwipeAction_OnSwipe(SwipeData data)
    {
        if (data.Direction == SwipeDirection.Left){

 
        }


        else if (data.Direction == SwipeDirection.Right){


        }
    }
}
