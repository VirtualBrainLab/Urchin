using MLAPI;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIClickHandler : MonoBehaviour
{
    // UI camera
    public Camera uiCamera;
    public RectTransform brainImageRectTransform;
    public PlayerManager pmanager;

    // Canvas
    private Canvas canvas;
    private Vector2 canvasSize;

    // Raycaster
    private GraphicRaycaster raycaster;

    /* DRAG */
    public float dragSpeed = 500.0f;
    private bool dragging;
    private GameObject draggingPanel;
    private Vector2 dragPanelSize;
    private Vector3 dragPanelOffset;
    private Vector4 panelBoundaries;
    private float panelEdgeOffset = 2.0f;

    /* BRAIN CAMERA CONTROLLER */
    public Camera brainCamera; // apply translations to this
    public GameObject brainCameraRotator; // apply rotations to this
    public GameObject brain;

    // Scrolling variables
    public float minFoV = 15.0f;
    public float maxFoV = 90.0f;
    public float fovDelta = 15.0f;
    public float moveSpeed = 100.0f;

    // Rotation variables
    public float minXRotation = -90;
    public float maxXRotation = 90;
    public float minZRotation = -90;
    public float maxZRotation = 90;
    public float rotSpeed = 1000.0f;
    private float totalPitch;
    private float totalYaw;
    private Vector3 initialCameraRotatorPosition;

    // Button down tracking
    private bool mouseDownOverBrain;
    private int mouseButtonDown;
    private float doubleClickTime = 0.5f;
    private float lastLeftClick;
    private float lastRightClick;

    // Controlling the main UI camera
    private bool controlMainCamera;
    private float minMainFoV = 5.0f;
    private float maxMainFoV = 100f;
    [SerializeField] private GameObject cameraControlUIPanel;

    // to differentiate between a click and some kind of drag action, we will track the brain state
    private bool brainTransformChanged;
    private GameObject brainCameraClickthroughTarget;


    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.Find("UICanvas").GetComponent<Canvas>();
        canvasSize = canvas.GetComponent<RectTransform>().rect.size;

        totalPitch = 0;
        totalYaw = 0;

        mouseDownOverBrain = false;

        initialCameraRotatorPosition = brainCameraRotator.transform.position;

        raycaster = GetComponent<GraphicRaycaster>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!controlMainCamera)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                GameObject uiTarget = GetUIRaycastTarget(pointerData);

                if (uiTarget != null)
                {
                    switch (uiTarget.tag)
                    {
                        case "BrainImage":
                            BrainCameraControl();
                            break;
                        case "UIDraggable":
                            DragPanel(uiTarget);
                            break;
                        case "StimWindow":
                            //Debug.Log("stim window hit");
                            break;
                        case "OntologyToggle":
                            ToggleRegion(uiTarget);
                            break;
                    }
                }
            }

            BrainCameraControl_noTarget();
            DragPanel_noTarget();
        }
        else
        {
            // Controlling main camera
            ControlMainCamera();
        }

        // Handle spacebar clicks (switch control to controlling the main camera)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            controlMainCamera = !controlMainCamera;
            cameraControlUIPanel.SetActive(controlMainCamera);
        }
    }

    GameObject GetUIRaycastTarget(PointerEventData pointerData)
    {
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        pointerData.position = Input.mousePosition;
        raycaster.Raycast(pointerData, results);

        if (results.Count == 1)
        {
            return results[0].gameObject;
        }
        if (results.Count > 1)
        {
            //Debug.Log("Warning: multiple raycast results");
            ////For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            //foreach (RaycastResult result in results)
            //{
            //    Debug.Log("Hit " + result.gameObject.name);
            //}
            return results[0].gameObject;
        }

        return null;
    }

    private void ControlMainCamera()
    {
        // Check the scroll wheel and deal with the field of view
        float fov = uiCamera.fieldOfView;

        float scroll = -Input.GetAxis("Mouse ScrollWheel");
        fov += fovDelta * scroll;
        fov = Mathf.Clamp(fov, minMainFoV, maxMainFoV);

        uiCamera.fieldOfView = fov;
    }

    void DragPanel(GameObject targetPanel)
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            draggingPanel = targetPanel;
            dragPanelSize = draggingPanel.GetComponent<RectTransform>().rect.size;
            dragPanelOffset = targetPanel.transform.localPosition - ScaledMousePosition();
            Debug.Log(dragPanelOffset);
            // Pre-compute the edges
            panelBoundaries = new Vector4(-canvasSize.x / 2 + panelEdgeOffset, canvasSize.x / 2 - panelEdgeOffset, -canvasSize.y/2 + panelEdgeOffset, canvasSize.y/2 - panelEdgeOffset);
            Debug.Log(panelBoundaries);

            // Also bring the panel to the front
            draggingPanel.transform.SetAsLastSibling();
        }
    }

    void DragPanel_noTarget()
    {
        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (dragging)
        {
            Vector3 newPosition = ScaledMousePosition() + dragPanelOffset;

            newPosition.x = Mathf.Clamp(newPosition.x, panelBoundaries.x, panelBoundaries.y - dragPanelSize.x);
            newPosition.y = Mathf.Clamp(newPosition.y, panelBoundaries.z + dragPanelSize.y, panelBoundaries.w);

            draggingPanel.transform.localPosition = newPosition;
        }
    }

    Vector3 ScaledMousePosition()
    {
        return Input.mousePosition / canvas.scaleFactor;
    }

    void BrainCameraControl()
    {
        // Check the scroll wheel and deal with the field of view
        float fov = brainCamera.fieldOfView;

        float scroll = -Input.GetAxis("Mouse ScrollWheel");
        fov += fovDelta * scroll;
        fov = Mathf.Clamp(fov, minFoV, maxFoV);

        brainCamera.fieldOfView = fov;

        // Now check if the mouse wheel is being held down
        if (Input.GetMouseButton(1))
        {
            mouseDownOverBrain = true;
            mouseButtonDown = 1;
        }

        // Now deal with dragging
        if (Input.GetMouseButtonDown(0))
        {
            BrainCameraDetectTargets();
            mouseDownOverBrain = true;
            mouseButtonDown = 0;
        }

    }

    void ClearMouseDown()
    {
        mouseDownOverBrain = false;
        brainCameraClickthroughTarget = null;
        brainTransformChanged = false;
    }

    void BrainCameraControl_noTarget()
    {

        // Deal with releasing the mouse (anywhere)
        if (mouseDownOverBrain && mouseButtonDown==0 && Input.GetMouseButtonUp(0))
        {
            if (!brainTransformChanged)
            {
                // All we did was click through the brain window 
                if (brainCameraClickthroughTarget)
                {
                    BrainCameraClickthrough();
                }
                else if ((Time.realtimeSinceStartup - lastLeftClick) < doubleClickTime)
                {
                    totalYaw = 0f;
                    totalPitch = 0f;
                    ApplyBrainCameraRotatorRotation();
                }
            }

            lastLeftClick = Time.realtimeSinceStartup;
            ClearMouseDown(); return;
        }
        if (mouseDownOverBrain && mouseButtonDown==1 && Input.GetMouseButtonUp(1))
        {
            if (!brainTransformChanged)
            {
                // Check for double click
                if ((Time.realtimeSinceStartup - lastRightClick) < doubleClickTime)
                {
                    // Reset the brainCamera transform position
                    brainCamera.transform.localPosition = Vector3.zero;
                }
            }

            lastRightClick = Time.realtimeSinceStartup;
            ClearMouseDown(); return;
        }

        if (mouseDownOverBrain && mouseButtonDown==1)
        {
            // While right-click is held down 
            float xMove = -Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
            float yMove = -Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;

            if (xMove != 0 || yMove != 0)
            {
                brainTransformChanged = true;
                brainCamera.transform.Translate(xMove, yMove, 0, Space.Self);
            }
        }

        // If the mouse is down, even if we are far way now we should drag the brain
        if (mouseDownOverBrain && mouseButtonDown==0)
        {
            float xRot = -Input.GetAxis("Mouse X") * rotSpeed * Time.deltaTime;
            float yRot = Input.GetAxis("Mouse Y") * rotSpeed * Time.deltaTime;

            if (xRot != 0 || yRot != 0)
            {
                brainTransformChanged = true;

                // Pitch Locally, Yaw Globally. See: https://gamedev.stackexchange.com/questions/136174/im-rotating-an-object-on-two-axes-so-why-does-it-keep-twisting-around-the-thir
                totalYaw = Mathf.Clamp(totalYaw + yRot, minXRotation, maxXRotation);
                totalPitch = Mathf.Clamp(totalPitch + xRot, minZRotation, maxZRotation);

                ApplyBrainCameraRotatorRotation();
            }
        }
    }

    void ApplyBrainCameraRotatorRotation()
    {
        Quaternion curRotation = Quaternion.Euler(totalYaw, 0, totalPitch);

        // Move the camera back to zero, perform rotation, then offset back
        brainCameraRotator.transform.position = initialCameraRotatorPosition;
        brainCameraRotator.transform.LookAt(brain.transform, Vector3.back);
        brainCameraRotator.transform.position = curRotation * (brainCameraRotator.transform.position - brain.transform.position) + brain.transform.position;
        brainCameraRotator.transform.rotation = curRotation * brainCameraRotator.transform.rotation;
    }

    /*
     * 
     */
    void BrainCameraDetectTargets()
    {
        // Raycast through the camera into real space and see whether we click on something, e.g. 
        // a probe or a brain area, save this gameObject information
        Vector2 localPoint = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(brainImageRectTransform, Input.mousePosition, uiCamera, out localPoint);
        Vector2 normalizedPoint = Rect.PointToNormalized(brainImageRectTransform.rect, localPoint);

        var renderRay = brainCamera.ViewportPointToRay(normalizedPoint);
        if (Physics.Raycast(renderRay, out var raycastHit, 500f, LayerMask.GetMask(new string[] { "BrainAreas", "Probe" })))
        {
            brainCameraClickthroughTarget = raycastHit.collider.gameObject;
        }
    }

    void BrainCameraClickthrough()
    {
        if (!brainTransformChanged && brainCameraClickthroughTarget)
        {
            if (brainCameraClickthroughTarget.CompareTag("ProbeModel"))
            {
                GameObject probe = brainCameraClickthroughTarget.transform.parent.gameObject; 
                pmanager.DeactivatePlayerObjects();
                probe.GetComponent<Electrode>().SetActive(true);
                Debug.LogWarning("Workaround in place due to PlayerObject issues");
                //NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject = probe.GetComponent<NetworkObject>();

            }
            if (brainCameraClickthroughTarget.CompareTag("BrainRegion"))
            {
                string regionName = brainCameraClickthroughTarget.name;
                GameObject[] toggles = GameObject.FindGameObjectsWithTag("OntologyToggle");
                // This line finds the toggle corresponding to this brain region within
                // the set of ontology toggles.
                GameObject toggle = Array.Find(toggles, t => regionName.Equals(t.transform.Find("Label").GetComponent<Text>().text));
                // Cycle this region's shader and update the toggle's status ('-', '/', or '+')
                CycleBrainShader(brainCameraClickthroughTarget, toggle);
            }

        }
    }

    void ToggleRegion(GameObject toggle) {
        if (Input.GetMouseButtonDown(0)) {
            // Get the brain region name corresponding to this toggle
            string regionName = toggle.transform.Find("Label").GetComponent<Text>().text;
            GameObject[] regions = GameObject.FindGameObjectsWithTag("BrainRegion");
            // This line finds the brain region corresponding to this toggle within
            // the set of brain region gameobjects.
            GameObject region = Array.Find(regions, r => regionName.Equals(r.name));
            // Cycle the brain region's shader and update this toggle's status ('-', '/', or '+')
            CycleBrainShader(region, toggle);
        }
    }

    void CycleBrainShader(GameObject region, GameObject regionToggle) {
        BrainRegionSelector brs = region.GetComponent<BrainRegionSelector>();
        brs.CycleModes();
        // "+" for opaque, "/" for "jello", and "-" for transparent modes
        if (regionToggle != null && regionToggle.activeSelf)
        {
            string status = brs.GetMode() switch
            {
                2 => "+",
                1 => "/",
                _ => "-",
            };
            regionToggle.transform.Find("Status").GetComponent<Text>().text = status;
        }
    }

    public void ChangeBrainTransparency(float newAlpha) {
        GameObject[] regions = GameObject.FindGameObjectsWithTag("BrainRegion");
        if (regions != null) {
            foreach (GameObject region in regions) {
                region.GetComponent<Renderer>().material.SetFloat("_Alpha", newAlpha);
            }
        }
    }
}
