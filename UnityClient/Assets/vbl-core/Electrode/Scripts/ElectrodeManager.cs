using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using MLAPI;
using MLAPI.Messaging;
using Unity.Entities;
using Unity.Mathematics;

public class ElectrodeManager : NetworkBehaviour
{
    [SerializeField] private Utils util;
    [SerializeField] private NeuronEntityManager nemanager;
    [SerializeField] private CCFModelControl modelControl;

    [SerializeField] private bool networkOverride;

    private AudioSource spikeSource;

    private int[] ccfSize = { 528, 320, 456 };
    private int ccfStride = 25; // to convert back and forth from ccf index space to neuron space

    private Dictionary<string, NeuronDataset> neuronDatasets;
    private AnnotationDataset annotationDataset;

    // this file stores the indexes that we actually have data for
    private string datasetIndexFile = "data_indexes";
    private byte[] ccfIndexMap;

    // annotation files
    private string annotationIndexFile = "ann/indexes";
    // rf files
    private string x_rfIndexFile = "rf/x_rf_index";
    private string y_rfIndexFile = "rf/y_rf_index";

    // NEURON DATA
    private float neuronExistPerc = 0.125f;
    private bool[,,] neuronExists;
    private bool[,,] neuronDataRequested;
    private Entity[,,] neurons;
    private int neuronMaxRange = 1; // real range is nMR * ccfStride

    // STIMULUS SET
    private List<VisualStimulus> visualStimuli;
    private float _wheelVelocity;

    //private int maxNeurons = 1000;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 144;
        visualStimuli = new List<VisualStimulus>();

        // Setup spiking
        spikeSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void ServerSideSetup()
    {
        if (!IsServer) { return; }

        Debug.LogWarning("Replace ndarray with jagged array");
        neurons = new Entity[ccfSize[0], ccfSize[1], ccfSize[2]];
        neuronExists = new bool[ccfSize[0], ccfSize[1], ccfSize[2]];
        neuronDataRequested = new bool[ccfSize[0], ccfSize[1], ccfSize[2]];

        for (int x = 0; x < ccfSize[0]; x++)
        {
            for (int y = 0; y < ccfSize[1]; y++)
            {
                for (int z = 0; z < ccfSize[2]; z++)
                {
                    neuronExists[x, y, z] = UnityEngine.Random.value < neuronExistPerc;
                    neuronDataRequested[x, y, z] = false;
                }
            }
        }

        LoadAllDatasets();
    }

    // ELECTRODE SITE FUNCTIONALITY
    
    public void ElectrodeMovedCallback(ulong clientId, int3 probeCoords)
    {
        if (!IsClient) { return; }

        // On the client, search the local space and see whether we need to populate any of the neurons
        // that are nearby us
        int ap25 = probeCoords.x; int dv25 = probeCoords.y; int lr25 = probeCoords.z;

        List<int3> apdvlrList = new List<int3>();

        // If the ap/dv/lr are outside the range in ccfSize we just ignore the tip position
        if ((ap25 >= 0 && ap25 < ccfSize[0]) && (dv25 >= 0 && dv25 < ccfSize[1]) && (lr25 >= 0 && lr25 < ccfSize[2]))
        {
            // We are within range, check if we have an annotation
            int annotation = annotationDataset.ValueAtIndex(ap25, dv25, lr25);

            // if the annotation exists (i.e. not just 0 or 1)
            if (annotation > 1)
            {

                // Request data for the neurons that are near the electrode tip
                for (int ai = Math.Max(ap25 - neuronMaxRange, 0); ai <= Math.Min(ap25 + neuronMaxRange, ccfSize[0]); ai++)
                {
                    for (int di = Math.Max(dv25 - neuronMaxRange, 0); di <= Math.Min(dv25 + neuronMaxRange, ccfSize[1]); di++)
                    {
                        for (int li = Math.Max(lr25 - neuronMaxRange, 0); li <= Math.Min(lr25 + neuronMaxRange, ccfSize[2]); li++)
                        {
                            if (neurons[ai,di,li]==Entity.Null && !neuronDataRequested[ai, di, li])
                            {
                                // The neuron exists, we don't have it's data, and we haven't requested it's data
                                apdvlrList.Add(new int3(ai, di, li));
                            }
                        }
                    }
                }
            }

            if (apdvlrList.Count > 0)
            {
                int[] ais = new int[apdvlrList.Count];
                int[] dis = new int[apdvlrList.Count];
                int[] lis = new int[apdvlrList.Count];
                for (int i = 0; i < apdvlrList.Count; i++)
                {
                    ais[i] = apdvlrList[i].x;
                    dis[i] = apdvlrList[i].y;
                    lis[i] = apdvlrList[i].z;
                    neuronDataRequested[ais[i], dis[i], lis[i]] = true;
                }
                Debug.Log("Requesting data for: " + apdvlrList.Count + " neurons");


                // Tell the client now to update their Neuron/Electrode map
                ElectrodeMovedServerRpc(clientId, ais, dis, lis);
            }
        }

    }

