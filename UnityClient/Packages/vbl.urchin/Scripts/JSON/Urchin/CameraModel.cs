
using UnityEngine;

public struct CameraModel
{
    public float Id;
    public string Type;
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Target;
    public float Zoom;
    public Vector2 Pan;
    public CameraMode Mode;
    public bool Controllable;
    public bool Main;
}


public enum CameraMode
{
    orthographic = 0,
    perspective = 1,
}

