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

    private float maxActivationDistance;

    [SerializeField]
    private GameObject annotationHint;

    //annotation marker
    [SerializeField]
    private GameObject annotationMarker;

    [SerializeField]
    private Texture2D annotationDetailTexture;

    //use this to detect conflicts with other ARAnnotations
    static ARAnnotation focusedAnnotation;

    [SerializeField]
    private Canvas canvas;

    private ARAnnotationDisplay annotationDisplay;

    //state enums
    enum State {hidden, active, hint};
    State state;

    void Awake()
    {
        m_Camera = Camera.main;
        //instantiate the annotation game objects
        annotationMarker = Instantiate(annotationMarker, transform.position,  Quaternion.identity, transform);
        //annotationDetailTexture = Instantiate(annotationDetailTexture);
        annotationHint = Instantiate(annotationHint, transform.position, Quaternion.identity, transform);

        //annotationDisplay = canvas.GetComponentsInChildren<ARAnnotationDisplay>(includeInactive: true)[0];
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
            if (distance < maxActivationDistance){
                Activate();
            }
            if (distance > maxVisibilityDistance){
                Hide();
            }
        }

        else if (state == State.active){
            if (distance > maxActivationDistance){
                Hint();
            }
            if (distance > maxVisibilityDistance){
                Hide();
            }
        }



        
    }

    public void Hint(){        
        state = State.hint;
        annotationHint.SetActive(true);
        annotationMarker.SetActive(false);
        DeactivateDetailDisplay();
    }

    public void Hide(){      
        state = State.hidden;
        annotationHint.SetActive(false);
        annotationMarker.SetActive(false);
        DeactivateDetailDisplay();
    }

    public void Activate(){
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

        state = State.active;
        
        annotationHint.SetActive(false);        
        annotationMarker.SetActive(true);

        //i.e. activate display
        ARAnnotation.focusedAnnotation = this;
        annotationDisplay.DisplayTexture(annotationDetailTexture);
    }

    public void DeactivateDetailDisplay(){
        if (ARAnnotation.focusedAnnotation == this){
            Debug.Log("hiding annotation detail");
            annotationDisplay.HideTexture();
        }
    }

    public bool isCloserToCameraThan(float yourDistance){
        float myDistance = Vector3.Distance(m_Camera.transform.position, transform.position);
        return (myDistance < yourDistance);
    }



}
