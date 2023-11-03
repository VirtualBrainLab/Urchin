using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urchin.Cameras
{
    public class CameraMiniControllerHandle : MonoBehaviour
    {
        [SerializeField] BrainCameraController cameraController;
        [SerializeField] Vector3 eulerAngles;
        private float lastClick = 0f;

        public void Click()
        {
            if ((Time.realtimeSinceStartup - lastClick) < BrainCameraController.doubleClickTime)
                cameraController.SetBrainAxisAngles(eulerAngles);
            else
                lastClick = Time.realtimeSinceStartup;
        }
    }
}