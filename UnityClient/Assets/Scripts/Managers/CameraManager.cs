using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO.Pem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    //Keep a dictionary that maps string names to camera components 
    private Dictionary<string, CameraBehavior> _cameras = new(); //directly access the camera nested within the prefab
    [SerializeField] private GameObject _cameraPrefab;
    [SerializeField] private RenderTexture output1;
    [SerializeField] private RenderTexture output2;
    [SerializeField] private RenderTexture output3;
    [SerializeField] private RenderTexture output4;
    [SerializeField] private CameraBehavior mainCamera;

    Stack<RenderTexture> textures = new();

 


    #region Unity functions
    private void Awake()
    {
        textures.Push(output4);
        textures.Push(output3);
        textures.Push(output2);
        textures.Push(output1);
        _cameras.Add("main", mainCamera);

        
    }
    private void Start()
    {
        //CreateCamera(new List<string> { "1", "3"});
        //float delayInSeconds = 2.0f; // Delay time in seconds
        //SetCameraControl("1");


        //Invoke("DeleteTest", delayInSeconds);
    }

    #endregion

   
    private void DeleteTest()
    {
        DeleteCamera(new List<string> { "2" });
    }



    #region Public functions

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
}