using BestHTTP.SecureProtocol.Org.BouncyCastle.Utilities.IO.Pem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    //Keep a dictionary that maps string names to camera components 
    private Dictionary<string, Camera> _cameras = new(); //directly access the camera nested within the prefab
    [SerializeField] private GameObject _cameraPrefab;
    [SerializeField] private RenderTexture output1;
    [SerializeField] private RenderTexture output2;
    [SerializeField] private RenderTexture output3;
    [SerializeField] private RenderTexture output4;

    public BrainCameraController cameraController;

    Stack<RenderTexture> textures = new();

    List<string> cameraTest = new();
    List<string> cameraTest2 = new();


    #region Unity functions
    private void Awake()
    {
        textures.Push(output4);
        textures.Push(output3);
        textures.Push(output2);
        textures.Push(output1);

        cameraTest.Add("1");
        cameraTest.Add("2");
    }
    private void Start()
    {
        CreateCamera(cameraTest);
        float delayInSeconds = 2.0f; // Delay time in seconds

        // Invoke a function after the specified delay
        Invoke("CreateTest", delayInSeconds);
        //Invoke("DeleteTest", delayInSeconds);
    }

    #endregion

    private void CreateTest()
    {
        CreateCamera(cameraTest2);
    }
    private void DeleteTest()
    {
        DeleteCamera(cameraTest);
    }



    #region Public functions

    public void CreateCamera(List<string> cameras)
    {
        //instantiating game object w camera component
        foreach (string camera in cameras)
        {
            GameObject tempObject = Instantiate(_cameraPrefab);
            tempObject.name = $"camera_{camera}";
            //cameraController.UserControllable = false; //will need to be changed if a new main camera is being created CAUSES TARGET TEXTURE TO NOT WORK

            // Get all Camera components attached to children of the script's GameObject (since there are multiple)
            Camera[] cameraComponenents = tempObject.GetComponentsInChildren<Camera>();

            // Access and manipulate the cameras as needed
            foreach (Camera cameraComponent in cameraComponenents)
            {
                // Do something with the camera
                bool isCameraOn = cameraComponent.enabled;

                // Use the boolean value as needed
                if (isCameraOn)
                {
                    // Camera is turned on + connected to proper texture
                    cameraComponent.targetTexture = textures.Pop();
                    _cameras.Add(camera, cameraComponent); //doesn't take care of the whole prefab, but makes everything easier regarding camera manipulation?
                                                 //in theory, creates new entry to the dictionary with the name of the camera [camera] and associates it with a new Game Object

                }
            }
        }

    }

    public void DeleteCamera(List<string> cameras)
    {

        //calls destroy (the one specific camera)
        foreach (string camera in cameras)
        {
            textures.Push(_cameras[camera].targetTexture);
            //Camera cameraComponent = _cameras[camera].gameObject.GetComponent<Camera>();
            //textures.Push(cameraComponent.targetTexture);
            Transform parentObj = _cameras[camera].gameObject.transform.parent;
            //Destroy(parentObj.gameObject);
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
    #endregion
}