    public int3 SitePosition2apdvlr(Vector3 sitePosition)
    {
        // Convert sitePosition to index
        // APDVLR is in the original scaling, we also want these in annotation dataset units
        int aScale = 1000 / 25;
        int ap25 = (int)Mathf.Round(sitePosition.x * aScale);
        int dv25 = (int)Mathf.Round(sitePosition.y * aScale);
        int lr25 = (int)Mathf.Round(sitePosition.z * aScale);
        return new int3(ap25, dv25, lr25);
    }


    /**
     * When an electrode is moved we ping the server to request the nearby neurons as well as their data
     * we then send this back to the client in a clientRpc call
     * 
     * On the client side they will then invoke the NeuronEntityManager to handle actually spawning
     * the neurons. These will get sent back to the ElectrodeManager to store in the neurons[,,] so 
     * we can access these later.
     */
    [ServerRpc(RequireOwnership = false)]
    public void ElectrodeMovedServerRpc(ulong clientId, int[] ais, int[] dis, int[] lis)
    {
        if (!IsServer) { return; }

        // Get the data for these neurons, return it to the client
        Debug.Log("called server rpc");
        Debug.Log("got list with length: " + ais.Length);

        // Check if each neuron exists (some neurons are ignored by the 12.5% spawn rate)
        List<int> indexExists = new List<int>();
        for (int i = 0; i < ais.Length; i++)
        {
            if (neuronExists[ais[i],dis[i],lis[i]])
            {
                indexExists.Add(i);
            }
        }

        // Convert the list data to int arrays and save out data

        // apdvlr location arrays
        int[] ret_ais = new int[indexExists.Count];
        int[] ret_dis = new int[indexExists.Count];
        int[] ret_lis = new int[indexExists.Count];
        // rf data arrays
        float[] rf_x = new float[indexExists.Count];
        float[] rf_y = new float[indexExists.Count];

        // [TODO] In the future, it would be ideal to have overrided ClientRpc calls for different data types
        // e.g. if you are in visual cortex, send rf data, but if you are in an area with no RF, skip that data type

        for (int i = 0; i < indexExists.Count; i++)
        {
            int ap = ais[indexExists[i]];
            int dv = dis[indexExists[i]];
            int lr = lis[indexExists[i]];

            // add coordinates
            ret_ais[i] = ap;
            ret_dis[i] = dv;
            ret_lis[i] = lr;

            // add rf data
            rf_x[i] = getData("rf_x", ap, dv, lr);
            rf_y[i] = getData("rf_y", ap, dv, lr);
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[]{clientId}
            }
        };

