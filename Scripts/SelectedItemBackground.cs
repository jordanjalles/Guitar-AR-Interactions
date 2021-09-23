using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectedItemBackground : MonoBehaviour
{
    //two colors, one for selected state and one for deselected state
    public Color selectedColor;
    public Color deselectedColor;
    public AnimationCurve curve;

    private bool selected = false;

    //function called when the item is selected
    public void OnItemSelected(){
        if(selected) return;
        selected = true;
        AnimateToColor(selectedColor);
        //GetComponent<Image>().color = selectedColor;
    }

    //function called when the item is deselected
    public void OnItemDeselected(){
        if(!selected) return;
        selected = false;
        AnimateToColor(deselectedColor);
        //GetComponent<Image>().color = deselectedColor;
    }

    //function to animate a new color
    public void AnimateToColor(Color color){
        AnimateColor animator = gameObject.AddComponent<AnimateColor>();
        animator.Configure(color, 1f, curve);
    }

}
