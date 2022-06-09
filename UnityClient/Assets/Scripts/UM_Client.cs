using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO3;
using System;
using System.Linq;
using TMPro;
using Unity.Entities;
using System.Threading.Tasks;
using System.Collections.Specialized;

public class UM_Client : MonoBehaviour
{
    [SerializeField] UM_Launch main;
    [SerializeField] CCFModelControl modelControl;
    [SerializeField] BrainCameraController cameraControl;
    [SerializeField] private bool localhost;

    // UI
    [SerializeField] private GameObject idPanel;
    [SerializeField] private TextMeshProUGUI idInput;

    // VOLUMES
    [SerializeField] private UM_VolumeRenderer volRenderer;

    // NEURONS
    [SerializeField] private GameObject neuronPrefab;
    [SerializeField] private List<Mesh> neuronMeshList;
    [SerializeField] private List<string> neuronMeshNames;
    [SerializeField] private Transform neuronParent;
    private Dictionary<string, GameObject> neurons;

    // PROBES
    [SerializeField] private GameObject probePrefab;
    [SerializeField] private GameObject probeLinePrefab;
    [SerializeField] private Transform probeParent;
    private Dictionary<string, GameObject> probes;
    private Dictionary<string, Vector3[]> probeCoordinates;

    // NODES
    private List<CCFTreeNode> visibleNodes;

    // POSITIONING
    private Vector3 center = new Vector3(5.7f, 4f, -6.6f);

    private string ID;

    SocketManager manager;

    /// <summary>
    /// Unity internal startup function, initializes internal variables and allocates memory
    /// </summary>
    private void Awake()
    {
        neurons = new Dictionary<string, GameObject>();
        probes = new Dictionary<string, GameObject>();
        nodeTasks = new Dictionary<int, Task>();
        probeCoordinates = new Dictionary<string, Vector3[]>();
        visibleNodes = new List<CCFTreeNode>();
    }

    /// <summary>
    /// Unity internal startup function, runs before the first frame Update()
    /// </summary>
    void Start()
    {

        string url = localhost ? "http://localhost:5000" : "https://um-commserver.herokuapp.com/";
        Debug.Log("Attempting to connect: " + url);
        manager = localhost ? new SocketManager(new Uri(url)) : new SocketManager(new Uri(url));

        manager.Socket.On("connect", Connected);

        // CCF Areas
        manager.Socket.On<Dictionary<string, bool>>("SetAreaVisibility", UpdateVisibility);
        manager.Socket.On<Dictionary<string, string>>("SetAreaColors", UpdateColors);
        manager.Socket.On<Dictionary<string, float>>("SetAreaIntensity", UpdateIntensity);
        manager.Socket.On<string>("SetAreaColormap", UpdateVolumeColormap);
        manager.Socket.On<Dictionary<string, string>>("SetAreaShader", UpdateVolumeMaterial);
        manager.Socket.On<Dictionary<string, float>>("SetAreaAlpha", UpdateAlpha);

        // 3D Volumes
        manager.Socket.On<List<object>>("SetVolumeVisibility", UpdateVolumeVisibility);
        //manager.Socket.On<List<float>>("SliceVolume", SetVolumeSlice);
        //manager.Socket.On<Dictionary<string, string>>("SetSliceColor", SetVolumeAnnotationColor);

        // Neurons
        manager.Socket.On<List<string>>("CreateNeurons", CreateNeurons);
        manager.Socket.On<Dictionary<string, List<float>>>("SetNeuronPos", UpdateNeuronPos);
        manager.Socket.On<Dictionary<string, float>>("SetNeuronSize", UpdateNeuronScale);
        manager.Socket.On<Dictionary<string, string>>("SetNeuronShape", UpdateNeuronShape);
        manager.Socket.On<Dictionary<string, string>>("SetNeuronColor", UpdateNeuronColor);

        // Probes
        manager.Socket.On<List<string>>("CreateProbes", CreateProbes);
        manager.Socket.On<Dictionary<string, string>>("SetProbeColors", UpdateProbeColors);
        manager.Socket.On<Dictionary<string, List<float>>>("SetProbePos", UpdateProbePos);
        manager.Socket.On<Dictionary<string, List<float>>>("SetProbeAngles", UpdateProbeAngles);
        manager.Socket.On<Dictionary<string, string>>("SetProbeStyle", UpdateProbeStyle);
        manager.Socket.On<Dictionary<string, List<float>>>("SetProbeSize", UpdateProbeScale);

        // Camera
        manager.Socket.On<List<float>>("SetCameraTarget", SetCameraTarget);
        manager.Socket.On<string>("SetCameraTargetArea", SetCameraTargetArea);
        manager.Socket.On<float>("SetCameraYAngle", SetCameraYAngle);

        // Misc
        manager.Socket.On<string>("Clear", Clear);

        // If we are building to WebGL or to Standalone, switch how you acquire the user's ID
#if UNITY_WEBGL
        // get the url
        string appURL = Application.absoluteURL;
        // parse for query strings
        int queryIdx = appURL.IndexOf("?");
        if (queryIdx > 0)
        {
            string queryString = appURL.Substring(queryIdx);
            NameValueCollection qscoll = System.Web.HttpUtility.ParseQueryString(queryString);
            foreach (string query in qscoll)
            {
                if (query.Equals("ID"))
                {
                    ID = qscoll[query];
                    Debug.Log("Found ID in URL querystring, setting to: " + ID);
                }
            }
        }
#else
        ID = Environment.UserName;
        idInput.text = ID;
        Debug.Log("Setting ID to: " + ID);
#endif
    }

