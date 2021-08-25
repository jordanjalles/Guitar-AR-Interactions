using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARSelectable : MonoBehaviour
{
    public bool isSelected = false;
    
    public Vector3 homePosition;
    public Vector3 homeRotation;
    
    // Start is called before the first frame update
    void Start()
    {
        this.homePosition = this.transform.position;
        this.homeRotation = this.transform.rotation.eulerAngles;
        Deselect();
    }

    public void Select(){
        Debug.Log("Selecting " + this.name);
        this.isSelected = true;
        //get all arannotations in children and activate them
        foreach (ARAnnotation a in GetComponentsInChildren<ARAnnotation>(includeInactive : true)){
            a.gameObject.SetActive(true);
            Debug.Log("Activating " + a.name);
        }
    }

    public void Deselect(){
        this.isSelected = false;
        //todo turn off annotations
        foreach (ARAnnotation a in GetComponentsInChildren<ARAnnotation>()){
            a.gameObject.SetActive(false);
        }
    }
}
