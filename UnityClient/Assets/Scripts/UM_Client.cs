using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO3;
using System;
using System.Linq;
using TMPro;
using Unity.Entities;

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
    private Dictionary<string, GameObject> probes;

    private string ID;

    SocketManager manager;

    private void Awake()
    {
        neurons = new Dictionary<string, GameObject>();
        probes = new Dictionary<string, GameObject>();
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
        manager.Socket.On<Dictionary<string, float>>("SetNeuronSize", UpdateIntensity);
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

    private void UpdateProbeStyle(Dictionary<string, string> obj)
    {
        main.Log("Not implemented");
    }

    private void UpdateProbeAngles(Dictionary<string, List<float>> obj)
    {
        main.Log("Not implemented");
    }

    private void UpdateProbePos(Dictionary<string, List<float>> obj)
    {
        main.Log("Not implemented");
    }

    private void UpdateProbeColors(Dictionary<string, string> obj)
    {
        main.Log("Not implemented");
    }

    private void CreateProbes(List<string> obj)
    {
        main.Log("Not implemented");
    }

    private void SetVolumeAnnotationColor(Dictionary<string, string> obj)
    {
        main.Log("Not implemented");
    }

    private void SetVolumeSlice(List<float> obj)
    {
        main.Log("Not implemented");
    }

    private void UpdateNeuronColor(Dictionary<string, string> data)
    {
        foreach (KeyValuePair<string, string> kvp in data)
        {
            if (neuronMeshNames.Contains(kvp.Value))
                neurons[kvp.Key].GetComponent<MeshFilter>().mesh = neuronMeshList[neuronMeshNames.IndexOf(kvp.Value)];
            else
                main.Log("Mesh type: " + kvp.Value + " does not exist");
        }
    }

    private void UpdateNeuronShape(Dictionary<string, string> data)
    {
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

    private void UpdateVolumeMaterial(Dictionary<string, string> data)
    {
        foreach (KeyValuePair<string, string> kvp in data)
        {
            modelControl.ChangeMaterial(int.Parse(kvp.Key), kvp.Value);
        }
    }

    private void UpdateVolumeStyle(Dictionary<string, string> obj)
    {
        main.Log("Not implemented");
    }

    private void UpdateVolumeColormap(string obj)
    {
        main.Log("Not implemented");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            idPanel.SetActive(!idPanel.activeSelf);
        }
    }

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

    private async void UpdateColors(Dictionary<string, string> data)
    {
        foreach (KeyValuePair<string, string> kvp in data)
        {
            CCFTreeNode node = modelControl.tree.findNode(GetID(kvp.Key));

            Color newColor;
            if (node != null && ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
            {
                if (!node.IsLoaded())
                    await node.loadNodeModel(true);
                node.SetColor(newColor);
            }
            else
                main.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

    private int GetID(string idOrAcronym)
    {
        Debug.Log(idOrAcronym);
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

    private async void UpdateVisibility(Dictionary<string, bool> data)
    {
        foreach (KeyValuePair<string, bool> kvp in data)
        {
            CCFTreeNode node = modelControl.tree.findNode(GetID(kvp.Key));

            if (node != null)
            {
                if (!node.IsLoaded())
                    await node.loadNodeModel(true);
                node.SetNodeModelVisibility(kvp.Value);
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

            if (node != null)
            {
                if (!node.IsLoaded())
                    await node.loadNodeModel(true);
                node.SetColor(main.Cool(kvp.Value));
            }
            else
                main.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

}
