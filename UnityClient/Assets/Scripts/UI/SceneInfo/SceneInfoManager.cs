using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Urchin.Cameras;

public class SceneInfoManager : MonoBehaviour
{
    // Text accessors
    [SerializeField] private TMP_Text cameraText;
    [SerializeField] private TMP_Text visModelText;
    [SerializeField] private TMP_Text sceneStatsText;

    // Access to data
    [SerializeField] private BrainCameraController cameraController;

    // Update is called once per frame
    void Update()
    {
        Vector3 target = cameraController.CameraTarget;
        Vector3 angles = cameraController.PitchYawRoll;
        float zoom = cameraController.GetZoom();

        // Note angles y/x flipped so that yaw/pitch/spin is correct
        cameraText.text = string.Format("camera target ({0},{1},{2}); camera rotation ({3},{4},{5})\nzoom: {6}",
            Mathf.Round(target.x*100)/100,
            Mathf.Round(target.y*100)/100,
            Mathf.Round(target.z*100)/100,
            Mathf.RoundToInt(angles.y),
            Mathf.RoundToInt(angles.x),
            Mathf.RoundToInt(angles.z),
            zoom);

    }
}
