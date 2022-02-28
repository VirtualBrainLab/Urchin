using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UM_Launch : MonoBehaviour
{
    [SerializeField] private CCFModelControl modelControl;
    [SerializeField] private UM_CameraController cameraController;
    [SerializeField] private float maxExplosion = 10f;

    [SerializeField] private GameObject consolePanel;
    [SerializeField] private TextMeshProUGUI consoleText;

    [SerializeField] private bool loadDefaults;

    [Range(0,1), SerializeField] private float percentageExploded = 0f;
    private float prevPerc = 0f;

    private Vector3 center = new Vector3(5.7f, 4f, -6.6f);

    private Vector3 teal = new Vector3(0f, 1f, 1f);
    private Vector3 magenta = new Vector3(1f, 0f, 1f);

    private int[] cosmos = { 315, 698, 1089, 703, 623, 549, 1097, 313, 1065, 512 };
    private Dictionary<int, Vector3> cosmosMeshCenters;
    private Dictionary<int, Vector3> originalTransformPositions;
    private Dictionary<int, Vector3> nodeMeshCenters;
    private Dictionary<int, Vector3> cosmosVectors;
    
    private Dictionary<int, CCFTreeNode> visibleNodes;

    // COSMOS
    [SerializeField] private List<GameObject> cosmosParentObjects;
    private int cosmosParentIdx = 0;

    private bool ccfLoaded;

    // Start is called before the first frame update
    void Start()
    {
        originalTransformPositions = new Dictionary<int, Vector3>();
        nodeMeshCenters = new Dictionary<int, Vector3>();

        visibleNodes = new Dictionary<int, CCFTreeNode>();

        modelControl.SetBeryl(true);
        modelControl.LateStart(loadDefaults);

        if (loadDefaults)
            DelayedStart();

        RecomputeCosmosCenters();
    }

    private async void DelayedStart()
    {
        await modelControl.GetDefaultLoaded();
        ccfLoaded = true;

        foreach (CCFTreeNode node in modelControl.GetDefaultLoadedNodes())
        {
            RegisterNode(node);
            node.SetNodeModelVisibility(true, true);
        }
    }

    public void ChangeCosmosIdx(int newIdx)
    {
        cosmosParentIdx = newIdx;
        RecomputeCosmosCenters();
        UpdateExploded(percentageExploded);
    }

    private void RecomputeCosmosCenters()
    {
        GameObject parentGO = cosmosParentObjects[cosmosParentIdx];
        parentGO.SetActive(true);

        cosmosMeshCenters = new Dictionary<int, Vector3>();
        cosmosVectors = new Dictionary<int, Vector3>();

        // save the cosmos transform positions
        foreach (int cosmosID in cosmos)
        {
            GameObject cosmosGO = parentGO.transform.Find(cosmosID + "L").gameObject;
            cosmosMeshCenters.Add(cosmosID, cosmosGO.GetComponentInChildren<Renderer>().bounds.center);
            cosmosGO.SetActive(false);

            cosmosVectors.Add(cosmosID, cosmosGO.transform.localPosition);
        }

        parentGO.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        //Check if we need to make an update
        if (prevPerc != percentageExploded)
        {
            prevPerc = percentageExploded;

            // for each tree node, move it's model away from the 0,0,0 point
            _UpdateExploded();
        }

        // Check for key down events
        if (Input.GetKeyDown(KeyCode.C))
        {
            consolePanel.SetActive(!consolePanel.activeSelf);
        }
    }

    public void RegisterNode(CCFTreeNode node)
    {
        originalTransformPositions.Add(node.ID, node.GetNodeTransform().localPosition);
        nodeMeshCenters.Add(node.ID, node.GetMeshCenter());
        visibleNodes.Add(node.ID,node);
    }

    // [TODO] Refactor colormaps into their own class
    public Color Cool(float perc)
    {
        Vector3 colorVector = Vector3.Lerp(teal, magenta, perc);
        return new Color(colorVector.x, colorVector.y, colorVector.z, 1f);
    }

    public void Log(string text)
    {
        // Todo: deal with log running off the screen
        Debug.Log(text);
        consoleText.text += "\n" + text;
    }

    public void UpdateExploded(float newPercExploded)
    {
        percentageExploded = newPercExploded;
        _UpdateExploded();
    }

    private void _UpdateExploded()
    {
        cameraController.BlockDragging();
        foreach (CCFTreeNode node in visibleNodes.Values)
        {
            int cosmos = modelControl.GetCosmosID(node.ID);
            Transform nodeT = node.GetNodeTransform();

            if (!cosmosVectors.ContainsKey(cosmos))
            {
                Debug.Log(node.ID);
                Debug.Log(cosmos);
            }
            nodeT.localPosition = originalTransformPositions[node.ID] +
                cosmosVectors[cosmos] * percentageExploded;

            //nodeT.localPosition = originalTransformPositions[node.ID] +
            //    (cosmosMeshCenters[cosmos] - nodeMeshCenters[node.ID]) * newPerc;
        }
    }

    public void UpdateDataIndex(float newIdx)
    {
        Debug.Log(newIdx);
    }
}
