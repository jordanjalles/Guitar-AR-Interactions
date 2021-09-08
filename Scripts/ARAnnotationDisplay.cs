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

        //GetComponent<RawImage>().texture = texture;
    }

    //function that hides the texture
    public void HideTexture(){
        gameObject.active = false;
        //set the image sprite to null
        GetComponent<Image>().sprite = null;

       //GetComponent<RawImage>().texture = null;
    }

}
