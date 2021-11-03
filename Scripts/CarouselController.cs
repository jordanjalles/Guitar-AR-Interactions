using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarouselController : MonoBehaviour
{
    private Transform selectedTargetLocation;
    
    [SerializeField]
    private AnimationCurve curveForTransitions;

    [SerializeField]
    private float transitionTime;

    [SerializeField]
    private Camera camera;

    private List<Transform> items = new List<Transform>();

    public Vector3 itemOffsetPositionPerIndex;
    public Vector3 itemOffsetRotationPerIndex;
    public float targetDistance;

    public int centerIndex;


    enum InteractionState { touchingNew, grabbingSelected, touchingEmpty, notTouching, twoFingersTouching, touchingAnnotation};
    private InteractionState interactionState = InteractionState.notTouching;

    private void Awake()
    {
        
        SwipeDetector.OnSwipe += SwipeAction_OnSwipe;
        //BasicInputDetector.OnTouchBegan += OnTouchBegan;
        //BasicInputDetector.OnTouchEnded += OnTouchEnded;
        //BasicInputDetector.OnTouchMoved += OnTouchMoved;
        //BasicInputDetector.OnTwoTouches += OnTwoTouches;

        //get the child guitars
        foreach (CarouselItem child in GetComponentsInChildren(typeof(CarouselItem))){
            items.Add(child.transform);
            Debug.Log("CarouselItem found: " + child.name);
        }

        selectedTargetLocation = new GameObject().transform;
        selectedTargetLocation.position = camera.transform.position + camera.transform.forward * targetDistance;
        selectedTargetLocation.rotation = camera.transform.rotation;

        UpdateCarouselItemTransforms(centerIndex);
    }


    private void UpdateCarouselItemTransforms(int centerIndex){
        for (int i = 0; i < items.Count; i++){
            Transform targetTransform = new GameObject("Animation target transform").transform;
            Vector3 targetPosition = Vector3.zero;
            targetPosition.x = selectedTargetLocation.position.x + itemOffsetPositionPerIndex.x * (i - centerIndex);
            targetPosition.y = selectedTargetLocation.position.y + itemOffsetPositionPerIndex.y * Mathf.Abs(i - centerIndex);
            targetPosition.z = selectedTargetLocation.position.z + itemOffsetPositionPerIndex.z * Mathf.Abs(i - centerIndex);

            targetTransform.position = targetPosition;
            Vector3 targetRotation = Vector3.zero;
            targetRotation.x = selectedTargetLocation.rotation.x + itemOffsetRotationPerIndex.x * Mathf.Abs(i - centerIndex);
            targetRotation.y = selectedTargetLocation.rotation.y + itemOffsetRotationPerIndex.y * (i - centerIndex);
            targetRotation.z = selectedTargetLocation.rotation.z + itemOffsetRotationPerIndex.z * Mathf.Abs(i - centerIndex);

            targetTransform.rotation = selectedTargetLocation.rotation * Quaternion.Euler(targetRotation);
            AnimateItemToTransform(items[i], targetTransform);
        }
    }

    private void AnimateItemToTransform(Transform item, Transform targetTransform, bool destroyTargetTransform = true){        
        AnimateTransform animator = item.gameObject.AddComponent<AnimateTransform>(); //animate the guitar body to the focused location
        animator.Configure(targetTransform, transitionTime, curveForTransitions);

        if (destroyTargetTransform){
            animator.OnComplete += () => {Destroy(targetTransform.gameObject);};
        }
    }

    private void SwipeAction_OnSwipe(SwipeData data)
    {
        if (data.Direction == SwipeDirection.Left){
            centerIndex = Mathf.Min(centerIndex + 1, items.Count - 1);
            UpdateCarouselItemTransforms(centerIndex);
        }
        else if (data.Direction == SwipeDirection.Right){
            centerIndex = Mathf.Max(centerIndex - 1, 0);
            UpdateCarouselItemTransforms(centerIndex);
        }
        
    }

    //take a string message and go to that carousel item
    public void GoToItem(string itemName){
        for (int i = 0; i < items.Count; i++){
            if (items[i].name == itemName){
                centerIndex = i;
                UpdateCarouselItemTransforms(centerIndex);
                break;
            }
        }
    }


}
