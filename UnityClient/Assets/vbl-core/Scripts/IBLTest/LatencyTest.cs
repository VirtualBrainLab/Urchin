using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LatencyTest : MonoBehaviour
{
    [SerializeField] private CCFModelControl modelControl;
    [SerializeField] private float maxExplosion = 10f;

    [SerializeField] private TextMeshProUGUI elapsedTimeText;
    [SerializeField] private UM_CameraController cameraController;

    [SerializeField] private GameObject tooltipPanelGO;

    [Range(0, 1), SerializeField] private float percentageExploded = 0f;
    [Range(0, 0.31f), SerializeField] private float elapsedTime = 0f;

    private float prevPerc = 0f;
    private float prevElapsed = 0f;

    private Dictionary<int, float> latencies;
    private Dictionary<int, CCFTreeNode> nodes;

    private Vector3 center = new Vector3(5.7f, 4f, -6.6f);

    private Vector3 teal = new Vector3(0f, 1f, 1f);
    private Vector3 magenta = new Vector3(1f, 0f, 1f);

    private int[] cosmos = { 315, 698, 1089, 703, 623, 549, 1097, 313, 1065, 512 };
    private Dictionary<int, Vector3> cosmosMeshCenters;
    private Dictionary<int, Vector3> originalTransformPositions;
    private Dictionary<int, Vector3> nodeMeshCenters;

    private void Awake()
    {
        tooltipPanelGO.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        latencies = new Dictionary<int, float>();
        nodes = new Dictionary<int, CCFTreeNode>();
        cosmosMeshCenters = new Dictionary<int, Vector3>();
        nodeMeshCenters = new Dictionary<int, Vector3>();
        originalTransformPositions = new Dictionary<int, Vector3>();

        modelControl.SetBeryl(true);
        modelControl.LateStart(false);

        List<Dictionary<string, object>> data = CSVReader.Read("Datasets/ibl_results/latencies");

        for (int i = 0; i < data.Count; i++)
        {
            Dictionary<string, object> row = data[i];

            string acronym = (string)row["acronym"];
            float lat = (float)row["latency"];
            int ID = modelControl.Acronym2ID(acronym);
            latencies.Add(ID, lat);
        }


        foreach (KeyValuePair<int, float> pair in latencies)
        {
            CCFTreeNode node = modelControl.tree.findNode(pair.Key);
            if (node != null)
            {
                node.loadNodeModel(true);
                node.SetNodeModelVisibility(false,false);
                GameObject nodeGO = node.LeftGameObject();
                nodeGO.AddComponent<MeshCollider>();
                LatencyTest_MouseOver mo = nodeGO.AddComponent<LatencyTest_MouseOver>();
                mo.SetNode(node);
                mo.SetTooltip(tooltipPanelGO);

                originalTransformPositions.Add(node.ID, node.GetNodeTransform().position);
                nodeMeshCenters.Add(node.ID, node.GetMeshCenter());
                nodes.Add(pair.Key, node);
            }
        }

        // save the cosmos transform positions
        foreach (int cosmosID in cosmos)
        {
            GameObject cosmosGO = GameObject.Find(cosmosID + "L");
            cosmosMeshCenters.Add(cosmosID, cosmosGO.GetComponentInChildren<Renderer>().bounds.center);
            cosmosGO.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        

        if (prevElapsed != elapsedTime)
        {
            elapsedTimeText.text = String.Format("Time since stim onset: {0:0.000}s", elapsedTime);

            foreach (KeyValuePair<int, float> pair in latencies)
            {
                CCFTreeNode node = nodes[pair.Key];
                float latency = latencies[pair.Key];

                if (elapsedTime >= latency)
                {
                    node.SetNodeModelVisibility(true,false);
                }
                else
                {
                    node.SetNodeModelVisibility(false,false);
                }
            }
            prevElapsed = elapsedTime;
        }

        // Check if we need to make an update
        if (prevPerc != percentageExploded)
        {
            prevPerc = percentageExploded;

            // for each tree node, move it's model away from the 0,0,0 point
            foreach (CCFTreeNode node in nodes.Values)
                node.ExplodeModel(Vector3.zero, maxExplosion * percentageExploded);
        }
    }

    public void UpdateElapsedTime(float newElapsedTime)
    {
        cameraController.BlockDragging();
        elapsedTime = newElapsedTime;
    }

    public void UpdateExploded(float newPerc)
    {
        cameraController.BlockDragging();
        foreach (CCFTreeNode node in nodes.Values)
        {
            int cosmos = modelControl.GetCosmosID(node.ID);
            Transform nodeT = node.GetNodeTransform();
            nodeT.position = originalTransformPositions[node.ID] + 
                (cosmosMeshCenters[cosmos] - nodeMeshCenters[node.ID]) * newPerc;
        }
    }
}
