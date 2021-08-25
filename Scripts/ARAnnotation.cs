using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private GameObject annotationDetail;

    //use this to detect conflicts with other ARAnnotations
    static ARAnnotation focusedAnnotation;

    //state enums
    enum State {hidden, active, hint};
    State state;

    void Awake()
    {
        m_Camera = Camera.main;
        //instantiate the annotation game objects
        annotationMarker = Instantiate(annotationMarker, transform.position,  Quaternion.identity, transform);
        //annotationDetail = Instantiate(annotationDetail);
        annotationHint = Instantiate(annotationHint, transform.position, Quaternion.identity, transform);


    }

    void Update()
    {
        //check the distance between the camera and the object
        float distance = Vector3.Distance(m_Camera.transform.position, transform.position);
        
        //if the distance is less than the visibility distance, show hint
        if (distance < maxVisibilityDistance)
        {
            Hint();
        }else
        {
            Hide();
        }

        //if the distance is less than the activation distance, activate the annotation
        if (distance < maxActivationDistance)
        {
            Activate();
        }

            
    }

    public void Hint(){
        state = State.hint;
        annotationHint.SetActive(true);
        //annotationDetail.SetActive(false);
        annotationMarker.SetActive(false);
    }

    public void Activate(){
        state = State.active;
        annotationHint.SetActive(false);
        //annotationDetail.SetActive(true);
        annotationMarker.SetActive(true);
    }

    public void Hide(){
        state = State.hidden;
        annotationHint.SetActive(false);
        //annotationDetail.SetActive(false);
        annotationMarker.SetActive(false);
    }


}
