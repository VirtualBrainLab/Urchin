using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LoadData_IBL_ProbeByLab : MonoBehaviour
{
    [SerializeField] Utils util;
    [SerializeField] NeuronEntityManager nemanager;
    [SerializeField] ElectrodeManager emanager;
    [SerializeField] private List<GameObject> labObjects;
    [SerializeField] private PA_Launch palaunch;

    private Color[] colors = { new Color(0.12156862745098039f, 0.4666666666666667f, 0.7058823529411765f, 1.0f),
        new Color(1.0f, 0.4980392156862745f, 0.054901960784313725f, 1.0f),
        new Color(0.17254901960784313f, 0.6274509803921569f, 0.17254901960784313f, 1.0f),
        new Color(0.8392156862745098f, 0.15294117647058825f, 0.1568627450980392f, 1.0f),
        new Color(0.5803921568627451f, 0.403921568627451f, 0.7411764705882353f, 1.0f),
        new Color(0.5490196078431373f, 0.33725490196078434f, 0.29411764705882354f, 1.0f),
        new Color(0.8901960784313725f, 0.4666666666666667f, 0.7607843137254902f, 1.0f),
        new Color(0.25f, 0.4980392156862745f, 0.75f, 1.0f),
        new Color(0.7372549019607844f, 0.7411764705882353f, 0.13333333333333333f, 1.0f),
        new Color(0.09019607843137255f, 0.7450980392156863f, 0.8117647058823529f, 1.0f)
    };

    private void Awake()
    {

        emanager.LoadAllDatasets();
    }

    // Start is called before the first frame update
    void Start()
    {

        // load the UUID and MLAPDV data
        Dictionary<string, float3> mlapdvData = util.LoadIBLmlapdv();
        List<float3> mlapdv = new List<float3>();

        // load the lab information
        List<Dictionary<string, object>> data_metadata = CSVReader.Read("Datasets/ibl/metadata");
        Dictionary<string, int> lab2int = new Dictionary<string, int>();
        int labCount = 0;
        Dictionary<string, int> metadata = new Dictionary<string, int>();

        // data for neuron entities
        List<Color> neuronColors = new List<Color>();
        List<int> neuronLabs = new List<int>();

        for (int i = 0; i < data_metadata.Count; i++)
        {
            string uuid = (string)data_metadata[i]["uuid"];
            string lab = (string)data_metadata[i]["lab"];

            if (!lab2int.ContainsKey(lab))
            {
                lab2int.Add(lab, labCount);
                labCount++;
            }

            metadata.Add(uuid, lab2int[lab]);

            if (mlapdvData.ContainsKey(uuid))
            {
                float3 cmlapdv = mlapdvData[uuid];
                // check if this coordinate actually exists in the annotation dataset
                Vector3 apdvlr = new Vector3(cmlapdv.y, cmlapdv.z, cmlapdv.x) * 40f;

                if (emanager.GetAnnotation(Mathf.RoundToInt(apdvlr.x), Mathf.RoundToInt(apdvlr.y), Mathf.RoundToInt(apdvlr.z)) > 0)
                {
                    mlapdv.Add(cmlapdv);
                    neuronColors.Add(colors[lab2int[lab]]);
                    neuronLabs.Add(lab2int[lab]);
                }
            }
        }

        nemanager.AddNeurons(mlapdv, neuronColors, neuronLabs);

        // Go through lab objects and assign colors
        for (int i=0; i<labObjects.Count;i++)
        {
            labObjects[i].GetComponent<Renderer>().material.SetColor("_Color", colors[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        int currentLab = palaunch.GetCurrentLab();

        for (int i = 0; i < labObjects.Count; i++)
            if (i == currentLab)
                labObjects[i].transform.localScale = new Vector3(3f, 3f, 3f);
            else
                labObjects[i].transform.localScale = new Vector3(1f, 1f, 1f);
    }

}
