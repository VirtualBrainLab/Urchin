using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO3;
using System;
using System.Linq;
using TMPro;
using Unity.Entities;
using System.Threading.Tasks;

public class UM_Client : MonoBehaviour
{
    [SerializeField] CCFModelControl modelControl;
    [SerializeField] UM_Launch main;

    [SerializeField] private GameObject idPanel;
    [SerializeField] private TextMeshProUGUI idInput;


    [SerializeField] private bool localhost;

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

    // POSITIONING
    private Vector3 center = new Vector3(5.7f, 4f, -6.6f);

    private string ID;

    SocketManager manager;

    private void Awake()
    {
        neurons = new Dictionary<string, GameObject>();
        probes = new Dictionary<string, GameObject>();
        nodeTasks = new Dictionary<int, Task>();
        probeCoordinates = new Dictionary<string, Vector3[]>();
    }
    // Start is called before the first frame update
    void Start()
    {

        string url = localhost ? "http://localhost:5000" : "https://um-commserver.herokuapp.com/";
        Debug.Log("Attempting to connect: " + url);
        manager = localhost ? new SocketManager(new Uri(url)) : new SocketManager(new Uri(url));

        manager.Socket.On("connect", Connected);
        manager.Socket.On<Dictionary<string, bool>>("SetVolumeVisibility", UpdateVisibility);
        manager.Socket.On<Dictionary<string, string>>("SetVolumeColors", UpdateColors);
        manager.Socket.On<Dictionary<string, float>>("SetVolumeIntensity", UpdateIntensity);
        manager.Socket.On<string>("SetVolumeColormap", UpdateVolumeColormap);
        manager.Socket.On<Dictionary<string, string>>("SetVolumeStyle", UpdateVolumeStyle);
        manager.Socket.On<Dictionary<string, float>>("SetVolumeAlpha", UpdateIntensity);
        manager.Socket.On<Dictionary<string, string>>("SetVolumeShader", UpdateVolumeMaterial);
        manager.Socket.On<List<string>>("CreateNeurons", CreateNeurons);
        manager.Socket.On<Dictionary<string, List<float>>>("SetNeuronPos", UpdateNeuronPos);
        manager.Socket.On<Dictionary<string, float>>("SetNeuronSize", UpdateNeuronScale);
        manager.Socket.On<Dictionary<string, string>>("SetNeuronShape", UpdateNeuronShape);
        manager.Socket.On<Dictionary<string, string>>("SetNeuronColor", UpdateNeuronColor);
        manager.Socket.On<List<float>>("SliceVolume", SetVolumeSlice);
        manager.Socket.On<Dictionary<string, string>>("SetSliceColor", SetVolumeAnnotationColor);
        manager.Socket.On<List<string>>("CreateProbes", CreateProbes);
        manager.Socket.On<Dictionary<string, string>>("SetProbeColors", UpdateProbeColors);
        manager.Socket.On<Dictionary<string, List<float>>>("SetProbePos", UpdateProbePos);
        manager.Socket.On<Dictionary<string, List<float>>>("SetProbeAngles", UpdateProbeAngles);
        manager.Socket.On<Dictionary<string, string>>("SetProbeStyle", UpdateProbeStyle);

        ID = Environment.UserName;
        idInput.text = ID;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            idPanel.SetActive(!idPanel.activeSelf);
        }
    }

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
        main.Log("Not implemented");
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
            neurons[kvp.Key].transform.position = new Vector3(-kvp.Value[0]/1000f, -kvp.Value[2]/1000f, kvp.Value[1]/1000f);
        }
    }

    private void CreateNeurons(List<string> data)
    {
        foreach (string id in data)
        {
            neurons.Add(id, Instantiate(neuronPrefab, neuronParent));
        }
    }

    private async void UpdateVolumeMaterial(Dictionary<string, string> data)
    {
        foreach (KeyValuePair<string, string> kvp in data)
        {
            if (WaitingOnTask(GetID(kvp.Key)))
                await nodeTasks[GetID(kvp.Key)];

            modelControl.ChangeMaterial(GetID(kvp.Key), kvp.Value);
        }
    }

    private async void UpdateVolumeStyle(Dictionary<string, string> data)
    {
        foreach (KeyValuePair<string, string> kvp in data)
        {
            CCFTreeNode node = modelControl.tree.findNode(GetID(kvp.Key));

            if (WaitingOnTask(node.ID))
                await nodeTasks[node.ID];

            if (kvp.Value.ToLower().Equals("whole"))
                node.SetNodeModelVisibility(true, true);
            if (kvp.Value.ToLower().Equals("left"))
                node.SetNodeModelVisibility(true, false);
            if (kvp.Value.ToLower().Equals("right"))
                node.SetNodeModelVisibility(false,true);
        }
    }

    private void UpdateVolumeColormap(string obj)
    {
        main.Log("Not implemented");
    }

    private int GetID(string idOrAcronym)
    {
        if (idOrAcronym.ToLower().Equals("root") || idOrAcronym.ToLower().Equals("void"))
            return -1;
        if (modelControl.IsAcronym(idOrAcronym))
            return modelControl.Acronym2ID(idOrAcronym);
        else
        {
            int ret;
            if (int.TryParse(idOrAcronym, out ret))
                return ret;
        }
        return -1;
    }

    ////
    //// VOLUME FUNCTIONS
    /// Note that these are asynchronous calls, because we can't guarantee that the volumes are loaded until the call to setVisibility evaluates fully
    ////

    Dictionary<int, Task> nodeTasks;

    private async void UpdateColors(Dictionary<string, string> data)
    {
        foreach (KeyValuePair<string, string> kvp in data)
        {
            CCFTreeNode node = modelControl.tree.findNode(GetID(kvp.Key));

            if (WaitingOnTask(node.ID))
                await nodeTasks[node.ID];

            Color newColor;
            if (node != null && ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
                node.SetColor(newColor);
            else
                main.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

    private void UpdateVisibility(Dictionary<string, bool> data)
    {
        foreach (KeyValuePair<string, bool> kvp in data)
        {
            CCFTreeNode node = modelControl.tree.findNode(GetID(kvp.Key));

            if (node != null)
            {
                if (!node.IsLoaded())
                {
                    Task nodeTask = node.loadNodeModel(true, handle =>
                    {node.SetNodeModelVisibility(kvp.Value); Debug.Log(kvp.Key + " fully loaded"); });
                    nodeTasks.Add(node.ID, nodeTask);
                }
            }
            else
                main.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

    private async void UpdateIntensity(Dictionary<string, float> data)
    {

        foreach (KeyValuePair<string, float> kvp in data)
        {
            CCFTreeNode node = modelControl.tree.findNode(GetID(kvp.Key));

            if (WaitingOnTask(node.ID))
                await nodeTasks[node.ID];

            if (node != null)
                node.SetColor(main.Cool(kvp.Value));
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
        manager.Socket.Emit("ID", newID);
    }

    private void OnDestroy()
    {
        manager.Close();
    }

    private void Connected()
    {
        main.Log("connected! Login with ID: " + ID);
        manager.Socket.Emit("ID", ID);
    }

}
