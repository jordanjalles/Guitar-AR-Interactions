using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitBehavior : MonoBehaviour
{
    public void OnButtonPressed()
    {
    	  Application.Unload();
    }
}
