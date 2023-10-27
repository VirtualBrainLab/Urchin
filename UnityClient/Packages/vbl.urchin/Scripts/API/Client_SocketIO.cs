using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO3;
using System;
using UnityEngine.Events;
using System.Collections.Specialized;

namespace Urchin.API
{/// <summary>
 /// Entry point for all client-side messages coming from the Python API
 /// Handles messages and tracks creation/destruction of prefab objects
 /// </summary>
    public class Client_SocketIO : MonoBehaviour
    {
        #region Static
        public static readonly int SOCKET_IO_MAX_CHUNK_BYTES = 100000;
        #endregion

        #region Events
        public UnityEvent<string> IDChangedEvent;
        #endregion

        #region variables
        private const string ID_SAVE_KEY = "id";
        private string _ID;
        public string ID
        {
            get { return _ID; }
            set
            {
                _ID = value;
                PlayerPrefs.SetString(ID_SAVE_KEY, ID);
                manager.Socket.Emit("ID", new List<string>() { ID, "receive" });
                IDChangedEvent.Invoke(ID);
            }
        }

        [SerializeField] private bool localhost;

        private static SocketManager manager;
        #endregion

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

            // Call the startup functions, these bind all the Socket.on events and setup the static Actions, which
            // other scripts can then listen to
            Start_Atlas();
            Start_Volume();
            Start_Particles();
            Start_Probes();
            Start_Camera();
            Start_Light();
            Start_Text();
            Start_LineRenderer();
            Start_PrimitiveMeshRenderer();
            Start_FOV();

            // Misc
            manager.Socket.On<string>("Clear", Clear);
        }

        #region Socket setup by action group
        public static Action<string> LoadAtlas;
        public static Action<Dictionary<string, bool>> SetAreaVisibility;
        public static Action<Dictionary<string, string>> SetAreaColors;
        public static Action<Dictionary<string, float>> SetAreaIntensity;
        public static Action<string> SetAreaColormap;
        public static Action<Dictionary<string, string>> SetAreaMaterial;
        public static Action<Dictionary<string, float>> SetAreaAlpha;
        public static Action<Dictionary<string, List<float>>> SetAreaData;
        public static Action<int> SetAreaIndex;
        public static Action<string> LoadDefaultAreas;

