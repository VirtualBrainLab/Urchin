using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UM_CameraController : MonoBehaviour
{
    [SerializeField] BrainCameraController cameraController;

    public void CameraContinuousRotationButton()
    {
        cameraController.SetCameraContinuousRotation(true);
    }

}
