using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO3;
using System;
using Urchin.Managers;

namespace Urchin.API
{/// <summary>
 /// Entry point for all client-side messages coming from the Python API
 /// Handles messages and tracks creation/destruction of prefab objects
 /// </summary>
    public class Client_SocketIO : MonoBehaviour
    {
        [SerializeField] private bool localhost;

        // NEURONS
        [SerializeField] private GameObject neuronPrefab;
        [SerializeField] private List<Mesh> neuronMeshList;
        [SerializeField] private List<string> neuronMeshNames;
        [SerializeField] private Transform neuronParent;
        private Dictionary<string, GameObject> neurons;


        #region Managers
        [SerializeField] private LineRendererManager _lineRendererManager;
        [SerializeField] private PrimitiveMeshManager _primitiveMeshManager;
        [SerializeField] private ProbeManager _probeManager;
        [SerializeField] private AtlasManager _areaManager;
        [SerializeField] private TextManager _textManager;
        [SerializeField] private VolumeManager _volumeManager;
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private FOVManager _fovManager;//TODO
        #endregion

        // NODES

        private string ID;

        private static SocketManager manager;

        /// <summary>
        /// Unity internal startup function, initializes internal variables and allocates memory
        /// </summary>
        private void Awake()
        {
            neurons = new Dictionary<string, GameObject>();

            if (_lineRendererManager == null || _primitiveMeshManager == null || _probeManager == null || _areaManager == null ||
                _textManager == null || _volumeManager == null || _cameraManager == null)
                throw new Exception("All managers must be linked in the editor!");
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
#if UNITY_EDITOR
        manager.Socket.On("reconnect", () => { Debug.Log("(Client) client reconnected -- could be sign of a timeout issue"); });
#endif

            // CCF Areas
            manager.Socket.On<string>("LoadAtlas", _areaManager.LoadAtlas);
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
            manager.Socket.On<List<object>>("SetVolumeVisibility", _volumeManager.SetVisibility);
            manager.Socket.On<List<object>>("SetVolumeDataMeta", _volumeManager.SetMetadata);
            manager.Socket.On<byte[]>("SetVolumeData", _volumeManager.SetData);
            manager.Socket.On<string>("CreateVolume", _volumeManager.Create);
            manager.Socket.On<string>("DeleteVolume", _volumeManager.Delete);
            manager.Socket.On<List<string>>("SetVolumeColormap", _volumeManager.SetVolumeColormap);

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
            manager.Socket.On<Dictionary<string, List<float>>>("SetCameraTarget", _cameraManager.SetCameraTarget);
            manager.Socket.On<Dictionary<string, List<float>>>("SetCameraPosition", _cameraManager.SetCameraPosition);
            manager.Socket.On<Dictionary<string, List<float>>>("SetCameraRotation", _cameraManager.SetCameraRotation);
            manager.Socket.On<Dictionary<string, string>>("SetCameraTargetArea", _cameraManager.SetCameraTargetArea);
            manager.Socket.On<Dictionary<string, float>>("SetCameraZoom", _cameraManager.SetCameraZoom);
            manager.Socket.On<Dictionary<string, List<float>>>("SetCameraPan", _cameraManager.SetCameraPan);
            manager.Socket.On<Dictionary<string, string>>("SetCameraMode", _cameraManager.SetCameraMode);
            manager.Socket.On<string>("SetCameraControl", _cameraManager.SetCameraControl);
            manager.Socket.On<string>("RequestCameraImg", _cameraManager.Screenshot);
            manager.Socket.On<Dictionary<string, float>>("SetCameraYAngle", _cameraManager.SetCameraYAngle);
            manager.Socket.On<List<string>>("CreateCamera", _cameraManager.CreateCamera);
            manager.Socket.On<List<string>>("DeleteCamera", _cameraManager.DeleteCamera);

            // Lights
            manager.Socket.On("ResetLightLink", _cameraManager.SetLightCameraLink);
            manager.Socket.On<string>("SetLightLink", _cameraManager.SetLightCameraLink);
            manager.Socket.On<List<float>>("SetLightRotation", _cameraManager.SetLightRotation);

            // Text
            manager.Socket.On<List<string>>("CreateText", _textManager.Create);
            manager.Socket.On<List<string>>("DeleteText", _textManager.Delete);
            manager.Socket.On<Dictionary<string, string>>("SetTextText", _textManager.SetText);
            manager.Socket.On<Dictionary<string, string>>("SetTextColors", _textManager.SetColor);
            manager.Socket.On<Dictionary<string, int>>("SetTextSizes", _textManager.SetSize);
            manager.Socket.On<Dictionary<string, List<float>>>("SetTextPositions", _textManager.SetPosition);

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
            manager.Socket.On<Dictionary<string, string>>("SetMaterial", _primitiveMeshManager.SetMaterial);

            // Calcium FOV Renderer
            manager.Socket.On<List<string>>("CreateFOV", _fovManager.Create);
            manager.Socket.On<List<string>>("DeleteFOV", _fovManager.Delete);
            manager.Socket.On<Dictionary<string, List<List<float>>>>("SetFOVPos", _fovManager.SetPosition);
            //manager.Socket.On<Dictionary<string, float>>("SetFOVOffset", _fovManager.SetOffset);
            //manager.Socket.On<List<object>>("SetFOVTextureDataMetaInit", _fovManager.SetTextureDataMetaInit);
            //manager.Socket.On<List<object>>("SetFOVTextureDataMeta", _fovManager.SetTextureDataMeta);
            //manager.Socket.On<byte[]>("SetFOVTextureData", _fovManager.SetTextureData);
            manager.Socket.On<Dictionary<string, bool>>("SetFOVVisibility", _fovManager.SetVisibility);


            // Misc
            manager.Socket.On<string>("Clear", Clear);

            // If we are building to WebGL or to Standalone, switch how you acquire the user's ID

#if UNITY_WEBGL && !UNITY_EDITOR
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
#endif
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
                    _volumeManager.Clear();
                    _textManager.Clear();
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
                    _volumeManager.Clear();
                    break;
                case "texts":
                    _textManager.Clear();
                    break;
                case "primitives":
                    _primitiveMeshManager.Clear();
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

        #endregion


        // NEURONS


        private void UpdateNeuronMaterial(Dictionary<string, string> obj)
        {
            throw new NotImplementedException();
        }

        private void UpdateNeuronScale(Dictionary<string, float> data)
        {
            Debug.Log("Updating neuron scale");
            foreach (KeyValuePair<string, float> kvp in data)
                neurons[kvp.Key].transform.localScale = Vector3.one * kvp.Value;
        }

        private void UpdateNeuronShape(Dictionary<string, string> data)
        {
            Debug.Log("Updating neuron shapes");
            foreach (KeyValuePair<string, string> kvp in data)
            {
                if (neuronMeshNames.Contains(kvp.Value))
                    neurons[kvp.Key].GetComponent<MeshFilter>().mesh = neuronMeshList[neuronMeshNames.IndexOf(kvp.Value)];
                else
                    Debug.Log("Mesh type: " + kvp.Value + " does not exist");
            }
        }

        private void UpdateNeuronColor(Dictionary<string, string> data)
        {
            Debug.Log("Updating neuron color");
            foreach (KeyValuePair<string, string> kvp in data)
            {

                Color newColor;
                if (neurons.ContainsKey(kvp.Key) && ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
                {
                    neurons[kvp.Key].GetComponent<Renderer>().material.color = newColor;
                }
                else
                    Debug.Log("Failed to set neuron color to: " + kvp.Value);
            }
        }

        // Takes coordinates in ML AP DV in um units
        private void UpdateNeuronPos(Dictionary<string, List<float>> data)
        {
            foreach (KeyValuePair<string, List<float>> kvp in data)
            {
                neurons[kvp.Key].transform.localPosition = new Vector3(-kvp.Value[0] / 1000f, -kvp.Value[2] / 1000f, kvp.Value[1] / 1000f);
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
                Destroy(neurons[id]);
                neurons.Remove(id);
            }
        }

        ////
        //// SOCKET FUNCTIONS
        ////

        public static void Emit(string header, object data)
        {
            manager.Socket.Emit(header, data);
        }

        public void UpdateID(string newID)
        {
            ID = newID;
            manager.Socket.Emit("ID", new List<string>() { ID, "receive" });
        }

        private void OnDestroy()
        {
            manager.Close();
        }

        private void Connected()
        {
            UpdateID(ID);
        }

        public static void Log(string msg)
        {
            manager.Socket.Emit("log", msg);
        }

        public static void LogWarning(string msg)
        {
            manager.Socket.Emit("log-warning", msg);
        }

        public static void LogError(string msg)
        {
            manager.Socket.Emit("log-error", msg);
        }
    }

}
