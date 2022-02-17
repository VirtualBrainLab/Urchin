using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class LoadIBLData_IBL_GLM : MonoBehaviour
{
    public NeuronEntityManager nemanager;

    private List<float3> iblPos;
    private List<IBLGLMComponent> iblData;

    // Start is called before the first frame update
    void Start()
    {
        ParseIBLData();

        nemanager.AddNeurons(iblPos, iblData);
    }

    private void ParseIBLData()
    {
        iblPos = new List<float3>();
        iblData = new List<IBLGLMComponent>();

        float scale = 1000;
        List<Dictionary<string, object>> mlapdv = CSVReader.Read("Datasets/ibl/ccf_mlapdv");

        for (int i = 0; i < mlapdv.Count - 1; i++)
        {
            float ml = (float)mlapdv[i]["ml"] / scale;
            float dv = (float)mlapdv[i]["dv"] / scale;
            float ap = (float)mlapdv[i]["ap"] / scale;
            iblPos.Add(new float3(ml, ap, dv));
        }

        List<Dictionary<string, object>> data = CSVReader.Read("Datasets/ibl/uuid_data");

        for (int i = 0; i < data.Count; i++)
        {
            iblData.Add(new IBLGLMComponent
            {
                rf_x = (float)data[i]["x"] * 7.33f,
                rf_y = (float)data[i]["y"] * 6.26f,
                rf_sig = (float)data[i]["sigma"] * 7f,
                stimOnL = (float)data[i]["stimOnL"],
                stimOnR = (float)data[i]["stimOnR"],
                correct = (float)data[i]["correct"],
                incorrect = (float)data[i]["incorrect"],
                wheel = (float)data[i]["wheel"]
            });
        }
    }
}
