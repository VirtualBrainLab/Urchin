using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class LoadData_IBL_EventAverage : MonoBehaviour
{
    public NeuronEntityManager nemanager;
    public Utils util;

    //float scale = 1000;

    int SCALED_LEN = 250;
    int conditions = 4;
    int[] side = { -1, -1, 1,  1};
    int[] corr = { 1, -1, 1, -1 };

    private float[] spikeRateMap;

    // Start is called before the first frame update
    void Start()
    {
        //ParseIBLData_EventAverage();

        Dictionary<string, IBLEventAverageComponent> eventAverageData = new Dictionary<string, IBLEventAverageComponent>();

        // load the UUID information for the event average data
        List<Dictionary<string, object>> data = CSVReader.Read("Datasets/ibl/uuid_avgs_info");
        // ibl/large_files/baseline-normalized_1d_clu_avgs
        // ibl/large_files/max-normalized_1d_clu_avgs
        float[] spikeRates = util.LoadBinaryFloatHelper("large_files/baseline-normalized_1d_clu_avgs");

        for (var ui = 0; ui < data.Count; ui++)
        {
            string uuid = (string)data[ui]["uuid"];
            FixedListFloat4096 spikeRate = new FixedListFloat4096();

            for (int i = 0; i < (SCALED_LEN*conditions); i++)
            {
                spikeRate.AddNoResize(spikeRates[(ui * SCALED_LEN) + i]);
            }

            IBLEventAverageComponent eventAverageComponent = new IBLEventAverageComponent();
            eventAverageComponent.spikeRate = spikeRate;
            eventAverageData.Add(uuid, eventAverageComponent);
        }

        // load the UUID and MLAPDV data
        Dictionary<string, float3> mlapdvData = util.LoadIBLmlapdv();

        //spikeRateMap = util.LoadBinaryFloatHelper("ibl/1d_clu_avgs_map");
        //byte[] spikeRates = util.LoadBinaryByteHelper("ibl/1d_clu_avgs_uint8");

        // Figure out which neurons we have both a mlapdv data and an event average dataset
        List<float3> iblPos = new List<float3>();
        List<IBLEventAverageComponent> eventAverageComponents = new List<IBLEventAverageComponent>();

        foreach (string uuid in eventAverageData.Keys)
        {
            if (mlapdvData.ContainsKey(uuid))
            {
                //Debug.Log("Found: " + uuid);
                iblPos.Add(mlapdvData[uuid]);
                eventAverageComponents.Add(eventAverageData[uuid]);
            }
        }

        Debug.Log("Num neurons: " + eventAverageComponents.Count);
        nemanager.AddNeurons(iblPos, eventAverageComponents);
    }
}