        // Spawn the neurons on the client
        ElectrodeMovedClientRpc(clientId, ret_ais, ret_dis, ret_lis, rf_x, rf_y, clientRpcParams);
    }

    /**
     * Returns the flattened index coordinate for a 3d coordinate
     */
    private int ccf2idx(int x, int y, int z)
    {
        return z * ccfSize[0] * ccfSize[1] + y * ccfSize[1] + x;
    }

    private Vector3 idx2ccf(int idx)
    {
        int z = idx / (ccfSize[0] * ccfSize[1]);
        idx -= z * ccfSize[0] * ccfSize[1];
        int y = idx / ccfSize[0];
        int x = idx % ccfSize[0];
        return new Vector3(x, y, z);
    }
    
    /*
     * The ClientRpc needs to figure out which Neurons actually matter for the electrodes
     * that are on the current probe. So for that we will call the FindNearbyNeurons function
     * from ElectrodeSite
     */
    [ClientRpc]
    public void ElectrodeMovedClientRpc(ulong calledByClientId, int[] ais, int[] dis, int[] lis, float[] rf_x, float[] rf_y, ClientRpcParams clientRpcParams = default)
    {

        // We only want to find nearby neurons if we were the client that moved the probe
        if (OwnerClientId != calledByClientId) return;
        Debug.Log("called client rpc");
        Debug.Log("got list with length: " + ais.Length);

        List<float3> mlapdv = new List<float3>();
        List<int3> apdvlr = new List<int3>();
        List<float3> rfData = new List<float3>();
        for (int i = 0; i < ais.Length; i++)
        {
            // Note that we rotate the order because we handle entities in mlapdv
            mlapdv.Add((new float3(lis[i], ais[i], dis[i]) * ccfStride + (ccfStride/2)) / 1000);
            // For now we are also going to keep track of the apdvlr coordinate as an int, since this is useful for the simulation
            apdvlr.Add(new int3(ais[i], dis[i], lis[i]));
            // RF data
            rfData.Add(new float3(rf_x[i], rf_y[i], 5));
        }

        List<Entity> entities = nemanager.AddNeurons(mlapdv, apdvlr, rfData);

        for (int i = 0; i < mlapdv.Count; i++) 
        {
            neurons[ais[i], dis[i], lis[i]] = entities[i];
            // in case we delete a neuron later, make sure to set DataRequested to false
            neuronDataRequested[ais[i], dis[i], lis[i]] = false;
        }

        // Get all ElectrodeSites
        // [TODO] Later this needs to be removed so that the site list is stored explicitly and updated
        // only when a probe is added or removed, as is this is very inefficient
        GameObject[] sites = GameObject.FindGameObjectsWithTag("ElectrodeSite");
        foreach (GameObject site in sites)
        {
            site.GetComponent<ElectrodeSite>().FindNearbyNeurons();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void RegisterVisualStimulus(VisualStimulus v)
    {
        visualStimuli.Add(v);
    }

    public void RemoveVisualStimulus(VisualStimulus v)
    {
        visualStimuli.Remove(v);
    }

    public void SetWheelVelocity(float wheelVelocity)
    {
        _wheelVelocity = wheelVelocity;
    }

    public Vector3 ConvertBrainPositionToCCF(Vector3 positionInBrainModel)
    {
        Vector3 temp = positionInBrainModel + new Vector3(-5.7f, -4f, 6.6f);
        return new Vector3(temp.z, -temp.y, -temp.x);
    }

    private void Spike()
    {
        spikeSource.PlayOneShot(spikeSource.clip);
    }

    // ** DATASET FUNCTIONALITY ** //
    public void LoadAllDatasets()
    {
        if (!IsServer && !networkOverride) return;

        neuronDatasets = new Dictionary<string, NeuronDataset>();

        // First load the indexing file
        Debug.Log("Loading the CCF index file");
        ccfIndexMap = util.LoadBinaryByteHelper(datasetIndexFile);

        // Load the annotation file
        Debug.Log("Loading the CCF annotation index and map files");
        ushort[] annData = util.LoadBinaryUShortHelper(annotationIndexFile);
        uint[] annMap = util.LoadBinaryUInt32Helper(annotationIndexFile + "_map");
        Debug.Log("Creating the CCF AnnotationDataset object");
        annotationDataset = new AnnotationDataset("annotation", annData, annMap, ccfIndexMap);

        // Load the RF data
        Debug.Log("Loading the X/Y reference data");
        byte[] xData = util.LoadBinaryByteHelper(x_rfIndexFile);
        float[] xMap = LoadCSVHelper(x_rfIndexFile + "_map");
        neuronDatasets.Add("rf_x", new NeuronDataset(xData, xMap, ccfSize, ccfIndexMap));
        byte[] yData = util.LoadBinaryByteHelper(y_rfIndexFile);
        float[] yMap = LoadCSVHelper(y_rfIndexFile + "_map");
        neuronDatasets.Add("rf_y", new NeuronDataset(yData, yMap, ccfSize, ccfIndexMap));
    }

    private float[] LoadCSVHelper(string fileName)
    {
        TextAsset textAsset = Resources.Load("Datasets/" + fileName) as TextAsset;
        string[] lines = textAsset.text.Split("\n"[0]);
        float[] data = new float[lines.Length];
        for (int i=0; i<(lines.Length-1); i++)
        {
            string[] lineData = (lines[i].Trim()).Split(","[0]);
            float.TryParse(lineData[1], out data[i]);
        }

        return data;
    }

    public int GetAnnotation(int ap, int dv, int lr)
    {
        return annotationDataset.ValueAtIndex(ap, dv, lr);
    }

    public string GetAnnotationAcronym(int ap, int dv, int lr)
    {
        return modelControl.GetCCFAreaAcronym(GetAnnotation(ap, dv, lr));
    }

    public float getData(string datasetName, int ap, int dv, int lr)
    {
        return neuronDatasets[datasetName].ValueAtIndex(ap, dv, lr);
    }
}
