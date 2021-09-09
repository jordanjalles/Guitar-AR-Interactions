using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARAnnotationDisplay : MonoBehaviour
{
    //function that takes a texture and displays it on the screen
    public void DisplayTexture(Texture2D texture){
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
    }

}
