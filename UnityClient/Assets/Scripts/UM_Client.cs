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
using System.Text;

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
    private int[] missing = { 738, 995 };

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
        manager.Socket.On<Dictionary<string, string>>("SetAreaMaterial", UpdateVolumeMaterial);
        manager.Socket.On<Dictionary<string, float>>("SetAreaAlpha", UpdateAlpha);
        manager.Socket.On<Dictionary<string, List<float>>>("SetAreaData", UpdateAreaData);
        manager.Socket.On<int>("SetAreaIndex", UpdateAreaDataIndex);
        manager.Socket.On<string>("LoadDefaultAreas", LoadDefaultAreas);

        // 3D Volumes
        manager.Socket.On<List<object>>("SetVolumeVisibility", UpdateVolumeVisibility);
        manager.Socket.On<List<object>>("SetVolumeDataMeta", UpdateVolumeMeta);
        manager.Socket.On<byte[]>("SetVolumeData", UpdateVolumeData);
        manager.Socket.On<string>("CreateVolume", CreateVolume);
        manager.Socket.On<List<string>>("SetVolumeColormap", SetVolumeColormap);

        // Neurons
        manager.Socket.On<List<string>>("CreateNeurons", CreateNeurons);
        manager.Socket.On<Dictionary<string, List<float>>>("SetNeuronPos", UpdateNeuronPos);
        manager.Socket.On<Dictionary<string, float>>("SetNeuronSize", UpdateNeuronScale);
        manager.Socket.On<Dictionary<string, string>>("SetNeuronShape", UpdateNeuronShape);
        manager.Socket.On<Dictionary<string, string>>("SetNeuronColor", UpdateNeuronColor);
        manager.Socket.On<Dictionary<string, string>>("SetNeuronMaterial", UpdateNeuronMaterial);

        // Probes
        manager.Socket.On<List<string>>("CreateProbes", CreateProbes);
        manager.Socket.On<Dictionary<string, string>>("SetProbeColors", UpdateProbeColors);
        manager.Socket.On<Dictionary<string, List<float>>>("SetProbePos", UpdateProbePos);
        manager.Socket.On<Dictionary<string, List<float>>>("SetProbeAngles", UpdateProbeAngles);
        manager.Socket.On<Dictionary<string, string>>("SetProbeStyle", UpdateProbeStyle);
        manager.Socket.On<Dictionary<string, List<float>>>("SetProbeSize", UpdateProbeScale);

        // Camera
        manager.Socket.On<List<float>>("SetCameraTarget", SetCameraTarget);
        manager.Socket.On<List<float>>("SetCameraPosition", SetCameraPosition);
        manager.Socket.On<List<float>>("SetCameraRotation", SetCameraRotation);
        manager.Socket.On<string>("SetCameraTargetArea", SetCameraTargetArea);

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
            Debug.Log("Found query string");
            string queryString = appURL.Substring(queryIdx);
            Debug.Log(queryString);
            NameValueCollection qscoll = System.Web.HttpUtility.ParseQueryString(queryString);
            foreach (string query in qscoll)
            {
                Debug.Log(query);
                Debug.Log(qscoll[query]);
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
            volRenderer.DisplayAllenVolume((bool)data[1]);
        else
            volRenderer.SetVolumeVisibility((string)data[0], (bool)data[1]);
    }

    private void SetVolumeColormap(List<string> obj)
    {
        string name = obj[0];
        obj.RemoveAt(0);
        volRenderer.SetVolumeColormap(name, obj);
    }

    private void CreateVolume(string name)
    {
        volRenderer.CreateVolume(name);
    }

    private void UpdateVolumeMeta(List<object> data)
    {
        volRenderer.AddVolumeMeta((string)data[0], (int)data[1], (bool)data[2]);
    }
    private void UpdateVolumeData(byte[] bytes)
    {
        volRenderer.AddVolumeData(bytes);
    }
    //void OnFrame(Socket socket, Packet packet, params object[] args)
    //{
    //    texture.LoadImage(packet.Attachments[0]);
    //}

    // CAMERA CONTROLS


    private void SetCameraRotation(List<float> obj)
    {
        cameraControl.SetBrainAxisAngles(new Vector3(obj[1], obj[0], obj[2]));
    }

    private void SetCameraPosition(List<float> obj)
    {
        // position in ml/ap/dv relative to ccf 0,0,0
        LogError("Setting camera position not implemented yet. Use set_camera_target and set_camera_rotation instead.");
        //Vector3 ccfPosition25 = new Vector3(obj[0]/25, obj[1]/25, obj[2]/25);
        //cameraControl.SetOffsetPosition(Utils.apdvlr2World(ccfPosition25));
    }

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
            Vector3 center = node.GetMeshCenter(leftSide, rightSide);
            cameraControl.SetCameraTarget(center);
        }
        else
            main.Log("Failed to find node to set camera target: " + obj);
    }

    private void SetCameraTarget(List<float> mlapdv)
    {
        // data comes in in um units in ml/ap/dv
        // note that (0,0,0) in world is the center of the brain
        // so offset by (-6.6 ap, -4 dv, -5.7 lr) to get to the corner
        // in world space, x = ML, y = DV, z = AP

        Vector3 worldCoords = new Vector3(5.7f - mlapdv[0]/1000f, 4f - mlapdv[2] / 1000f, mlapdv[1] / 1000f - 6.6f);
        cameraControl.SetCameraTarget(worldCoords);
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

    // NEURONS


    private void UpdateNeuronMaterial(Dictionary<string, string> obj)
    {
        throw new NotImplementedException();
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
        int leftIndex = idOrAcronym.IndexOf("-lh");
        int rightIndex = idOrAcronym.IndexOf("-rh");
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
    //// AREA FUNCTIONS
    /// Note that these are asynchronous calls, because we can't guarantee that the volumes are loaded until the call to setVisibility evaluates fully
    ////

    Dictionary<int, Task> nodeTasks;
    private int areaDataIndex;
    private Dictionary<int, List<float>> areaData;
    private Dictionary<int, (bool, bool)> areaSides;
    private void UpdateAreaDataIndex(int obj)
    {
        SetAreaDataIndex(obj);
    }

    private void UpdateAreaData(Dictionary<string, List<float>> newAreaData)
    {
        foreach (KeyValuePair<string, List<float>> kvp in newAreaData)
        {
            (int ID, bool leftSide, bool rightSide) = GetID(kvp.Key);

            if (areaData.ContainsKey(ID))
            {
                areaData[ID] = kvp.Value;
                areaSides[ID] = (leftSide, rightSide);
            }
            else
            {
                areaData.Add(ID, kvp.Value);
                areaSides.Add(ID, (leftSide, rightSide));
            }
        }
    }

    public void SetAreaDataIndex(int newIndex)
    {
        areaDataIndex = newIndex;
        UpdateAreaDataIntensity();
    }

    private async void UpdateAreaDataIntensity()
    {
        foreach (KeyValuePair<int, List<float>> kvp in areaData)
        {
            int ID = kvp.Key;
            (bool leftSide, bool rightSide) = areaSides[ID];

            CCFTreeNode node = modelControl.tree.findNode(ID);
            
            if (WaitingOnTask(node.ID))
                await nodeTasks[node.ID];

            float currentValue = kvp.Value[areaDataIndex];

            if (node != null)
            {
                if (leftSide && rightSide)
                    node.SetColor(main.GetColormapColor(currentValue), true);
                else if (leftSide)
                    node.SetColorOneSided(main.GetColormapColor(currentValue), true, true);
                else if (rightSide)
                    node.SetColorOneSided(main.GetColormapColor(currentValue), false, true);
            }
            else
                main.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }


    private async void LoadDefaultAreas(string whichNodes)
    {
        Task<List<CCFTreeNode>> nodeTask;
        if (whichNodes.Equals("cosmos"))
            nodeTask = modelControl.LoadCosmosNodes(true);
        else if (whichNodes.Equals("beryl"))
            nodeTask = modelControl.LoadBerylNodes(true);
        else
        {
            main.Log("Failed to load nodes: " + whichNodes);
            LogError("Node group " + whichNodes + " does not exist.");
            return;
        }

        await nodeTask;

        foreach (CCFTreeNode node in nodeTask.Result)
        {
            node.SetNodeModelVisibility(true, true);
            visibleNodes.Add(node);
            // There is a bug somewhere that forces us to have to do this, if it gets tracked down this can be removed...
            main.FixNodeTransformPosition(node);
            // Make sure to fix position before registering!
            main.RegisterNode(node);
        }
    }

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

            Color newColor = Color.black;
            if (node != null && ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
                if (WaitingOnTask(node.ID))
                    await nodeTasks[node.ID];

                if (leftSide && rightSide)
                    node.SetColor(newColor, true);
                else if (leftSide)
                    node.SetColorOneSided(newColor, true, true);
                else if (rightSide)
                    node.SetColorOneSided(newColor, false, true);
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

            if (node != null && !node.IsLoaded())
            {
                if (missing.Contains(node.ID))
                {
                    LogWarning("The mesh file for area " + node.ID + " does not exist, we can't load it");
                    continue;
                }
                if (nodeTasks.ContainsKey(node.ID))
                {
                    main.Log("Node " + node.ID + " is already being loaded, did you send duplicate instructions?");
                }
                else
                {
                    Task nodeTask = node.loadNodeModel(true);
                    nodeTasks.Add(node.ID, nodeTask);
                }
            }
        }

        await Task.WhenAll(nodeTasks.Values);

        foreach (KeyValuePair<string, bool> kvp in data)
        {
            (int ID, bool leftSide, bool rightSide) = GetID(kvp.Key);
            CCFTreeNode node = modelControl.tree.findNode(ID);

            if (node != null && node.IsLoaded())
            {
                visibleNodes.Add(node);
                // There is a bug somewhere that forces us to have to do this, if it gets tracked down this can be removed...
                main.FixNodeTransformPosition(node);
                // Make sure to fix position before registering!
                main.RegisterNode(node);

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

            if (node != null)
            {
                if (WaitingOnTask(node.ID))
                    await nodeTasks[node.ID];

                if (leftSide && rightSide)
                    node.SetColor(main.GetColormapColor(kvp.Value), true);
                else if (leftSide)
                    node.SetColorOneSided(main.GetColormapColor(kvp.Value), true, true);
                else if (rightSide)
                    node.SetColorOneSided(main.GetColormapColor(kvp.Value), false, true);
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

        //manager.Socket.Emit("log", "test log");
        //manager.Socket.Emit("log-warning", "test warning");
        //manager.Socket.Emit("log-error", "test error");
    }

    private void Log(string msg)
    {
        main.Log("Sending message to client: " + msg);
        manager.Socket.Emit("log", msg);
    }

    private void LogWarning(string msg)
    {
        main.Log("Sending warning to client: " + msg);
        manager.Socket.Emit("log-warning", msg);
    }

    private void LogError(string msg)
    {
        main.Log("Sending error to client: " + msg);
        manager.Socket.Emit("log-error", msg);
    }
}
