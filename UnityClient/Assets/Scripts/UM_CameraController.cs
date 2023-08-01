using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UM_CameraController : MonoBehaviour
{
    [SerializeField] BrainCameraController cameraController;

    [SerializeField] Camera orthoCamera;
    [SerializeField] Camera perspectiveCamera;

    public bool Orthographic;
    
    private void Awake()
    {
        Orthographic = orthoCamera.isActiveAndEnabled;
    }

    public void CameraContinuousRotationButton()
    {
        cameraController.SetCameraContinuousRotation(true);
    }

    public void SwitchCameraMode(bool orthographic)
    {
        Orthographic = orthographic;
        if (orthographic)
        {
            orthoCamera.gameObject.SetActive(true);
            perspectiveCamera.gameObject.SetActive(false);
           
            cameraController.SetCamera(orthoCamera);
        }
        else
        {
            orthoCamera.gameObject.SetActive(false);
            perspectiveCamera.gameObject.SetActive(true);
            cameraController.SetCamera(perspectiveCamera);
        }
    }
}
