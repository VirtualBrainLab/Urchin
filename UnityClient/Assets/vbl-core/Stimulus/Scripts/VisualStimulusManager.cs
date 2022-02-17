using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Handles spawning and destroying VisualStimulus objects. Note that the stimuli are entirely *client-side*
 * they don't exist on the server!!
 * 
 * See the prefabs/Stimulus folder for relevant objects
 */
public class VisualStimulusManager : MonoBehaviour
{
    public ElectrodeManager emanager;
    [SerializeField] private GameObject stimPlane;
    [SerializeField] private bool debug;
    // TOOLTIP
    [SerializeField] private GameObject visStimTooltip;

    // DRAG VARS
    public Camera uiCam;
    public Camera stimCam;
    public LayerMask stimuliMask;
    public LayerMask stimPlaneMask;

    Vector3 offsetVector;
    GameObject draggingStimulus;
    bool dragStart;
    bool dragMoved;

    // INSTANTIATE VARS
    public List<GameObject> visualStimulusPrefabs;
    public List<string> visualStimulusKeys;
    public Camera stimulusCamera;
    public RenderTexture stimulusCameraTexture;

    private bool usingTooltip;
    private VisualStimulus curVisStim;
    private VisualStimulusTooltip curVisTooltip;


    Dictionary<string, GameObject> visualStimulusPrefabDict;

    private Vector3 initStimPos;
    private Vector3 initMousePos;

    private bool taskRunning;

    private GameObject debugStimulus;

