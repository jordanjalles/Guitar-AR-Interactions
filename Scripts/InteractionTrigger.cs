using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    //type of interaction that will trigger 
    public enum InteractionType {Tap, Drag, DragTwo, Rotate, Pinch, Swipe};
    //defaults to tap
    public InteractionType interactionType = InteractionType.Tap;
    
    //applies for Tap and Drag
    public enum InteractionTarget {Self, NotSelf, Any, Empty};
    //defaults to self
    public InteractionTarget interactionTarget = InteractionTarget.Self; 

    public BasicTransformAction actionFromEditor; //todo: add this to events if not null
    
    private Camera mainCamera;
    private Vector2 lastTouchPosition;

    private float lastTwoFingerDistance;
    private float lastTwoFingerAngle;
    private Vector2 lastTwoFingerAveragePosition;

    //enum for interaction state 
    private bool touchBeganOnTarget = false;
    private bool twoFingersTouching = false;

    private void Awake()
    {
        if (interactionType == InteractionType.Tap){
            BasicInputDetector.OnTouchBegan += OnTouchBegan;
            BasicInputDetector.OnTouchEnded += OnTouchEnded;
        }
        else if (interactionType == InteractionType.Drag){
            BasicInputDetector.OnTouchBegan += OnTouchBegan;
            BasicInputDetector.OnTouchMoved += OnTouchMoved;
            BasicInputDetector.OnTouchEnded += OnTouchEnded;
        }
        else if (interactionType == InteractionType.DragTwo || interactionType == InteractionType.Rotate || interactionType == InteractionType.Pinch){
            BasicInputDetector.OnTwoTouches += OnTwoTouches;
        }
        else if (interactionType == InteractionType.Swipe){
            //SwipeDetector.OnSwipe += OnSwipe;
        }
        mainCamera = Camera.main;
    }

    private void OnDestroy(){
        if (interactionType == InteractionType.Tap){
            BasicInputDetector.OnTouchBegan -= OnTouchBegan;
            BasicInputDetector.OnTouchEnded -= OnTouchEnded;
        }
        else if (interactionType == InteractionType.Drag){
            BasicInputDetector.OnTouchBegan -= OnTouchBegan;
            BasicInputDetector.OnTouchMoved -= OnTouchMoved;
            BasicInputDetector.OnTouchEnded -= OnTouchEnded;
        }
        else if (interactionType == InteractionType.DragTwo || interactionType == InteractionType.Rotate || interactionType == InteractionType.Pinch){
            BasicInputDetector.OnTwoTouches -= OnTwoTouches;
        }
        else if (interactionType == InteractionType.Swipe){
            //SwipeDetector.OnSwipe -= OnSwipe;
        }
    }


    private void OnTouchBegan(Vector2 tapPosition)
    {
        lastTouchPosition = tapPosition;

        Ray ray = mainCamera.ScreenPointToRay(tapPosition);
        RaycastHit hitObject;
        if(Physics.Raycast(ray, out hitObject))
        {
            if (interactionTarget == InteractionTarget.Self){
                if (hitObject.transform == transform){
                    touchBeganOnTarget = true;
                }else{
                    touchBeganOnTarget = false;
                }
            }
            else if (interactionTarget == InteractionTarget.NotSelf){
                if (hitObject.transform != transform){
                    touchBeganOnTarget = true;
                }else{
                    touchBeganOnTarget = false;
                }
            }
            else if (interactionTarget == InteractionTarget.Any){
                touchBeganOnTarget = true;
            } 
        } else if (interactionTarget == InteractionTarget.Empty){
            touchBeganOnTarget = true;
        } else {
            touchBeganOnTarget = false;
        }
    }

    private void OnTouchEnded(Vector2 tapPosition){
        lastTouchPosition = tapPosition;

        Ray ray = mainCamera.ScreenPointToRay(tapPosition);
        RaycastHit hitObject;
        if(Physics.Raycast(ray, out hitObject))
        {
            if (interactionTarget == InteractionTarget.Self){
                if (hitObject.transform == transform && touchBeganOnTarget){
                    OnTap();
                }
            }
            else if (interactionTarget == InteractionTarget.NotSelf && touchBeganOnTarget){
                if (hitObject.transform != transform){
                    OnTap();
                }
            }
            else if (interactionTarget == InteractionTarget.Any){
                OnTap();
            }
        } else if (interactionTarget == InteractionTarget.Empty && touchBeganOnTarget){
            OnTap();
        }
    }

    private void OnTouchMoved(Vector2 tapPosition){
        Vector2 touchPositionDelta = tapPosition - lastTouchPosition;
        lastTouchPosition = tapPosition;

        if (interactionType == InteractionType.Drag && touchBeganOnTarget){
            //todo: call drag handler with touchPositionDelta
            OnDrag(touchPositionDelta);
        }
    }

    private void OnTwoTouches(Vector2 touchOne, Vector2 touchTwo){
        //get the distance, angle, and average position of the two touches
        float newTwoFingerDistance = Vector2.Distance(touchOne, touchTwo);
        float newTwoFingerAngle = Vector2.Angle(new Vector2(0, 1), touchOne-touchTwo)*Mathf.Sign(touchOne.x - touchTwo.x);
        Vector2 newTwoFingerAveragePosition = (touchOne + touchTwo)/2f;

        //if we're just starting, set up
        if (twoFingersTouching == false){
            twoFingersTouching = true;
            lastTwoFingerDistance = newTwoFingerDistance;
            lastTwoFingerAngle = newTwoFingerAngle;
            lastTwoFingerAveragePosition = newTwoFingerAveragePosition;
        //else, the fingers might have moved
        }else{
            //how much have they moved?
            float distanceDelta = lastTwoFingerDistance - newTwoFingerDistance;
            float angleDelta = lastTwoFingerAngle - newTwoFingerAngle;
            Vector2 averagePositionDelta = lastTwoFingerAveragePosition - newTwoFingerAveragePosition;

            Vector3 cameraToSelected = (mainCamera.transform.position - transform.position);

            //todo, turn these into delegates
            if (interactionType == InteractionType.Rotate){
                OnRotate(angleDelta);
            }
            if (interactionType == InteractionType.Pinch){
                OnPinch(distanceDelta);
            }
            if (interactionType == InteractionType.DragTwo){
                OnDragTwo(averagePositionDelta);
            }

            //store the most recent inputs for next frame
            lastTwoFingerAngle = newTwoFingerAngle;
            lastTwoFingerDistance = newTwoFingerDistance;
            lastTwoFingerAveragePosition = newTwoFingerAveragePosition;
        }
    }

    public delegate void DelegateVoidEvent();
    public DelegateVoidEvent OnTap = () => {};

    public delegate void DelegateFloatEvent(float delta);
    public DelegateFloatEvent OnPinch = (float delta) => {};
    public DelegateFloatEvent OnRotate = (float delta) => {};

    public delegate void DelegateVector2Event(Vector2 delta);
    public DelegateVector2Event OnDrag = (Vector2 delta) => {};
    public DelegateVector2Event OnDragTwo = (Vector2 delta) => {};

    /*
    public delegate void DelegateSwipeEvent(SwipeData data);
    public DelegateSwipeEvent OnSwipe = (SwipeData data) => {};
    */
}


