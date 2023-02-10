using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO3;
using System;
using System.Linq;
using TMPro;
using System.Threading.Tasks;

/// <summary>
/// Entry point for all client-side messages coming from the Python API
/// Handles messages and tracks creation/destruction of prefab objects
/// 
/// TODO:
///  - needs a refactor to separate neurons/probes/text/areas and put them in their own components
/// </summary>
public class Client : MonoBehaviour
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

    // TEXT
    [SerializeField] private GameObject textParent;
    [SerializeField] private GameObject textPrefab;
    private Dictionary<string, GameObject> texts;

    #region Managers
    [SerializeField] private LineRendererManager _lineRendererManager;
    [SerializeField] private PrimitiveMeshManager _primitiveMeshManager;
    [SerializeField] private ProbeManager _probeManager;
    [SerializeField] private AreaManager _areaManager;
    #endregion
    //Line Renderer

    //Primitive Mesh Renderer

    // NODES

    private string ID;

    private static SocketManager manager;

    /// <summary>
    /// Unity internal startup function, initializes internal variables and allocates memory
    /// </summary>
    private void Awake()
    {
        neurons = new Dictionary<string, GameObject>();
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
        manager.Socket.On<Dictionary<string, bool>>("SetAreaVisibility", _areaManager.SetAreaVisibility);
        manager.Socket.On<Dictionary<string, string>>("SetAreaColors", _areaManager.SetAreaColor);
        manager.Socket.On<Dictionary<string, float>>("SetAreaIntensity", _areaManager.SetAreaColorIntensity);
        manager.Socket.On<string>("SetAreaColormap", _areaManager.SetAreaColormap);
        manager.Socket.On<Dictionary<string, string>>("SetAreaMaterial", _areaManager.SetAreaMaterial);
        manager.Socket.On<Dictionary<string, float>>("SetAreaAlpha", _areaManager.SetAreaAlpha);
        manager.Socket.On<Dictionary<string, List<float>>>("SetAreaData", _areaManager.SetAreaData);
        manager.Socket.On<int>("SetAreaIndex", _areaManager.SetAreaDataIndex);
        manager.Socket.On<string>("LoadDefaultAreas", _areaManager.LoadDefaultAreas);

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
        manager.Socket.On<List<string>>("CreateProbes", _probeManager.CreateProbes);
        manager.Socket.On<List<string>>("DeleteProbes", _probeManager.DeleteProbes);
        manager.Socket.On<Dictionary<string, string>>("SetProbeColors", _probeManager.SetProbeColor);
        manager.Socket.On<Dictionary<string, List<float>>>("SetProbePos", _probeManager.SetProbePosition);
        manager.Socket.On<Dictionary<string, List<float>>>("SetProbeAngles", _probeManager.SetProbeAngle);
        manager.Socket.On<Dictionary<string, string>>("SetProbeStyle", _probeManager.SetProbeStyle);
        manager.Socket.On<Dictionary<string, List<float>>>("SetProbeSize", _probeManager.SetProbeScale);

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

        // Line Renderer
        manager.Socket.On<List<string>>("CreateLine", _lineRendererManager.CreateLine);
        manager.Socket.On<Dictionary<string, List<List<float>>>>("SetLinePosition", _lineRendererManager.SetLinePosition);
        manager.Socket.On<List<string>>("DeleteLine", _lineRendererManager.DeleteLine);
        manager.Socket.On<Dictionary<string, string>>("SetLineColor", _lineRendererManager.SetLineColor);

        //Primitive Mesh Renderer
        manager.Socket.On<List<string>>("CreateMesh", _primitiveMeshManager.CreateMesh);
        manager.Socket.On<List<string>>("DeleteMesh", _primitiveMeshManager.DeleteMesh);
        manager.Socket.On<Dictionary<string, List<float>>>("SetPosition", _primitiveMeshManager.SetPosition);
        manager.Socket.On<Dictionary<string, List<float>>>("SetScale", _primitiveMeshManager.SetScale);
        manager.Socket.On<Dictionary<string, string>>("SetColor", _primitiveMeshManager.SetColor);

        // Misc
        manager.Socket.On<string>("Clear", Clear);

        // If we are building to WebGL or to Standalone, switch how you acquire the user's ID
#if UNITY_EDITOR && UNITY_EDITOR_WIN 
        ID = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        if (ID.Contains("\\"))
        {
            int idx = ID.IndexOf("\\");
            ID = ID.Substring(idx + 1, ID.Length - idx - 1);
        }
        //ID = "Jasmine Schoch";
        idInput.text = ID;
        Debug.Log("Setting ID to: " + ID);
#elif UNITY_WEBGL
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
        (int ID, bool full, bool leftSide, bool rightSide) = _areaManager.GetID(obj);
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
            UM_Launch.Log("Failed to find node to set camera target: " + obj);
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
                _probeManager.ClearProbes();
                _areaManager.ClearAreas();
                ClearVolumes();
                ClearTexts();
                break;
            case "neurons":
                ClearNeurons();
                break;
            case "probes":
                _probeManager.ClearProbes();
                break;
            case "areas":
                _areaManager.ClearAreas();
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

    private void SetVolumeAnnotationColor(Dictionary<string, string> data)
    {
        UM_Launch.Log("Not implemented");
    }

    private void SetVolumeSlice(List<float> obj)
    {
        UM_Launch.Log("Not implemented");
    }

    // NEURONS


    private void UpdateNeuronMaterial(Dictionary<string, string> obj)
    {
        throw new NotImplementedException();
    }

    private void UpdateNeuronScale(Dictionary<string, float> data)
    {
        UM_Launch.Log("Updating neuron scale");
        foreach (KeyValuePair<string, float> kvp in data)
            neurons[kvp.Key].transform.localScale = Vector3.one * kvp.Value;
    }

    private void UpdateNeuronShape(Dictionary<string, string> data)
    {
        UM_Launch.Log("Updating neuron shapes");
        foreach (KeyValuePair<string, string> kvp in data)
        {
            if (neuronMeshNames.Contains(kvp.Value))
                neurons[kvp.Key].GetComponent<MeshFilter>().mesh = neuronMeshList[neuronMeshNames.IndexOf(kvp.Value)];
            else
                UM_Launch.Log("Mesh type: " + kvp.Value + " does not exist");
        }
    }

    private void UpdateNeuronColor(Dictionary<string, string> data)
    {
        UM_Launch.Log("Updating neuron color");
        foreach (KeyValuePair<string, string> kvp in data)
        {

            Color newColor;
            if (neurons.ContainsKey(kvp.Key) && ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
            {
                neurons[kvp.Key].GetComponent<Renderer>().material.color = newColor;
            }
            else
                UM_Launch.Log("Failed to set neuron color to: " + kvp.Value);
        }
    }

    // Takes coordinates in ML AP DV in um units
    private void UpdateNeuronPos(Dictionary<string, List<float>> data)
    {
        UM_Launch.Log("Updating neuron positions");
        foreach (KeyValuePair<string, List<float>> kvp in data)
        {
            neurons[kvp.Key].transform.localPosition = new Vector3(-kvp.Value[0]/1000f, -kvp.Value[2]/1000f, kvp.Value[1]/1000f);
        } 
    }

    private void CreateNeurons(List<string> data)
    {
        UM_Launch.Log("Creating neurons");
        foreach (string id in data)
        {
            neurons.Add(id, Instantiate(neuronPrefab, neuronParent));
        }
    }

    private void DeleteNeurons(List<string> data)
    {
        UM_Launch.Log("Deleting neurons");
        foreach (string id in data)
        {
            Destroy(neurons[id]);
            neurons.Remove(id);
        }
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
        UM_Launch.Log("Updating ID to : " + newID);
        ID = newID;
        manager.Socket.Emit("ID", new List<string>() { ID, "receive" });
    }

    private void OnDestroy()
    {
        manager.Close();
    }

    private void Connected()
    {
        UM_Launch.Log("connected! Login with ID: " + ID);
        UpdateID(ID);
    }

    public static void Log(string msg)
    {
        UM_Launch.Log("Sending message to client: " + msg);
        manager.Socket.Emit("log", msg);
    }

    public static void LogWarning(string msg)
    {
        UM_Launch.Log("Sending warning to client: " + msg);
        manager.Socket.Emit("log-warning", msg);
    }

    public static void LogError(string msg)
    {
        UM_Launch.Log("Sending error to client: " + msg);
        manager.Socket.Emit("log-error", msg);
    }
}
