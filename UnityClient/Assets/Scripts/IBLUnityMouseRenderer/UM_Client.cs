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

    [SerializeField] private GameObject probePrefab;

    // NEURONS
    private Dictionary<int, Entity> neurons;

    private string ID;

    SocketManager manager;

    // Start is called before the first frame update
    void Start()
    {

        Debug.Log("Attempting to connect");
        manager = new SocketManager(new Uri("https://um-commserver.herokuapp.com/"));
        //manager = new SocketManager(new Uri("http://localhost:5000"));

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

    private void UpdateNeuronShape(Dictionary<string, string> obj)
    {
        main.Log("Not implemented");
    }

    private void UpdateNeuronPos(Dictionary<string, List<float>> obj)
    {
        main.Log("Not implemented");
    }

    private void CreateNeurons(List<string> obj)
    {
        main.Log("Not implemented");
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

    private void UpdateColors(Dictionary<string, string> data)
    {
        foreach (KeyValuePair<string, string> kvp in data)
        {
            CCFTreeNode node = modelControl.tree.findNode(GetID(kvp.Key));

            Color newColor;
            if (node != null && ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
            {
                if (!node.IsLoaded())
                    node.loadNodeModel(true);
                node.SetColor(newColor);
            }
            else
                main.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

    private int GetID(string idOrAcronym)
    {
        if (modelControl.IsAcronym(idOrAcronym))
            return modelControl.Acronym2ID(idOrAcronym);
        else
            return int.Parse(idOrAcronym);
    }

    private void UpdateVisibility(Dictionary<string, bool> data)
    {
        foreach (KeyValuePair<string, bool> kvp in data)
        {
            CCFTreeNode node = modelControl.tree.findNode(GetID(kvp.Key));

            if (node != null)
            {
                if (!node.IsLoaded())
                    node.loadNodeModel(true);
                node.SetNodeModelVisibility(kvp.Value);
            }
            else
                main.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

    private void UpdateIntensity(Dictionary<string, float> data)
    {

        foreach (KeyValuePair<string, float> kvp in data)
        {
            CCFTreeNode node = modelControl.tree.findNode(GetID(kvp.Key));

            if (node != null)
            {
                if (!node.IsLoaded())
                    node.loadNodeModel(true);
                node.SetColor(main.Cool(kvp.Value));
            }
            else
                main.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

}
