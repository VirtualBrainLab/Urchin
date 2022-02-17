using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;
using UnityEngine.UI;
using System;

public class TestECSWebGL : MonoBehaviour
{
    public NeuronEntityManager nem;
    private EntityManager eManager;
    private List<float3> positions;
    private int numNeurons;
    private int[] dropdownCounts = new int[] { 1000, 5000, 10000, 50000, 100000, 500000, 1000000 };

    // Start is called before the first frame update
    void Start()
    {
        eManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        ParseSteinmetzData();
        updateNeuronNum(positions.Count);
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

    private void updateNeuronNum(int newNum)
    {
        Debug.Log(newNum);
        nem.RemoveAllNeurons();
        List<float3> newPositions = Shuffle(positions);
        if (newNum < positions.Count)
        {
            newPositions = newPositions.GetRange(0, newNum);
        }
        else
        {
            for (int i = 0; i < newNum - positions.Count; i++)
            {
                int curIdx = (int) UnityEngine.Random.Range(0, positions.Count);
                float3 curNoise = new float3(UnityEngine.Random.Range(-1, 1),
                                             UnityEngine.Random.Range(-1, 1),
                                             UnityEngine.Random.Range(-1, 1));
                newPositions.Add(positions[curIdx] + curNoise);
            }
        }

        List<Entity> neurons = nem.AddNeurons(newPositions);
        foreach (Entity neuron in neurons)
        {
            eManager.AddComponentData(neuron, new SpikingComponent { spiking = 1.0f });
        }
        numNeurons = neurons.Count;
    }

    public void UpdateDropDownNeurons(int dropdownIdx)
    {
        UpdateNumNeurons(dropdownCounts[dropdownIdx]);
    }

    public void UpdateNumNeurons(int newNum)
    {
        updateNeuronNum(newNum);
    }

    public void UpdateNumNeurons(float newNum)
    {
        updateNeuronNum((int) newNum);
    }

    // Copied from https://stackoverflow.com/questions/273313/randomize-a-listt
    public List<float3> Shuffle(List<float3> oldList)
    {
        List<float3> list = new List<float3>(oldList);
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = (int) UnityEngine.Random.Range(0, n + 1);
            float3 value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }
}