        private void Start_Atlas()
        {
            // CCF Areas
            manager.Socket.On<string>("LoadAtlas", x => LoadAtlas.Invoke(x));
            manager.Socket.On<Dictionary<string, bool>>("SetAreaVisibility", x => SetAreaVisibility.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetAreaColors", x => SetAreaColors.Invoke(x));
            manager.Socket.On<Dictionary<string, float>>("SetAreaIntensity", x => SetAreaIntensity.Invoke(x));
            manager.Socket.On<string>("SetAreaColormap", x => SetAreaColormap.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetAreaMaterial", x => SetAreaMaterial.Invoke(x));
            manager.Socket.On<Dictionary<string, float>>("SetAreaAlpha", x => SetAreaAlpha.Invoke(x));
            manager.Socket.On<Dictionary<string, List<float>>>("SetAreaData", x => SetAreaData.Invoke(x));
            manager.Socket.On<int>("SetAreaIndex", x => SetAreaIndex.Invoke(x));
            manager.Socket.On<string>("LoadDefaultAreas", x => LoadDefaultAreas.Invoke(x));
        }

        public static Action<List<object>> SetVolumeVisibility;
        public static Action<List<object>> SetVolumeDataMeta;
        public static Action<byte[]> SetVolumeData;
        public static Action<string> CreateVolume;
        public static Action<string> DeleteVolume;
        public static Action<List<string>> SetVolumeColormap;

        private void Start_Volume()
        {
            manager.Socket.On<List<object>>("SetVolumeVisibility", x => SetVolumeVisibility.Invoke(x));
            manager.Socket.On<List<object>>("SetVolumeDataMeta", x => SetVolumeDataMeta.Invoke(x));
            manager.Socket.On<byte[]>("SetVolumeData", x => SetVolumeData.Invoke(x));
            manager.Socket.On<string>("CreateVolume", x => CreateVolume.Invoke(x));
            manager.Socket.On<string>("DeleteVolume", x => DeleteVolume.Invoke(x));
            manager.Socket.On<List<string>>("SetVolumeColormap", x => SetVolumeColormap.Invoke(x));
        }

        public static Action<List<string>> CreateParticles;
        public static Action<List<string>> DeleteParticles;
        public static Action<Dictionary<string, float[]>> SetParticlePosition;
        public static Action<Dictionary<string, float>> SetParticleSize;
        public static Action<Dictionary<string, string>> SetParticleShape;
        public static Action<Dictionary<string, string>> SetParticleColor;
        public static Action<Dictionary<string, string>> SetParticleMaterial;

        private void Start_Particles()
        {
            manager.Socket.On<List<string>>("CreateNeurons", x => CreateParticles.Invoke(x));
            manager.Socket.On<List<string>>("DeleteNeurons", x => DeleteParticles.Invoke(x));
            manager.Socket.On<Dictionary<string, float[]>>("SetNeuronPos", x => SetParticlePosition.Invoke(x));
            manager.Socket.On<Dictionary<string, float>>("SetNeuronSize", x => SetParticleSize.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetNeuronShape", x => SetParticleShape.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetNeuronColor", x => SetParticleColor.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetNeuronMaterial", x => SetParticleMaterial.Invoke(x));
        }

        public static Action<List<string>> CreateProbes;
        public static Action<List<string>> DeleteProbes;
        public static Action<Dictionary<string, string>> SetProbeColors;
        public static Action<Dictionary<string, List<float>>> SetProbePos;
        public static Action<Dictionary<string, List<float>>> SetProbeAngles;
        public static Action<Dictionary<string, string>> SetProbeStyle;
        public static Action<Dictionary<string, List<float>>> SetProbeSize;

        private void Start_Probes()
        {
            manager.Socket.On<List<string>>("CreateProbes", x => CreateProbes.Invoke(x));
            manager.Socket.On<List<string>>("DeleteProbes", x => DeleteProbes.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetProbeColors", x => SetProbeColors.Invoke(x));
            manager.Socket.On<Dictionary<string, List<float>>>("SetProbePos", x => SetProbePos.Invoke(x));
            manager.Socket.On<Dictionary<string, List<float>>>("SetProbeAngles", x => SetProbeAngles.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetProbeStyle", x => SetProbeStyle.Invoke(x));
            manager.Socket.On<Dictionary<string, List<float>>>("SetProbeSize", x => SetProbeSize.Invoke(x));
        }

        public static Action<Dictionary<string, List<float>>> SetCameraTarget;
        public static Action<Dictionary<string, List<float>>> SetCameraRotation;
        public static Action<Dictionary<string, string>> SetCameraTargetArea;
        public static Action<Dictionary<string, float>> SetCameraZoom;
        public static Action<Dictionary<string, List<float>>> SetCameraPan;
        public static Action<Dictionary<string, string>> SetCameraMode;
        public static Action<string> SetCameraControl;
        public static Action<string> RequestScreenshot;
        public static Action<Dictionary<string, float>> SetCameraYAngle;
        public static Action<List<string>> CreateCamera;
        public static Action<List<string>> DeleteCamera;

        private void Start_Camera()
        {
            manager.Socket.On<Dictionary<string, List<float>>>("SetCameraTarget", x => SetCameraTarget.Invoke(x));
            manager.Socket.On<Dictionary<string, List<float>>>("SetCameraRotation", x => SetCameraRotation.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetCameraTargetArea", x => SetCameraTargetArea.Invoke(x));
            manager.Socket.On<Dictionary<string, float>>("SetCameraZoom", x => SetCameraZoom.Invoke(x));
            manager.Socket.On<Dictionary<string, List<float>>>("SetCameraPan", x => SetCameraPan.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetCameraMode", x => SetCameraMode.Invoke(x));
            manager.Socket.On<string>("SetCameraControl", x => SetCameraControl.Invoke(x));
            manager.Socket.On<string>("RequestCameraImg", x => RequestScreenshot.Invoke(x));
            manager.Socket.On<Dictionary<string, float>>("SetCameraYAngle", x => SetCameraYAngle.Invoke(x));
            manager.Socket.On<List<string>>("CreateCamera", x => CreateCamera.Invoke(x));
            manager.Socket.On<List<string>>("DeleteCamera", x => DeleteCamera.Invoke(x));
        }

        public static Action ResetLightLink;
        public static Action<string> SetLightLink;
        public static Action<List<float>> SetLightRotation;

        private void Start_Light()
        {
            manager.Socket.On("ResetLightLink", () => ResetLightLink.Invoke());
            manager.Socket.On<string>("SetLightLink", x => SetLightLink.Invoke(x));
            manager.Socket.On<List<float>>("SetLightRotation", x => SetLightRotation.Invoke(x));
        }

        public static Action<List<string>> CreateText;
        public static Action<List<string>> DeleteText;
        public static Action<Dictionary<string, string>> SetTextText;
        public static Action<Dictionary<string, string>> SetTextColors;
        public static Action<Dictionary<string, int>> SetTextSizes;
        public static Action<Dictionary<string, List<float>>> SetTextPositions;

        private void Start_Text()
        {
            manager.Socket.On<List<string>>("CreateText", x => CreateText.Invoke(x));
            manager.Socket.On<List<string>>("DeleteText", x => DeleteText.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetTextText", x => SetTextText.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetTextColors", x => SetTextColors.Invoke(x));
            manager.Socket.On<Dictionary<string, int>>("SetTextSizes", x => SetTextSizes.Invoke(x));
            manager.Socket.On<Dictionary<string, List<float>>>("SetTextPositions", x => SetTextPositions.Invoke(x));
        }

        public static Action<List<string>> CreateLine;
        public static Action<Dictionary<string, List<List<float>>>> SetLinePosition;
        public static Action<List<string>> DeleteLine;
        public static Action<Dictionary<string, string>> SetLineColor;

        private void Start_LineRenderer()
        {
            manager.Socket.On<List<string>>("CreateLine", x => CreateLine.Invoke(x));
            manager.Socket.On<Dictionary<string, List<List<float>>>>("SetLinePosition", x => SetLinePosition.Invoke(x));
            manager.Socket.On<List<string>>("DeleteLine", x => DeleteLine.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetLineColor", x => SetLineColor.Invoke(x));
        }

        public static Action<List<string>> CreateMesh;
        public static Action<List<string>> DeleteMesh;
        public static Action<Dictionary<string, List<float>>> SetPosition;
        public static Action<Dictionary<string, List<float>>> SetScale;
        public static Action<Dictionary<string, string>> SetColor;
        public static Action<Dictionary<string, string>> SetMaterial;

        private void Start_PrimitiveMeshRenderer()
        {
            manager.Socket.On<List<string>>("CreateMesh", x => CreateMesh.Invoke(x));
            manager.Socket.On<List<string>>("DeleteMesh", x => DeleteMesh.Invoke(x));
            manager.Socket.On<Dictionary<string, List<float>>>("SetPosition", x => SetPosition.Invoke(x));
            manager.Socket.On<Dictionary<string, List<float>>>("SetScale", x => SetScale.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetColor", x => SetColor.Invoke(x));
            manager.Socket.On<Dictionary<string, string>>("SetMaterial", x => SetMaterial.Invoke(x));
        }

        public static Action<List<string>> CreateFOV;
        public static Action<List<string>> DeleteFOV;
        public static Action<Dictionary<string, List<List<float>>>> SetFOVPos;
        public static Action<Dictionary<string, float>> SetFOVOffset;
        public static Action<List<object>> SetFOVTextureDataMetaInit;
        public static Action<List<object>> SetFOVTextureDataMeta;
        public static Action<byte[]> SetFOVTextureData;
        public static Action<Dictionary<string, bool>> SetFOVVisibility;

        private void Start_FOV()
        {
            manager.Socket.On<List<string>>("CreateFOV", x => CreateFOV.Invoke(x));
            manager.Socket.On<List<string>>("DeleteFOV", x => DeleteFOV.Invoke(x));
            manager.Socket.On<Dictionary<string, List<List<float>>>>("SetFOVPos", x => SetFOVPos.Invoke(x));
            manager.Socket.On<Dictionary<string, float>>("SetFOVOffset", x => SetFOVOffset.Invoke(x));
            manager.Socket.On<List<object>>("SetFOVTextureDataMetaInit", x => SetFOVTextureDataMetaInit.Invoke(x));
            manager.Socket.On<List<object>>("SetFOVTextureDataMeta", x => SetFOVTextureDataMeta.Invoke(x));
            manager.Socket.On<byte[]>("SetFOVTextureData", x => SetFOVTextureData.Invoke(x));
            manager.Socket.On<Dictionary<string, bool>>("SetFOVVisibility", x => SetFOVVisibility.Invoke(x));
        }


        #endregion

        #region Clear

        public static Action ClearProbes;
        public static Action ClearAreas;
        public static Action ClearVolumes;
        public static Action ClearText;
        public static Action ClearParticles;
        public static Action ClearMeshes;

        private void Clear(string val)
        {
            switch (val)
            {
                case "all":
                    ClearProbes.Invoke();
                    ClearAreas.Invoke();
                    ClearVolumes.Invoke();
                    ClearText.Invoke();
                    ClearMeshes.Invoke();
                    ClearParticles.Invoke();
                    break;
                case "probes":
                    ClearProbes.Invoke();
                    break;
                case "areas":
                    ClearAreas.Invoke();
                    break;
                case "volumes":
                    ClearVolumes.Invoke();
                    break;
                case "texts":
                    ClearText.Invoke();
                    break;
                case "primitives":
                    ClearMeshes.Invoke();
                    break;
                case "particles":
                    ClearParticles.Invoke();
                    break;
            }
        }
        #endregion

        ////
        //// SOCKET FUNCTIONS
        ////

        public static void Emit(string header, string data)
        {
            manager.Socket.Emit(header, data);
        }

        public void UpdateID(string newID)
        {
            ID = newID;
            Debug.Log($"ID updated to {ID}");
        }

        private void OnDestroy()
        {
            manager.Close();
        }

        private void Connected()
        {

            // If we are building to WebGL or to Standalone, switch how you acquire the user's ID
            string queryID;
            bool webGLID = Utils.Utils.ParseQueryForID(out queryID);

            if (webGLID)
            {
                UpdateID(queryID);
                Debug.Log("Found ID in Query string, setting to: " + ID);
            }
            else if (PlayerPrefs.HasKey(ID_SAVE_KEY))
            {
                UpdateID(PlayerPrefs.GetString(ID_SAVE_KEY));
                Debug.Log("Found ID in PlayerPrefs, setting to: " + ID);
            }
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
