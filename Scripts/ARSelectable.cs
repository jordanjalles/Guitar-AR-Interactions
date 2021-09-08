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
        this.isSelected = true;
        //Debug.Log("Selecting " + this.name);
        //get all arannotations in children and activate them
        foreach (ARAnnotation a in GetComponentsInChildren<ARAnnotation>(includeInactive : true)){
            a.gameObject.SetActive(true);
            //Debug.Log("Activating " + a.name);
        }
    }

    public void Deselect(){
        this.isSelected = false;
        //Debug.Log("Deselecting " + this.name);
        //turn off annotations
        foreach (ARAnnotation a in GetComponentsInChildren<ARAnnotation>()){
            //Debug.Log("Deactivating " + a.name);
            a.Hide(); //calling hide so that it removes any detail display that may be active
            a.gameObject.SetActive(false);
        }
    }
}
