using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ARAnnotationDisplay : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve animationCurveIn;
    
    [SerializeField]
    private AnimationCurve animationCurveOut;

    public void AnimateIn(){
        Debug.Log("AnimateIn");
        //set the rect transform off the bottom of the screen
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, -GetComponent<RectTransform>().rect.height, GetComponent<RectTransform>().rect.height);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, GetComponent<RectTransform>().rect.width);

        //animate to the bottom edge of the screen over 0.5 seconds
        AnimateRectTransform animator = gameObject.AddComponent<AnimateRectTransform>();
        animator.Configure(Vector2.zero, Quaternion.identity, 0.5f, animationCurveIn);
    }

    public void AnimateOut(){
        Debug.Log("AnimateOut");
        AnimateRectTransform animator = gameObject.AddComponent<AnimateRectTransform>();
        animator.Configure(new Vector2(0, -GetComponent<RectTransform>().rect.height), Quaternion.identity, 0.5f, animationCurveOut);
        
        //animator.OnComplete += HideTexture;
    }

    //function that takes a texture and displays it on the screen
    //it also initializes the size of sprite and rect transform
    public void DisplayTexture(Texture2D texture){
        Debug.Log("Displaying texture");
        gameObject.active = true;
        //set the image sprite to the texture passed in
        GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        
        float textureToScreenScaleFactor = Camera.main.pixelWidth*1.0f/texture.width; //1.0f converts this to a float calc

        //set the rect transform to the bottom of the screen
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, textureToScreenScaleFactor*texture.height);
        rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, Camera.main.pixelWidth);
    }



    //function that hides the texture
    public void HideTexture(){
        gameObject.active = false;
        //set the image sprite to null
        GetComponent<Image>().sprite = null;

        AnimateRectTransform animator = GetComponent<AnimateRectTransform>();
        if (animator != null){
            Debug.Log("Removing hidetexture oncomplete from animator");
            animator.OnComplete -= HideTexture;
        }
    }

}
