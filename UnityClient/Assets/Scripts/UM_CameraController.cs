using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UM_CameraController : MonoBehaviour
{
    [SerializeField] BrainCameraController cameraController;

    [SerializeField] Camera orthoCamera;
    [SerializeField] Camera perspectiveCamera;

    private GameObject uiGO;
    
    private void Awake()
    {

        uiGO = GameObject.Find("UI");
    }

    public void CameraContinuousRotationButton()
    {
        cameraController.SetCameraContinuousRotation(true);
    }

    public void SwitchCameraMode(bool orthographic)
    {
        if (orthographic)
        {
            orthoCamera.gameObject.SetActive(true);
            perspectiveCamera.gameObject.SetActive(false);
            if (uiGO)
                uiGO.GetComponent<Canvas>().worldCamera = orthoCamera;
            cameraController.SetCamera(orthoCamera);
        }
        else
        {
            orthoCamera.gameObject.SetActive(false);
            perspectiveCamera.gameObject.SetActive(true);
            if (uiGO)
                uiGO.GetComponent<Canvas>().worldCamera = perspectiveCamera;
            cameraController.SetCamera(perspectiveCamera);
        }
    }
}
