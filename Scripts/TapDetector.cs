using System;
using UnityEngine;

public class TapDetector : MonoBehaviour
{
    
    //delegate tap event
    public static event Action<Vector2> OnTap = delegate { };

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                OnTap(touch.position);
            }
        }
    }
}
