
using UnityEngine;
    
public struct CameraModel
{
    public float id;
    public string type;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 target;
    public float zoom;
    public Vector2 pan;
    public CameraMode mode;
    public bool controllable;
    public bool main;
}


public enum CameraMode
{
    orthographic = 0,
    perspective = 1,
}

