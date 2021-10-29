using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    [SerializeField]
    public new string name = "default";

    //type of interaction that will trigger 
    public enum InteractionType {Tap, Drag, DragTwo, DragX, DragY, DragTwoX, DragTwoY, Rotate, Pinch, Swipe};
    //defaults to tap
    public InteractionType interactionType = InteractionType.Tap;
    
    //applies for Tap and Drag
    public enum InteractionTarget {Self, NotSelf, Any, Empty};
    //defaults to self
    public InteractionTarget interactionTarget = InteractionTarget.Self; 

    public BasicTransformAction linkedAction; //todo: add this to events if not null
    
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
        Debug.Log("InteractionTrigger Awake: " + interactionType);

        BasicInputDetector.OnTouchBegan += OnTouchBegan;
        BasicInputDetector.OnTouchMoved += OnTouchMoved;
        BasicInputDetector.OnTouchEnded += OnTouchEnded;

        BasicInputDetector.OnTwoTouches += OnTwoTouches;

        if (interactionType == InteractionType.Swipe){
            //SwipeDetector.OnSwipe += OnSwipe;
        }
        mainCamera = Camera.main;

        if (linkedAction != null){
            SetUpLinkedAction();
        }
    }

    private void OnDestroy(){
        BasicInputDetector.OnTouchBegan -= OnTouchBegan;
        BasicInputDetector.OnTouchMoved -= OnTouchMoved;
        BasicInputDetector.OnTouchEnded -= OnTouchEnded;
    
        BasicInputDetector.OnTwoTouches -= OnTwoTouches;
        
        if (interactionType == InteractionType.Swipe){
            //SwipeDetector.OnSwipe -= OnSwipe;
        }
    }

    private void OnTouchBegan(Vector2 tapPosition)
    {
        lastTouchPosition = tapPosition;
        touchBeganOnTarget = TouchOnInteractionTarget(tapPosition); 
    }

    private void OnTouchEnded(Vector2 tapPosition){
        lastTouchPosition = tapPosition;

        if (interactionType == InteractionType.Tap){
            //if touch began and ended on target, it's a tap
            if (touchBeganOnTarget && TouchOnInteractionTarget(tapPosition)){
                if (linkedAction != null){
                    linkedAction.Activate(1f);
                }
                OnTap();
            }
        }
    }

    private void OnTouchMoved(Vector2 tapPosition){
        Debug.Log("touch moved");

        Vector2 touchPositionDelta = tapPosition - lastTouchPosition;
        lastTouchPosition = tapPosition;

        if (interactionType == InteractionType.Drag && touchBeganOnTarget){
            //todo: call drag handler with touchPositionDelta
            OnDrag(touchPositionDelta);
        }
        if (interactionType == InteractionType.DragX && touchBeganOnTarget){
            OnDragX(touchPositionDelta.x);
        }
        if (interactionType == InteractionType.DragY && touchBeganOnTarget){
            OnDragY(touchPositionDelta.y);
        }
    }

    private bool TouchOnInteractionTarget(Vector2 tapPosition){
        Ray ray = mainCamera.ScreenPointToRay(tapPosition);
        RaycastHit hitObject;

        if (interactionTarget == InteractionTarget.Self){
            if(Physics.Raycast(ray, out hitObject))
            {
                if (hitObject.transform == transform || hitObject.transform.IsChildOf(transform)){
                    return true;
                }else{
                    return false;
                }
            }else{
                return false;
            }
        }

        if (interactionTarget == InteractionTarget.NotSelf){
            if(Physics.Raycast(ray, out hitObject))
            {
                if (hitObject.transform == transform || hitObject.transform.IsChildOf(transform)){
                    return false;
                }else{
                    return true;
                }
            }else{
                return true;
            }
        }

        if (interactionTarget == InteractionTarget.Empty){
            if(Physics.Raycast(ray, out hitObject))
            {
                return false;
            }else{
                return true;
            }
        } 
        
        if (interactionTarget == InteractionTarget.Any){
            return true;
        } 

        Debug.LogError("InteractionTarget not set");
        return false;
    }

    private void OnTwoTouches(Vector2 touchOne, Vector2 touchTwo){
        if (interactionType != InteractionType.Rotate 
            && interactionType != InteractionType.Pinch 
            && interactionType != InteractionType.DragTwo
            && interactionType != InteractionType.DragTwoX
            && interactionType != InteractionType.DragTwoY){
            return;
        }

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
            if (interactionType == InteractionType.DragTwoX){
                Debug.Log("DragTwoX: "+ averagePositionDelta.x);
                OnDragTwoX(averagePositionDelta.x);
            }
            if (interactionType == InteractionType.DragTwoY){
                OnDragTwoY(averagePositionDelta.y);
            }

            //store the most recent inputs for next frame
            lastTwoFingerAngle = newTwoFingerAngle;
            lastTwoFingerDistance = newTwoFingerDistance;
            lastTwoFingerAveragePosition = newTwoFingerAveragePosition;
        }
    }

    private void SetUpLinkedAction(){
        //switch on the interaction type
        switch (interactionType){
            case InteractionType.Tap:
                OnTap += () => {linkedAction.Activate(1f);};
                break;
            case InteractionType.Drag:
                Debug.LogError("Drag not compatible with basic transform actions, try DragX or DragY instead");
                break;
            case InteractionType.DragX:
                OnDragX += (float delta) => {linkedAction.Activate(delta);};
                break;
            case InteractionType.DragY:
                OnDragY += (float delta) => {linkedAction.Activate(delta);};
                break;
            case InteractionType.DragTwo:
                Debug.LogError("DragTwo not compatible with basic transform actions, try DragTwoX or DragTwoY instead");
                break;
            case InteractionType.DragTwoX:
                OnDragTwoX += (float delta) => {linkedAction.Activate(delta);};
                break;
            case InteractionType.DragTwoY:
                OnDragTwoY += (float delta) => {linkedAction.Activate(delta);};
                break;
            case InteractionType.Pinch:
                OnPinch += (float delta) => {linkedAction.Activate(delta);};
                break;
            case InteractionType.Rotate:
                OnRotate += (float delta) => {linkedAction.Activate(delta);};
                break;
            default:
                Debug.LogError("InteractionType not set");
                break;
        }
    }

    public delegate void DelegateVoidEvent();
    public DelegateVoidEvent OnTap = () => {};

    public delegate void DelegateFloatEvent(float delta);
    public DelegateFloatEvent OnPinch = (float delta) => {};
    public DelegateFloatEvent OnRotate = (float delta) => {};
    public DelegateFloatEvent OnDragX = (float delta) => {};
    public DelegateFloatEvent OnDragY = (float delta) => {};
    public DelegateFloatEvent OnDragTwoX = (float delta) => {};
    public DelegateFloatEvent OnDragTwoY = (float delta) => {};

    public delegate void DelegateVector2Event(Vector2 delta);
    public DelegateVector2Event OnDrag = (Vector2 delta) => {};
    public DelegateVector2Event OnDragTwo = (Vector2 delta) => {};

    /*
    public delegate void DelegateSwipeEvent(SwipeData data);
    public DelegateSwipeEvent OnSwipe = (SwipeData data) => {};
    */
}


