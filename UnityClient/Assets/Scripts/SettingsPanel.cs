using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Urchin.Cameras;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private BrainCameraController cameraController;
    [SerializeField] private GameObject settingsPanel;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
            cameraController.SetControlBlock(true);

        if (Input.GetMouseButtonUp(0))
            cameraController.SetControlBlock(false);

        if (Input.GetKeyDown(KeyCode.S))
            settingsPanel.SetActive(settingsPanel.activeSelf);
    }
}
