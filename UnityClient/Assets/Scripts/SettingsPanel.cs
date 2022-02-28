using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private UM_CameraController cameraController;
    [SerializeField] private GameObject settingsPanel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
            cameraController.BlockDragging();

        if (Input.GetKeyDown(KeyCode.S))
            settingsPanel.SetActive(settingsPanel.activeSelf);
    }
}
