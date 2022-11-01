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

/// <summary>
/// Entry point for all client-side messages coming from the Python API
/// Handles messages and tracks creation/destruction of prefab objects
/// 
/// TODO:
///  - needs a refactor to separate neurons/probes/text/areas and put them in their own components
/// </summary>
public class UM_Client : MonoBehaviour
{
    [SerializeField] UM_Launch main;
    [SerializeField] CCFModelControl modelControl;
    [SerializeField] BrainCameraController cameraControl;
    [SerializeField] private bool localhost;

    // UI
    [SerializeField] private Canvas uiCanvas;
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

    // TEXT
    [SerializeField] private GameObject textParent;
    [SerializeField] private GameObject textPrefab;
    private Dictionary<string, GameObject> texts;

    // NODES
    private List<CCFTreeNode> visibleNodes;
    private int[] missing = { 738, 995 };

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
        texts = new Dictionary<string, GameObject>();
    }

    /// <summary>
    /// Unity internal startup function, runs before the first frame Update()
    /// </summary>
    void Start()
    {
        // Only allow localhost when running in the editor
#if UNITY_EDITOR
        string url = localhost ? "http://localhost:5000" : "https://urchin-commserver.herokuapp.com/";
#else
        string url = "https://urchin-commserver.herokuapp.com/";
#endif
        Debug.Log("Attempting to connect: " + url);
        manager = localhost ? new SocketManager(new Uri(url)) : new SocketManager(new Uri(url));

        manager.Socket.On("connect", Connected);

        // CCF Areas
        manager.Socket.On<Dictionary<string, bool>>("SetAreaVisibility", SetAreaVisibility);
        manager.Socket.On<Dictionary<string, string>>("SetAreaColors", SetAreaColors);
        manager.Socket.On<Dictionary<string, float>>("SetAreaIntensity", SetAreaIntensity);
        manager.Socket.On<string>("SetAreaColormap", SetAreaColormap);
        manager.Socket.On<Dictionary<string, string>>("SetAreaMaterial", SetAreaMaterial);
        manager.Socket.On<Dictionary<string, float>>("SetAreaAlpha", SetAreaAlpha);
        manager.Socket.On<Dictionary<string, List<float>>>("SetAreaData", SetAreaData);
        manager.Socket.On<int>("SetAreaIndex", SetAreaIndex);
        manager.Socket.On<string>("LoadDefaultAreas", LoadDefaultAreas);

        // 3D Volumes
        manager.Socket.On<List<object>>("SetVolumeVisibility", UpdateVolumeVisibility);
        manager.Socket.On<List<object>>("SetVolumeDataMeta", UpdateVolumeMeta);
        manager.Socket.On<byte[]>("SetVolumeData", UpdateVolumeData);
        manager.Socket.On<string>("CreateVolume", CreateVolume);
        manager.Socket.On<string>("DeleteVolume", DeleteVolume);
        manager.Socket.On<List<string>>("SetVolumeColormap", SetVolumeColormap);

        // Neurons
        manager.Socket.On<List<string>>("CreateNeurons", CreateNeurons);
        manager.Socket.On<List<string>>("DeleteNeurons", DeleteNeurons);
        manager.Socket.On<Dictionary<string, List<float>>>("SetNeuronPos", UpdateNeuronPos);
        manager.Socket.On<Dictionary<string, float>>("SetNeuronSize", UpdateNeuronScale);
        manager.Socket.On<Dictionary<string, string>>("SetNeuronShape", UpdateNeuronShape);
        manager.Socket.On<Dictionary<string, string>>("SetNeuronColor", UpdateNeuronColor);
        manager.Socket.On<Dictionary<string, string>>("SetNeuronMaterial", UpdateNeuronMaterial);

        // Probes
        manager.Socket.On<List<string>>("CreateProbes", CreateProbes);
        manager.Socket.On<List<string>>("DeleteProbes", DeleteProbes);
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
        manager.Socket.On<float>("SetCameraZoom", SetCameraZoom);
        manager.Socket.On<List<float>>("SetCameraPan", SetCameraPan);


        // Text
        manager.Socket.On<List<string>>("CreateText", CreateText);
        manager.Socket.On<List<string>>("DeleteText", DeleteText);
        manager.Socket.On<Dictionary<string, string>>("SetTextText", SetText);
        manager.Socket.On<Dictionary<string, string>>("SetTextColors", SetTextColors);
        manager.Socket.On<Dictionary<string, int>>("SetTextSizes", SetTextSizes);
        manager.Socket.On<Dictionary<string, List<float>>>("SetTextPositions", SetTextPositions);

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

    private void DeleteVolume(string name)
    {
        volRenderer.DeleteVolume(name);
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

    private void SetCameraZoom(float obj)
    {
        cameraControl.SetZoom(obj);
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
        (int ID, bool full, bool leftSide, bool rightSide) = GetID(obj);
        CCFTreeNode node = modelControl.tree.findNode(ID);
        if (node != null)
        {
            Vector3 center;
            if (full)
                center = node.GetMeshCenterFull();
            else
                center = node.GetMeshCenterSided(leftSide);
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

    private void SetCameraPan(List<float> panXY)
    {
        cameraControl.SetCameraPan(new Vector2(panXY[0], panXY[1]));
    }

#region Clear

    private void Clear(string val)
    {
        switch (val)
        {
            case "all":
                ClearNeurons();
                ClearProbes();
                ClearAreas();
                ClearVolumes();
                ClearTexts();
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
            case "texts":
                ClearTexts();
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
        {
            Debug.Log("Clearing: " + node.Name);
            node.SetNodeModelVisibility_Full(false);
            node.SetNodeModelVisibility_Left(false);
            node.SetNodeModelVisibility_Right(false);
        }
        visibleNodes = new List<CCFTreeNode>();
    }

    private void ClearVolumes()
    {
        Debug.Log("(Client) Clearing volumes");
        volRenderer.Clear();
    }

    private void ClearTexts()
    {
        Debug.Log("(Client) Clearing text");
        foreach (GameObject text in texts.Values)
            Destroy(text);
        texts = new Dictionary<string, GameObject>();
    }

#endregion

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
                probes[kvp.Key].transform.GetChild(0).localPosition = new Vector3(0f, kvp.Value[1] / 2, 0f);
            }
            else
                main.Log("Probe " + kvp.Key + " not found");
        }
    }

    private void CreateProbes(List<string> data)
    {
        foreach (string probeName in data)
        {
            if (!probes.ContainsKey(probeName))
            {
                GameObject newProbe = Instantiate(probeLinePrefab, probeParent);
                probes.Add(probeName, newProbe);
                probeCoordinates.Add(probeName, new Vector3[2]);
                SetProbePositionAndAngles(probeName);
                main.Log("Created probe: " + probeName);
            }
            else
            {
                LogError(string.Format("Probe {0} already exists in the scene",probeName));
            }
        }
    }

    private void DeleteProbes(List<string> data)
    {
        foreach (string probeName in data)
        {
            Destroy(probes[probeName]);
            probes.Remove(probeName);
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
        main.Log("Creating neurons");
        foreach (string id in data)
        {
            neurons.Add(id, Instantiate(neuronPrefab, neuronParent));
        }
    }

    private void DeleteNeurons(List<string> data)
    {
        main.Log("Deleting neurons");
        foreach (string id in data)
        {
            Destroy(neurons[id]);
            neurons.Remove(id);
        }
    }

    private void SetAreaColormap(string data)
    {
        main.ChangeColormap(data);
    }

    /// <summary>
    /// Convert an acronym or ID label into the Allen CCF area ID
    /// </summary>
    /// <param name="idOrAcronym">An ID number (e.g. 0) or an acronym (e.g. "root")</param>
    /// <returns>(int Allen CCF ID, bool left side model, bool right side model)</returns>
    private (int ID, bool full, bool leftSide, bool rightSide) GetID(string idOrAcronym)
    {
        // Check whether a suffix was included
        int leftIndex = idOrAcronym.IndexOf("-lh");
        int rightIndex = idOrAcronym.IndexOf("-rh");
        bool leftSide = leftIndex > 0;
        bool rightSide = rightIndex > 0;
        bool full = !(leftSide || rightSide);

        //Remove the suffix
        if (leftSide)
            idOrAcronym = idOrAcronym.Substring(0,leftIndex);
        if (rightSide)
            idOrAcronym = idOrAcronym.Substring(0,rightIndex);

        // Lowercase
        string lower = idOrAcronym.ToLower();

        // Check for root (special case, which we can't currently handle)
        if (lower.Equals("root") || lower.Equals("void"))
            return (-1, full, leftSide, rightSide);

        // Figure out what the acronym was by asking CCFModelControl
        if (modelControl.IsAcronym(idOrAcronym))
            return (modelControl.Acronym2ID(idOrAcronym), full, leftSide, rightSide);
        else
        {
            // It wasn't an acronym, so it has to be an integer
            int ret;
            if (int.TryParse(idOrAcronym, out ret))
                return (ret, full, leftSide, rightSide);
        }

        // We failed to figure out what this was
        return (-1, full, leftSide, rightSide);
    }

    ////
    //// AREA FUNCTIONS
    /// Note that these are asynchronous calls, because we can't guarantee that the volumes are loaded until the call to setVisibility evaluates fully
    ////

    Dictionary<int, Task> nodeTasks;
    private int areaDataIndex;
    private Dictionary<int, List<float>> areaData;
    private Dictionary<int, (bool, bool)> areaSides;
    private void SetAreaIndex(int obj)
    {
        SetAreaDataIndex(obj);
    }

    private void SetAreaData(Dictionary<string, List<float>> newAreaData)
    {
        foreach (KeyValuePair<string, List<float>> kvp in newAreaData)
        {
            (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);

            Debug.LogWarning("Might be broken with new data loading");
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
            nodeTask = modelControl.LoadCosmosNodes(false);
        else if (whichNodes.Equals("beryl"))
            nodeTask = modelControl.LoadBerylNodes(false);
        else
        {
            main.Log("Failed to load nodes: " + whichNodes);
            LogError("Node group " + whichNodes + " does not exist.");
            return;
        }

        await nodeTask;

        foreach (CCFTreeNode node in nodeTask.Result)
        {
            node.SetNodeModelVisibility_Left(true);
            node.SetNodeModelVisibility_Right(true);
            visibleNodes.Add(node);
            // Make sure to fix position before registering!
            main.RegisterNode(node);
        }
    }

    private async void SetAreaMaterial(Dictionary<string, string> data)
    {
        foreach (KeyValuePair<string, string> kvp in data)
        {
            (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);
            if (WaitingOnTask(ID))
                await nodeTasks[ID];

            if (full)
                modelControl.ChangeMaterial(ID, kvp.Value);
            else if (leftSide)
                modelControl.ChangeMaterialOneSided(ID, kvp.Value, true);
            else if (rightSide)
                modelControl.ChangeMaterialOneSided(ID, kvp.Value, false);
        }
    }

    private async void SetAreaColors(Dictionary<string, string> data)
    {
        foreach (KeyValuePair<string, string> kvp in data)
        {
            (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);
            CCFTreeNode node = modelControl.tree.findNode(ID);

            Color newColor = Color.black;
            if (node != null && ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
                if (WaitingOnTask(node.ID))
                    await nodeTasks[node.ID];

                if (full)
                    node.SetColor(newColor, true);
                else if (leftSide)
                    node.SetColorOneSided(newColor, true, true);
                else if (rightSide)
                    node.SetColorOneSided(newColor, false, true);
            else
                main.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

    private void SetAreaVisibility(Dictionary<string, bool> data)
    {
        foreach (KeyValuePair<string, bool> kvp in data)
        {
            (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);
            CCFTreeNode node = modelControl.tree.findNode(ID);

            if (node == null)
                return;

            if (missing.Contains(node.ID))
            {
                LogWarning("The mesh file for area " + node.ID + " does not exist, we can't load it");
                continue;
            }
            if (nodeTasks.ContainsKey(node.ID))
            {
                main.Log("Node " + node.ID + " is already being loaded, did you send duplicate instructions?");
                continue;
            }

            bool set = false;

            if (full && node.IsLoaded(true))
            {
                node.SetNodeModelVisibility_Full(kvp.Value);
                visibleNodes.Add(node);
                set = true;
            }
            if (leftSide && node.IsLoaded(false))
            {
                set = true;
                node.SetNodeModelVisibility_Left(kvp.Value);
                visibleNodes.Add(node);
            }
            if (rightSide && node.IsLoaded(false))
            {
                node.SetNodeModelVisibility_Right(kvp.Value);
                visibleNodes.Add(node);
                set = true;
            }

            if (!set)
                LoadIndividualArea(ID, full, leftSide, rightSide, kvp.Value);
        }
    }

    private async void LoadIndividualArea(int ID, bool full, bool leftSide, bool rightSide, bool visibility)
    {
        CCFTreeNode node = modelControl.tree.findNode(ID);
        visibleNodes.Add(node);

        node.LoadNodeModel(full, leftSide || rightSide);
        await node.GetLoadedTask(full);

        if (full)
            node.SetNodeModelVisibility_Full(visibility);
        if (leftSide)
            node.SetNodeModelVisibility_Left(visibility);
        if (rightSide)
            node.SetNodeModelVisibility_Right(visibility);
    }

    private async void SetAreaAlpha(Dictionary<string, float> data)
    {

        foreach (KeyValuePair<string, float> kvp in data)
        {
            (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);
            CCFTreeNode node = modelControl.tree.findNode(ID);

            if (node != null)
            {
                if (WaitingOnTask(node.ID))
                    await nodeTasks[node.ID];

                if (full)
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
    private async void SetAreaIntensity(Dictionary<string, float> data)
    {

        foreach (KeyValuePair<string, float> kvp in data)
        {
            (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);
            CCFTreeNode node = modelControl.tree.findNode(ID);

            if (node != null)
            {
                if (WaitingOnTask(node.ID))
                    await nodeTasks[node.ID];

                if (full)
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

#region Text

    private void CreateText(List<string> data)
    {
        Debug.Log("Creating text");
        foreach (string textName in data)
        {
            if (!texts.ContainsKey(textName))
            {
                GameObject textGO = Instantiate(textPrefab, textParent.transform);
                texts.Add(textName, textGO);
            }
        }
    }

    private void DeleteText(List<string> data)
    {
        Debug.Log("Deleting text");
        foreach (string textName in data)
        {
            if (texts.ContainsKey(textName))
            {
                Destroy(texts[textName]);
                texts.Remove(textName);
            }
        }
    }

    private void SetText(Dictionary<string, string> data)
    {
        Debug.Log("Setting text");
        foreach (KeyValuePair<string, string> kvp in data)
        {
            if (texts.ContainsKey(kvp.Key))
                texts[kvp.Key].GetComponent<TMP_Text>().text = kvp.Value;
        }
    }

    private void SetTextColors(Dictionary<string, string> data)
    {
        Debug.Log("Setting text colors");
        foreach (KeyValuePair<string, string> kvp in data)
        {
            Color newColor;
            if (texts.ContainsKey(kvp.Key) && ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
            {
                texts[kvp.Key].GetComponent<TMP_Text>().color = newColor;
            }
            else
                LogError("Failed to set text color to: " + kvp.Value);
        }
    }

    private void SetTextSizes(Dictionary<string, int> data)
    {
        Debug.Log("Setting text sizes");
        foreach (KeyValuePair<string, int> kvp in data)
        {
            if (texts.ContainsKey(kvp.Key))
                texts[kvp.Key].GetComponent<TMP_Text>().fontSize = kvp.Value;
        }
    }

    private void SetTextPositions(Dictionary<string, List<float>> data)
    {
#if UNITY_EDITOR
        Debug.Log("Setting text positions");
#endif
        Vector2 canvasWH = new Vector2(uiCanvas.GetComponent<RectTransform>().rect.width, uiCanvas.GetComponent<RectTransform>().rect.height);
        foreach (KeyValuePair<string, List<float>> kvp in data)
        {
            if (texts.ContainsKey(kvp.Key))
            {
                texts[kvp.Key].transform.localPosition = new Vector2(canvasWH.x * kvp.Value[0] / 2, canvasWH.y * kvp.Value[1] / 2);
            }
            else
            {
                Debug.Log("Couldn't set position for " + kvp.Key);
                LogError(string.Format("Couldn't set position of {0}", kvp.Key));
            }
        }
    } 
     
#endregion

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
