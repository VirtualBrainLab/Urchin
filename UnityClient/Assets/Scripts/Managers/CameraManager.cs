using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO.Pem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    #region Exposed fields
    [SerializeField] private GameObject _cameraPrefab;
    [SerializeField] private RenderTexture _renderTexture1;
    [SerializeField] private RenderTexture _renderTexture2;
    [SerializeField] private RenderTexture _renderTexture3;
    [SerializeField] private RenderTexture _renderTexture4;
    [SerializeField] private CameraBehavior mainCamera;
    [SerializeField] private LightBehavior _lightBehavior;
    #endregion

    #region Variables
    private Dictionary<string, CameraBehavior> _cameras; //directly access the camera nested within the prefab
    private Stack<RenderTexture> textures = new();
    #endregion

    #region Unity functions
    private void Awake()
    {
        textures.Push(_renderTexture4);
        textures.Push(_renderTexture3);
        textures.Push(_renderTexture2);
        textures.Push(_renderTexture1);
        _cameras = new();
        _cameras.Add("main", mainCamera);
    }

    #endregion


    #region Public camera functions

    public void CreateCamera(List<string> cameras)
    {
        //instantiating game object w camera component
        foreach (string camera in cameras)
        {
            GameObject tempObject = Instantiate(_cameraPrefab);
            tempObject.name = $"camera_{camera}";
            CameraBehavior cameraBehavior = tempObject.GetComponent<CameraBehavior>();
            cameraBehavior.RenderTexture = textures.Pop();
            // Get all Camera components attached to children of the script's GameObject (since there are multiple)
            _cameras.Add(camera, cameraBehavior);
        }

    }

    public void DeleteCamera(List<string> cameraNames)
    {
        foreach (string cameraName in cameraNames)
        {
            textures.Push(_cameras[cameraName].RenderTexture);
            GameObject.Destroy(_cameras[cameraName].gameObject);
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

    public void SetCameraControl(string cameraName)
    {
        //set everything to false except for given camera
        foreach(var kvp in _cameras)
        {
            if (kvp.Key == cameraName)
            {
                kvp.Value.SetCameraControl(true);
            }
            else
            {
                kvp.Value.SetCameraControl(false);
            }

            //kvp.Value.SetCameraControl(kvp.Key == cameraName);
        }
    }
    #endregion

    #region Public light functions

    /// <summary>
    /// Reset the camera-light link to the main camera
    /// </summary>
    public void SetLightCameraLink()
    {
        _lightBehavior.SetCamera(mainCamera.gameObject);
    }

    /// <summary>
    /// Set the camera-light link to a specific camera in the scene
    /// </summary>
    /// <param name="newCameraGO"></param>
    public void SetLightCameraLink(string cameraName)
    {
        _lightBehavior.SetCamera(_cameras[cameraName].gameObject);
    }

    /// <summary>
    /// Rotate the light in the scene to a specific set of euler angles
    /// </summary>
    /// <param name="eulerAngles"></param>
    public void SetLightRotation(List<float> eulerAngles)
    {
        Debug.Log($"Received new light angles");
        _lightBehavior.SetRotation(new Vector3(eulerAngles[0], eulerAngles[1], eulerAngles[2]));
    }

    #endregion
}