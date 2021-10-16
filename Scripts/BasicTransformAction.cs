using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTransformAction : MonoBehaviour
{
    //enum types of actions
    public enum Type {Translate, Rotate, Scale};
    public Type type;

    //enum types of axis
    public enum Axis {x, y, z};
    public Axis axis;

    //enum types space
    public enum Space {local, world, camera};
    public Space space;

    public void Action(float input){
        switch(type){
            case Type.Translate:
                Translate(input);
                break;
            case Type.Rotate:
                Rotate(input);
                break;
            case Type.Scale:
                Scale(input);
                break;
            default:
                break;
        }
    }

    void Translate(float input){
        //todo
    }

    void Rotate(float input){
        //todo
    }

    void Scale(float input){
        //todo
    }
}
