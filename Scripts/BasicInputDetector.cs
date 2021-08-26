using System;
using UnityEngine;

public class BasicInputDetector : MonoBehaviour
{
    
    //delegate input events
    public static event Action<Vector2> OnTouchBegan = delegate { };
    public static event Action<Vector2> OnTouchMoved = delegate { };
    public static event Action<Vector2> OnTouchEnded = delegate { };
    public static event Action<Vector2, Vector2> OnTwoTouches = delegate { };


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.touchCount == 1)
        {
            var touch = Input.touches[0];
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
        if (Input.touchCount == 2){
            OnTwoTouches(Input.touches[0].position, Input.touches[1].position);
        }
    }
}
