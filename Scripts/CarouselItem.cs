using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarouselItem : MonoBehaviour
{
    public bool isSelected = false;

    public Color backgroundColor;
    
    // Start is called before the first frame update
    void Start()
    {
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

    public void ChangeToLayer(string layerName){
        foreach (Transform child in GetComponentsInChildren<Transform>(includeInactive : true)){
            child.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }

}
