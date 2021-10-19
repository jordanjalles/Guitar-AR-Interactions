using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTransformAction : MonoBehaviour
{
    //enum types of actions
    public enum Type {Translate, Rotate, Scale};
    public Type type;

    //enum types of axis
    public enum Axis {x, y, z, All};
    public Axis axis;

    //enum types space
    public enum Space {Local, World, Camera};
    public Space space;

    //bool invert positionDelta
    public bool invertInput = false;

    //float multiplier
    public float multiplier = 1f;

    public void Activate(float input){

        input = invertInput ? -input : input;
        input *= multiplier;

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

    public void Translate(float input){
        Vector3 positionDelta = GetAxisVector(axis)*input;
        positionDelta = TransformVectorToSpace(positionDelta, space);
        transform.position += positionDelta;
    }

    public Vector3 TransformVectorToSpace(Vector3 vector, Space space){
        switch(space){
            case Space.Local:
                return transform.TransformDirection(vector);
            case Space.World:
                return vector;
            case Space.Camera:
                return Camera.main.transform.TransformDirection(vector);
            default:
                Debug.LogError("Invalid space");
                return vector;
        }
    }

    public Vector3 GetAxisVector(Axis axis){
        switch(axis){
            case Axis.x:
                return new Vector3(1, 0, 0);
            case Axis.y:
                return new Vector3(0, 1, 0);
            case Axis.z:
                return new Vector3(0, 0, 1);
            case Axis.All:
                return new Vector3(1, 1, 1);
            default:
                Debug.LogError("Invalid axis");
                return Vector3.zero;
                break;
        }
    }

    void Rotate(float input){
        Vector3 rotationDelta = GetAxisVector(axis)*input;
        rotationDelta = TransformVectorToSpace(rotationDelta, space);
        transform.Rotate(rotationDelta, UnityEngine.Space.World);

        Debug.Log("Rotate: " + rotationDelta);
        //todo
    }

    void Scale(float input){
        Vector3 scaleDelta = GetAxisVector(axis)*input;
        scaleDelta = TransformVectorToSpace(scaleDelta, space);
        transform.localScale += scaleDelta;
        //transform.scale += scaleDelta;
    }
}
