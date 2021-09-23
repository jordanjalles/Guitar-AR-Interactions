using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ARAnnotation : MonoBehaviour
{
    private Camera m_Camera;

    [SerializeField]
    private float maxVisibilityDistance;
    [SerializeField]

    public float maxFocusDistance;

    [SerializeField]
    private GameObject basicHintPrefab;
    [SerializeField]
    private GameObject basicActivePrefab;
    [SerializeField]
    private GameObject commentHintPrefab;
    [SerializeField]
    private GameObject commentActivePrefab;
    [SerializeField]
    private GameObject videoHintPrefab;
    [SerializeField]
    private GameObject videoActivePrefab;

    public Vector3 recommendedCameraRotation = Vector3.zero;
    
    

    private GameObject annotationMarker;
    private GameObject annotationHint;

    [SerializeField]
    private Texture2D annotationDetailTexture;

    //use this to detect conflicts with other ARAnnotations
    static ARAnnotation focusedAnnotation;

    private ARAnnotationDisplay annotationDisplay;

    //enum for types of annotations: basic, comment, video
    public enum AnnotationType { Basic, Comment, Video };

    public AnnotationType annotationType;


    //state enums
    enum State {hidden, focused, hint};
    State state;

    void Awake()
    {
        m_Camera = Camera.main;
        //instantiate the annotation game objects
        //if type is basic, instantiate basic prefabs
        //if type is comment, instantiate comment prefabs
        //if type is video, instantiate video prefabs
        if (annotationType == AnnotationType.Basic){
            annotationMarker = Instantiate(basicActivePrefab, transform.position,  Quaternion.identity, transform);
            annotationHint = Instantiate(basicHintPrefab, transform.position, Quaternion.identity, transform);
        }
        else if (annotationType == AnnotationType.Comment){
            annotationMarker = Instantiate(commentActivePrefab, transform.position, Quaternion.identity, transform);
            annotationHint = Instantiate(commentHintPrefab, transform.position, Quaternion.identity, transform);
        }
        else if (annotationType == AnnotationType.Video){
            annotationMarker = Instantiate(videoActivePrefab, transform.position, Quaternion.identity, transform);
            annotationHint = Instantiate(videoHintPrefab, transform.position, Quaternion.identity, transform);
        }

        annotationDisplay = FindObjectsOfType<ARAnnotationDisplay>(includeInactive: true)[0];
    }

    void Update()
    {
        //check the distance between the camera and the object
        float distance = Vector3.Distance(m_Camera.transform.position, transform.position);
        


        if (state == State.hidden){
            if (distance < maxVisibilityDistance){
                Hint();
            }
        }

        else if (state == State.hint){
            if (distance < maxFocusDistance){
                Focus();
            }
            if (distance > maxVisibilityDistance){
                Hide();
            }
        }

        else if (state == State.focused){
            if (distance > maxFocusDistance){
                Hint();
            }
            if (distance > maxVisibilityDistance){
                Hide();
            }
        }



        
    }

    public void Hint(){     
        if (ARAnnotation.focusedAnnotation == this){
            DeFocusDetailDisplay(); 
        }  
        state = State.hint;
        annotationHint.SetActive(true);
        annotationMarker.SetActive(false);
        
    }

    public void Hide(){     
        if (ARAnnotation.focusedAnnotation == this){
            DeFocusDetailDisplay(); 
        }
        state = State.hidden;
        annotationHint.SetActive(false);
        annotationMarker.SetActive(false);
        
    }

    public void Focus(){
        //if there is a closer annotation, switch ack to hint and return
        if (ARAnnotation.focusedAnnotation != null){
            float myDistance = Vector3.Distance(m_Camera.transform.position, transform.position);
            if (ARAnnotation.focusedAnnotation.isCloserToCameraThan(myDistance)){
                Hint();
                return;
            }else{
                ARAnnotation.focusedAnnotation.Hint();
            }
        }

        state = State.focused;
        
        annotationHint.SetActive(false);        
        annotationMarker.SetActive(true);


        //i.e. Focus display
        ARAnnotation.focusedAnnotation = this;
        annotationDisplay.DisplayTexture(annotationDetailTexture);
        annotationDisplay.AnimateIn();
    }

    public void DeFocusDetailDisplay(){
        if (ARAnnotation.focusedAnnotation == this && state == State.focused){
            Debug.Log("deFocus detail display");
            annotationDisplay.AnimateOut();
        }
    }

    public bool isCloserToCameraThan(float yourDistance){
        float myDistance = Vector3.Distance(m_Camera.transform.position, transform.position);
        return (myDistance < yourDistance);
    }

    //checks if the most recently focused annotation is in focused
    public static bool IsFocused(){
        if (focusedAnnotation != null){
            if (focusedAnnotation.state == State.focused){
                Debug.Log("AR annotation is focused");
                return true;
            }
        }
        Debug.Log("AR annotation is NOT focused");
        return false;
    }

}
