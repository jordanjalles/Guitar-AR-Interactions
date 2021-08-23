using System;
using UnityEngine;

public class BasicInputDetector : MonoBehaviour
{
    
    //delegate input events
    public static event Action<Vector2> OnTouchBegan = delegate { };
    public static event Action<Vector2> OnTouchMoved = delegate { };
    public static event Action<Vector2> OnTouchEnded = delegate { };


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
                OnTouchBegan(touch.position);
            }
            if (touch.phase == TouchPhase.Moved)
            {
                OnTouchMoved(touch.position);
            }
            if (touch.phase == TouchPhase.Ended)
            {   
                OnTouchEnded(touch.position);
            }
        }
    }
}
