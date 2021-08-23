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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
