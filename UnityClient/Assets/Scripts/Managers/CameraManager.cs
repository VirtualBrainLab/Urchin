using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    //Keep a dictionary that maps string names to camera components 
    private Dictionary<string, Camera> _cameras;
    [SerializeField] private GameObject _cameraPrefab;

    public void CreateCamera(List<string> cameras)
    {
        //instantiating game object w camera component
        foreach (string camera in cameras)
        {
            GameObject tempObject = Instantiate(_cameraPrefab);
            tempObject.name = $"camera_{camera}";
            _cameras.Add(camera, tempObject.GetComponent<Camera>());
            //in theory, creates new entry to the dictionary with the name of the camera [camera] and associates it with a new Game Object

            //adds the camera component to the camera manager (actually creates the camera of the empty object)
        }

    }

    public void DeleteCamera(List<string> cameras)
    {

        //calls destroy (the one specific camera)
        foreach (string camera in cameras)
        {
            Destroy(_cameras[camera].gameObject);
            _cameras.Remove(camera);
        }
    }

    //each function below works by first checking if camera exits in dictionary, and then calling cameraBehavior function
    public void SetCameraRotation(Dictionary<string, List<float>> cameraRotation) { }

    public void SetCameraZoom(Dictionary<string, float> cameraZoom) { }

    public void SetCameraMode(Dictionary<string, string> cameraMode) { }

    public void Screenshot() { }

    public void SetCameraPosition(Dictionary<string, List<float>> cameraPosition) { }

    public void SetCameraYAngle(Dictionary<string, float> cameraYAngle) { }

    public void SetCameraTargetArea(Dictionary<string, string> cameraTargetArea) { }

    public void SetCameraTarget(Dictionary<string, List<float>> cameraTargetmlapdv) { }

    public void SetCameraPan(Dictionary<string, List<float>> cameraPanXY) { }
}