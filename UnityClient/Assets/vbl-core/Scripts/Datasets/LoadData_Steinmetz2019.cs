using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class LoadData_Steinmetz2019 : MonoBehaviour
{
    public NeuronEntityManager nemanager;

    private List<float3> positions;

    // Start is called before the first frame update
    void Start()
    {
        ParseSteinmetzData();

        nemanager.AddNeurons(positions);
    }

    private void ParseSteinmetzData()
    {
        // Load and create gameObjects for all of the data in Nick's dataset
        List<Dictionary<string, object>> data = CSVReader.Read("Datasets/nick_neuron_positions");
        positions = new List<float3>();

        for (var i = 0; i < data.Count; i++)
        {
            // get the values in the CSV file and add to the tree
            float ap = (int)data[i]["ap"] / 100.0f;
            float dv = (int)data[i]["dv"] / 100.0f;
            float ml = (int)data[i]["lr"] / 100.0f;

            positions.Add(new float3(ml, ap, dv));
        }
    }

    private void ParseIBLData()
    {
        // Load and create gameObjects for all of the data in Nick's dataset
        List<Dictionary<string, object>> data = CSVReader.Read("Datasets/IBL_neuron_positions");
        positions = new List<float3>();

        for (var i = 0; i < data.Count; i++)
        {
            // get the values in the CSV file and add to the tree
            float ap = (int)data[i]["ap"] / 100.0f;
            float dv = (int)data[i]["dv"] / 100.0f;
            float ml = (int)data[i]["lr"] / 100.0f;

            positions.Add(new float3(ml, ap, dv));
        }
    }
}