    private void Awake()
    {
        visualStimulusPrefabDict = new Dictionary<string, GameObject>();
        for (int i = 0; i < visualStimulusPrefabs.Count; i++)
        {
            visualStimulusPrefabDict.Add(visualStimulusKeys[i], visualStimulusPrefabs[i]);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (debug)
        {
            debugStimulus = AddNewStimulus("gabor");
        }

        // Tooltip
        if (visStimTooltip)
        {
            visStimTooltip.SetActive(false);
            usingTooltip = true;
        }
        else
        {
            usingTooltip = false;
        }

        // DRAG CODE
        if (uiCam == null) { uiCam = GameObject.Find("UICamera").GetComponent<Camera>(); }
        offsetVector = Vector3.forward * 0.01f;
    }

    // Update is called once per frame
     void Update() {
        if (debug)
        {
            int debugPosition = Mathf.FloorToInt(Time.realtimeSinceStartup);
            int count = 0;

            for (float x = -45; x <= 45; x += 15f)
            {
                for (float y = -45; y <= 45; y += 15f)
                {
                    if (debugPosition==count++)
                    {
                        SetStimPositionDegrees(debugStimulus, new Vector2(x, y));
                    }
                    //Quaternion angle = Quaternion.Euler(new Vector3(x, y));
                    //Debug.DrawRay(stimulusCamera.transform.position, angle * Vector3.back * 200f);
                }
            }
        }

        // Test if the mouse is over any objects in the stimuli mask layer
        Ray ray = uiCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, Mathf.Infinity, stimuliMask)) {
            draggingStimulus = hit.collider.gameObject;
            curVisStim = draggingStimulus.GetComponent<VisualStimulus>();
            // If left click is pressed over the stimulus, record the current
            // mouse and stim positions for later use when dragging
            if (Input.GetMouseButtonDown(0)) {
                if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit planeHit, Mathf.Infinity, stimPlaneMask)) {
                    initMousePos = planeHit.point;
                    initStimPos = draggingStimulus.transform.position;
                }
                dragStart = true;
            }
        }
        if (dragStart)
        {
            // If a drag was started, check that we haven't moved
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit planeHit, Mathf.Infinity, stimPlaneMask))
            {
                // Add the difference between initial mouse / stim positions
                // so the stimulus doesn't move when clicked
                Vector3 newStimPos = planeHit.point + (initStimPos - initMousePos);

                if (!dragMoved)
                {
                    // We haven't actually moved, just see if we moved
                    if (Vector3.Distance(initStimPos, newStimPos) > 1)
                    {
                        dragMoved = true;
                        if (usingTooltip)
                            curVisStim.HideTooltip();
                    }
                    else
                    {
                        // If we didn't move and left click is released, toggle the tooltip
                        if (Input.GetMouseButtonUp(0))
                        {
                            curVisStim.Clicked();
                        }
                    }
                }
                else
                {
                    SetStimPosition(draggingStimulus, newStimPos);
                    curVisStim.ComputeStimulusPosition();
                }
            }
        }
        // Stop dragging if left click is released at any time
        if (Input.GetMouseButtonUp(0)) {
            dragStart = false;
            dragMoved = false;
        }
    }

    public bool SetStimPositionDegrees(GameObject stimulusObject, Vector2 degreePos)
    {
        Debug.Log(degreePos);

        // TODO: Warning -- degree position is ignoring y!
        //Quaternion angle = Quaternion.Euler(new Vector3(degreePos.x, degreePos.y));
        Quaternion angle = Quaternion.AngleAxis(degreePos.x, Vector3.up);
        RaycastHit hit;
        if (debug)
        {
            Debug.DrawRay(stimCam.transform.position, (angle * Vector3.back) * 100);
        }
        if (Physics.Raycast(stimCam.transform.position, angle * Vector3.back, out hit, Mathf.Infinity, stimPlaneMask))
        {
            SetStimPosition(stimulusObject, hit.point);
            return true;
        }
        else
        {
            Debug.LogWarning("Failed to move stimulus -- movement would take it off screen");
            return false;
        }

    }

    public void SetStimPosition(GameObject stimulusObject, Vector3 point)
    {
        stimulusObject.transform.position = point + offsetVector;
    }

    public void SetTaskRunningState(bool state)
    {
        taskRunning = state;

        if (taskRunning)
        {
            // Need to do something to disable the window so that users don't add stimuli
        }
    }

    /*
     * stimulusType refers to the index in the prefab list
     */
    public GameObject AddNewStimulus(string stimulusType)
    {
        if (visualStimulusPrefabDict.ContainsKey(stimulusType))
        {
            // Spawn new object, parented to the stimPlane
            GameObject newStim = Instantiate(visualStimulusPrefabDict[stimulusType], stimPlane.transform);
            newStim.name = stimulusType + GameObject.FindGameObjectsWithTag("Stimulus").Length;
            newStim.transform.position += offsetVector;
            // TODO: Fix errors with tooltips!!
            //newStim.GetComponent<VisualStimulusTooltip>().SetTooltipObject(visStimTooltip);
            // Register with emanager
            VisualStimulus vs = newStim.GetComponent<VisualStimulus>();
            if (vs)
            {
                emanager.RegisterVisualStimulus(vs);
            }

            return newStim;
        }
        else
        {
            Debug.LogError("(VisStimManager) Failed to add non-existent stimulus type");
            return null;
        }
    }

    public void AddNewStimulusVoid(string stimulusType)
    {
        AddNewStimulus(stimulusType);
    }

    public void DestroyVisualStimulus(GameObject stimulus)
    {
        VisualStimulus vs = stimulus.GetComponent<VisualStimulus>();
        emanager.RemoveVisualStimulus(vs);
        Destroy(stimulus);
    }

    public void ClearAllStimuli()
    {
        foreach (GameObject stimulus in GameObject.FindGameObjectsWithTag("Stimulus"))
        {
            VisualStimulus vs = stimulus.GetComponent<VisualStimulus>();
            if (vs)
            {
                emanager.RemoveVisualStimulus(vs);
            }
            Destroy(stimulus);
        }
    }

    internal void SetContrast(GameObject stim, float contrast)
    {
        Renderer rend = stim.GetComponent<Renderer>();
        Color col = rend.material.color;
        col.a = contrast;
        rend.material.color = col;
    }

    public void DelayedDestroy(GameObject stim, float delay)
    {
        StartCoroutine(DelayedDestroyEnum(stim, delay));
    }
    IEnumerator DelayedDestroyEnum(GameObject stim, float delay)
    {
        yield return new WaitForSeconds(delay);

        DestroyVisualStimulus(stim);
    }
}