    // VOLUME CONTROLS


    private void UpdateVolumeVisibility(List<object> data)
    {
        if (((string)data[0]).Equals("allen"))
            volRenderer.DisplayAllenVolume();
    }

    // CAMERA CONTROLS

    private void SetCameraYAngle(float obj)
    {
        cameraControl.SetSpin(obj);
    }

    private void SetCameraTargetArea(string obj)
    {
        (int ID, bool leftSide, bool rightSide) = GetID(obj);
        CCFTreeNode node = modelControl.tree.findNode(ID);
        if (node != null)
        {
            Vector3 center = node.GetMeshCenter();
            cameraControl.SetCameraTarget(center);
        }
        else
            main.Log("Failed to find node to set camera target: " + obj);
    }

    private void SetCameraTarget(List<float> obj)
    {
        cameraControl.SetCameraTarget(new Vector3(obj[0], obj[1], obj[2]));
    }

    // UPDATE

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            idPanel.SetActive(!idPanel.activeSelf);
        }
    }

    // CLEAR

    private void Clear(string val)
    {
        switch (val)
        {
            case "all":
                ClearNeurons();
                ClearProbes();
                ClearAreas();
                ClearVolumes();
                break;
            case "neurons":
                ClearNeurons();
                break;
            case "probes":
                ClearProbes();
                break;
            case "areas":
                ClearAreas();
                break;
            case "volumes":
                ClearVolumes();
                break;
        }
    }

    private void ClearNeurons()
    {
        Debug.Log("(Client) Clearing neurons");
        foreach (GameObject neuron in neurons.Values)
            Destroy(neuron);
        neurons = new Dictionary<string, GameObject>();
    }

    private void ClearProbes()
    {
        Debug.Log("(Client) Clearing probes");
        foreach (GameObject probe in probes.Values)
            Destroy(probe);
        probes = new Dictionary<string, GameObject>();
        probeCoordinates = new Dictionary<string, Vector3[]>();
    }

    private void ClearAreas()
    {
        Debug.Log("(Client) Clearing areas");
        foreach (CCFTreeNode node in visibleNodes)
            node.SetNodeModelVisibility(false, false);
        visibleNodes = new List<CCFTreeNode>();
    }

    private void ClearVolumes()
    {
        Debug.Log("(Client) Clearing volumes");
        volRenderer.Clear();
    }

    // PROBE CONTROLS

    private void UpdateProbeStyle(Dictionary<string, string> data)
    {
        main.Log("Not implemented");
    }

    private void UpdateProbeAngles(Dictionary<string, List<float>> data)
    {
        foreach (KeyValuePair<string, List<float>> kvp in data)
        {
            if (probes.ContainsKey(kvp.Key))
            {
                // store coordinates in mlapdv       
                probeCoordinates[kvp.Key][1] = new Vector3(kvp.Value[0], kvp.Value[1], kvp.Value[2]);
                SetProbePositionAndAngles(kvp.Key);
            }
            else
                main.Log("Probe " + kvp.Key + " not found");
        }
    }

    private void UpdateProbePos(Dictionary<string, List<float>> data)
    {
        foreach (KeyValuePair<string, List<float>> kvp in data)
        {
            if (probes.ContainsKey(kvp.Key))
            {
                // store coordinates in mlapdv       
                probeCoordinates[kvp.Key][0] = new Vector3(kvp.Value[0], kvp.Value[1], kvp.Value[2]);
                SetProbePositionAndAngles(kvp.Key);
            }
            else
                main.Log("Probe " + kvp.Key + " not found");
        }
    }

    private void SetProbePositionAndAngles(string probeName)
    {
        Vector3 pos = probeCoordinates[probeName][0];
        Vector3 angles = probeCoordinates[probeName][1];
        Transform probeT = probes[probeName].transform;

        // reset position and angles
        probeT.transform.localPosition = Vector3.zero;
        probeT.localRotation = Quaternion.identity;

        // then translate
        probeT.Translate(new Vector3(-pos.x / 1000f, -pos.z / 1000f, pos.y / 1000f));
        // rotate around azimuth first
        probeT.RotateAround(probeT.position, Vector3.up, -angles.x -90f);
        // then elevation
        probeT.RotateAround(probeT.position, probeT.right, angles.y);
        // then spin
        probeT.RotateAround(probeT.position, probeT.up, angles.z);

    }

    private void UpdateProbeColors(Dictionary<string, string> data)
    {
        foreach (KeyValuePair<string, string> kvp in data)
        {
            if (probes.ContainsKey(kvp.Key))
            {
                Color newColor;
                if (ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
                {
                    Debug.Log("Setting " + kvp.Key + " to " + kvp.Value);
                    probes[kvp.Key].GetComponentInChildren<Renderer>().material.color = newColor;
                }
            }
            else
                main.Log("Probe " + kvp.Key + " not found");
        }
    }

    private void UpdateProbeScale(Dictionary<string, List<float>> data)
    {
        foreach (KeyValuePair<string, List<float>> kvp in data)
        {
            if (probes.ContainsKey(kvp.Key))
            {
                // store coordinates in mlapdv       
                probes[kvp.Key].transform.GetChild(0).localScale = new Vector3(kvp.Value[0], kvp.Value[1], kvp.Value[2]);
            }
            else
                main.Log("Probe " + kvp.Key + " not found");
        }
    }

    private void CreateProbes(List<string> data)
    {
        foreach (string probeName in data)
        {
            GameObject newProbe = Instantiate(probeLinePrefab, probeParent);
            probes.Add(probeName, newProbe);
            probeCoordinates.Add(probeName, new Vector3[2]);
            SetProbePositionAndAngles(probeName);
            main.Log("Created probe: " + probeName);
        }
    }

    private void SetVolumeAnnotationColor(Dictionary<string, string> data)
    {
        main.Log("Not implemented");
    }

    private void SetVolumeSlice(List<float> obj)
    {
        main.Log("Not implemented");
    }

    private void UpdateNeuronScale(Dictionary<string, float> data)
    {
        main.Log("Updating neuron scale");
        foreach (KeyValuePair<string, float> kvp in data)
            neurons[kvp.Key].transform.localScale = Vector3.one * kvp.Value;
    }

    private void UpdateNeuronShape(Dictionary<string, string> data)
    {
        main.Log("Updating neuron shapes");
        foreach (KeyValuePair<string, string> kvp in data)
        {
            if (neuronMeshNames.Contains(kvp.Value))
                neurons[kvp.Key].GetComponent<MeshFilter>().mesh = neuronMeshList[neuronMeshNames.IndexOf(kvp.Value)];
            else
                main.Log("Mesh type: " + kvp.Value + " does not exist");
        }
    }

    private void UpdateNeuronColor(Dictionary<string, string> data)
    {
        main.Log("Updating neuron color");
        foreach (KeyValuePair<string, string> kvp in data)
        {

            Color newColor;
            if (neurons.ContainsKey(kvp.Key) && ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
            {
                neurons[kvp.Key].GetComponent<Renderer>().material.color = newColor;
            }
            else
                main.Log("Failed to set neuron color to: " + kvp.Value);
        }
    }

    // Takes coordinates in ML AP DV in um units
    private void UpdateNeuronPos(Dictionary<string, List<float>> data)
    {
        main.Log("Updating neuron positions");
        foreach (KeyValuePair<string, List<float>> kvp in data)
        {
            neurons[kvp.Key].transform.localPosition = new Vector3(-kvp.Value[0]/1000f, -kvp.Value[2]/1000f, kvp.Value[1]/1000f);
        } 
    }

    private void CreateNeurons(List<string> data)
    {
        foreach (string id in data)
        {
            neurons.Add(id, Instantiate(neuronPrefab, neuronParent));
        }
    }

    private void DeleteNeurons(List<string> data)
    {
        foreach (string id in data)
        {
            neurons.Remove(id);
        }
    }

    private void UpdateVolumeColormap(string data)
    {
        main.ChangeColormap(data);
    }

    /// <summary>
    /// Convert an acronym or ID label into the Allen CCF area ID
    /// </summary>
    /// <param name="idOrAcronym">An ID number (e.g. 0) or an acronym (e.g. "root")</param>
    /// <returns>(int Allen CCF ID, bool left side model, bool right side model)</returns>
    private (int, bool, bool) GetID(string idOrAcronym)
    {
        // Check whether a suffix was included
        int leftIndex = idOrAcronym.IndexOf("-l");
        int rightIndex = idOrAcronym.IndexOf("-r");
        bool leftSide = leftIndex > 0;
        bool rightSide = rightIndex > 0;

        //Remove the suffix
        if (leftSide)
            idOrAcronym = idOrAcronym.Substring(0,leftIndex);
        if (rightSide)
            idOrAcronym = idOrAcronym.Substring(0,rightIndex);

        // If neither suffix is present, then we want to control both sides
        if (!leftSide && !rightSide)
        {
            // set both to true
            leftSide = true;
            rightSide = true;
        }

        // Lowercase
        string lower = idOrAcronym.ToLower();

        // Check for root (special case, which we can't currently handle)
        if (lower.Equals("root") || lower.Equals("void"))
            return (-1, leftSide, rightSide);

        // Figure out what the acronym was by asking CCFModelControl
        if (modelControl.IsAcronym(idOrAcronym))
            return (modelControl.Acronym2ID(idOrAcronym), leftSide, rightSide);
        else
        {
            // It wasn't an acronym, so it has to be an integer
            int ret;
            if (int.TryParse(idOrAcronym, out ret))
                return (ret, leftSide, rightSide);
        }

        // We failed to figure out what this was
        return (-1, leftSide, rightSide);
    }

    ////
    //// VOLUME FUNCTIONS
    /// Note that these are asynchronous calls, because we can't guarantee that the volumes are loaded until the call to setVisibility evaluates fully
    ////

    Dictionary<int, Task> nodeTasks;

    private async void UpdateVolumeMaterial(Dictionary<string, string> data)
    {
        foreach (KeyValuePair<string, string> kvp in data)
        {
            (int ID, bool leftSide, bool rightSide) = GetID(kvp.Key);
            if (WaitingOnTask(ID))
                await nodeTasks[ID];

            if (leftSide && rightSide)
                modelControl.ChangeMaterial(ID, kvp.Value);
            else if (leftSide)
                modelControl.ChangeMaterialOneSided(ID, kvp.Value, true);
            else if (rightSide)
                modelControl.ChangeMaterialOneSided(ID, kvp.Value, false);
        }
    }

    private async void UpdateColors(Dictionary<string, string> data)
    {
        foreach (KeyValuePair<string, string> kvp in data)
        {
            (int ID, bool leftSide, bool rightSide) = GetID(kvp.Key);
            CCFTreeNode node = modelControl.tree.findNode(ID);

            if (WaitingOnTask(node.ID))
                await nodeTasks[node.ID];

            Color newColor;
            if (node != null && ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
                if (leftSide && rightSide)
                    node.SetColor(newColor);
                else if (leftSide)
                    node.SetColorOneSided(newColor, true);
                else if (rightSide)
                    node.SetColorOneSided(newColor, false);
            else
                main.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

    private async void UpdateVisibility(Dictionary<string, bool> data)
    {
        foreach (KeyValuePair<string, bool> kvp in data)
        {
            (int ID, bool leftSide, bool rightSide) = GetID(kvp.Key);
            CCFTreeNode node = modelControl.tree.findNode(ID);

            if (node != null)
            {
                // Check if we already loaded this and whether 
                if (!node.IsLoaded())
                {
                    if (nodeTasks.ContainsKey(node.ID))
                    {
                        main.Log("Node " + node.ID + " is already being loaded, did you send duplicate instructions?");
                    }
                    else
                    {
                        Task nodeTask = node.loadNodeModel(true);
                        nodeTasks.Add(node.ID, nodeTask);
                        await nodeTask;

                        visibleNodes.Add(node);
                        // There is a bug somewhere that forces us to have to do this, if it gets tracked down this can be removed...
                        main.FixNodeTransformPosition(node);
                        // Make sure to fix position before registering!
                        main.RegisterNode(node);
                    }
                }

                if (leftSide && rightSide)
                    node.SetNodeModelVisibility(kvp.Value);
                else if (leftSide)
                    node.SetNodeModelVisibilityLeft(kvp.Value);
                else if (rightSide)
                    node.SetNodeModelVisibilityRight(kvp.Value);
            }
            else
                main.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

    private async void UpdateAlpha(Dictionary<string, float> data)
    {

        foreach (KeyValuePair<string, float> kvp in data)
        {
            (int ID, bool leftSide, bool rightSide) = GetID(kvp.Key);
            CCFTreeNode node = modelControl.tree.findNode(ID);

            if (node != null)
            {
                if (WaitingOnTask(node.ID))
                    await nodeTasks[node.ID];

                if (leftSide && rightSide)
                    node.SetShaderProperty("_Alpha", kvp.Value);
                else if (leftSide)
                    node.SetShaderPropertyOneSided("_Alpha", kvp.Value, true);
                else if (rightSide)
                    node.SetShaderPropertyOneSided("_Alpha", kvp.Value, false);
            }
            else
                main.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }
    private async void UpdateIntensity(Dictionary<string, float> data)
    {

        foreach (KeyValuePair<string, float> kvp in data)
        {
            (int ID, bool leftSide, bool rightSide) = GetID(kvp.Key);
            CCFTreeNode node = modelControl.tree.findNode(ID);

            if (WaitingOnTask(node.ID))
                await nodeTasks[node.ID];

            if (node != null)
            {
                if (leftSide && rightSide)
                    node.SetColor(main.GetColormapColor(kvp.Value));
                else if (leftSide)
                    node.SetColorOneSided(main.GetColormapColor(kvp.Value), true);
                else if (rightSide)
                    node.SetColorOneSided(main.GetColormapColor(kvp.Value), false);
            }
            else
                main.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

    private bool WaitingOnTask(int id)
    {
        if (nodeTasks.ContainsKey(id))
        {
            if (nodeTasks[id].IsCompleted || nodeTasks[id].IsFaulted || nodeTasks[id].IsCanceled)
            {
                nodeTasks.Remove(id);
                return false;
            }
            return true;
        }
        return false;
    }


    ////
    //// SOCKET FUNCTIONS
    ////
    public void UpdateID(string newID)
    {
        main.Log("Updating ID to : " + newID);
        ID = newID;
        manager.Socket.Emit("ID", new List<string>() { ID, "receive" });
    }

    private void OnDestroy()
    {
        manager.Close();
    }

    private void Connected()
    {
        main.Log("connected! Login with ID: " + ID);
        UpdateID(ID);
    }

}
