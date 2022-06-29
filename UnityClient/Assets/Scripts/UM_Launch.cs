using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UM_Launch : MonoBehaviour
{
    [SerializeField] private CCFModelControl modelControl;
    [SerializeField] private BrainCameraController cameraController;
    [SerializeField] private float maxExplosion = 10f;

    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Transform brainControlsT;

    [SerializeField] private GameObject consolePanel;
    [SerializeField] private TextMeshProUGUI consoleText;

    [SerializeField] private bool loadDefaults;

    // Axes + Grid
    [SerializeField] private GameObject axesGO;
    [SerializeField] private GameObject gridGO;

    // Exploding
    [Range(0,1), SerializeField] private float percentageExploded = 0f;
    private float prevPerc = 0f;
    private bool explodeLeftOnly;
    private bool colorLeftOnly;

    private Vector3 center = new Vector3(5.7f, 4f, -6.6f);

    // Neuron materials
    [SerializeField] private Dictionary<string, Material> neuronMaterials;

    // Colormaps
    private List<Converter<float, Color>> colormaps;
    private List<string> colormapOptions = new List<string>{"cool","gray","grey-green","grey-purple","grey-red"};
    private Converter<float, Color> activeColormap;

    // Starting colors
    private Vector3 teal = new Vector3(0f, 1f, 1f);
    private Vector3 magenta = new Vector3(1f, 0f, 1f);
    private Vector3 lightgreen = new Vector3(14f / 255f, 1f, 0f);
    private Vector3 darkgreen = new Vector3(6f / 255f, 59 / 255f, 0f);
    private Vector3 lightpurple = new Vector3(202f / 255f, 105f / 255f, 227f / 255f);
    private Vector3 darkpurple = new Vector3(141f / 255f, 10f / 255f, 157f / 255f);
    private Vector3 lightred = new Vector3(1f, 165f / 255f, 0f);
    private Vector3 darkred = new Vector3(1f, 0f, 0f);

    private int[] cosmos = { 315, 698, 1089, 703, 623, 549, 1097, 313, 1065, 512 };
    private Dictionary<int, Vector3> cosmosMeshCenters;
    private Dictionary<int, Vector3> originalTransformPositionsLeft;
    private Dictionary<int, Vector3> originalTransformPositionsRight;
    private Dictionary<int, Vector3> cosmosVectors;
    
    private Dictionary<int, CCFTreeNode> visibleNodes;

    // COSMOS
    [SerializeField] private List<GameObject> cosmosParentObjects;
    private int cosmosParentIdx = 0;

    private bool ccfLoaded;

    // Start is called before the first frame update
    void Start()
    {
        colormaps = new List<Converter<float, Color>>();
        colormaps.Add(Cool);
        colormaps.Add(Grey);
        colormaps.Add(GreyGreen);
        colormaps.Add(GreyPurple);
        colormaps.Add(GreyRed);
        activeColormap = Cool;

        originalTransformPositionsLeft = new Dictionary<int, Vector3>();
        originalTransformPositionsRight = new Dictionary<int, Vector3>();

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
            FixNodeTransformPosition(node);

            RegisterNode(node);
            node.SetNodeModelVisibility(true, true);
        }
    }

    public void FixNodeTransformPosition(CCFTreeNode node)
    {
        // I don't know why we have to do this? For some reason when we load the node models their positions are all offset in space in a weird way... 
        node.GetNodeTransform().localPosition = Vector3.zero;
        node.GetNodeTransform().localRotation = Quaternion.identity;
        node.RightGameObject().transform.localPosition = Vector3.forward * 11.4f;
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

        // Check for key down events
        if (Input.GetKeyDown(KeyCode.S))
        {
            // Hacky workaround because the brain controls doesn't attach to the UI element properly
            if (settingsPanel.activeSelf)
                brainControlsT.localPosition = new Vector3(-82.3f, -85.6f, 0f);
            else
                brainControlsT.localPosition = new Vector3(20.2f, -85.6f, 0f);
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            axesGO.SetActive(!axesGO.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            gridGO.SetActive(!gridGO.activeSelf);
        }
    }

    public void RegisterNode(CCFTreeNode node)
    {
        if (!originalTransformPositionsLeft.ContainsKey(node.ID))
            originalTransformPositionsLeft.Add(node.ID, node.LeftGameObject().transform.localPosition);
        if (!originalTransformPositionsRight.ContainsKey(node.ID))
            originalTransformPositionsRight.Add(node.ID, node.RightGameObject().transform.localPosition);
        if (!visibleNodes.ContainsKey(node.ID))
            visibleNodes.Add(node.ID,node);
    }

    public Color GetColormapColor(float perc)
    {
        return activeColormap(perc);
    }

    public void ChangeColormap(string newColormap)
    {
        if (colormapOptions.Contains(newColormap))
            activeColormap = colormaps[colormapOptions.IndexOf(newColormap)];
        else
            Log("Colormap " + newColormap + " not an available option");
    }

    // [TODO] Refactor colormaps into their own class
    public Color Cool(float perc)
    {
        perc = CheckColormapRange(perc);
        Vector3 colorVector = Vector3.Lerp(teal, magenta, perc);
        return new Color(colorVector.x, colorVector.y, colorVector.z, 1f);
    }

    public Color Grey(float perc)
    {
        perc = CheckColormapRange(perc);
        Vector3 colorVector = Vector3.Lerp(Vector3.zero, Vector3.one, perc);
        return new Color(colorVector.x, colorVector.y, colorVector.z, 1f);
    }

    public Color GreyGreen(float perc)
    {
        perc = CheckColormapRange(perc);
        return GreyGradient(perc, lightgreen, darkgreen);
    }
    public Color GreyPurple(float perc)
    {
        perc = CheckColormapRange(perc);
        return GreyGradient(perc, lightpurple, darkpurple);
    }

    public Color GreyRed(float perc)
    {
        perc = CheckColormapRange(perc);
        return GreyGradient(perc, lightred, darkred);
    }

    public float CheckColormapRange(float perc)
    {
        return Mathf.Clamp(perc, 0, 1);
    }

    public Color GreyGradient(float perc, Vector3 lightcolor, Vector3 darkcolor)
    {
        if (perc == 0)
            return Color.grey;
        else
        {
            Vector3 colorVector = Vector3.Lerp(lightcolor, darkcolor, perc);
            return new Color(colorVector.x, colorVector.y, colorVector.z, 1f);
        }
    }

    public void Log(string text)
    {
        // Todo: deal with log running off the screen
        Debug.Log(text);
        consoleText.text += "\n" + text;
    }

    public void SetLeftExplodeOnly(bool state)
    {
        explodeLeftOnly = state;
        _UpdateExploded();
    }

    public void UpdateExploded(float newPercExploded)
    {
        percentageExploded = newPercExploded;
        _UpdateExploded();
    }

    public void SetLeftColorOnly(bool state)
    {
        foreach (CCFTreeNode node in visibleNodes.Values)
            if (state)
                node.SetColorOneSided(node.GetDefaultColor(), false, false);
            else
                node.SetColorOneSided(node.GetColor(), false, false);
    }

    private void _UpdateExploded()
    {
        cameraController.SetControlBlock(true);

        Vector3 flipVector = new Vector3(1f, 1f, -1f);

        foreach (CCFTreeNode node in visibleNodes.Values)
        {
            int cosmos = modelControl.GetCosmosID(node.ID);
            Transform nodeTLeft = node.LeftGameObject().transform;
            Transform nodeTright = node.RightGameObject().transform;

            nodeTLeft.localPosition = originalTransformPositionsLeft[node.ID] +
                cosmosVectors[cosmos] * percentageExploded;

            if (!explodeLeftOnly)
            {
                nodeTright.localPosition = originalTransformPositionsRight[node.ID] +
                    Vector3.Scale(cosmosVectors[cosmos], flipVector) * percentageExploded;
            }
            else
            {
                nodeTright.localPosition = originalTransformPositionsRight[node.ID];
            }
        }
    }

    public void UpdateDataIndex(float newIdx)
    {
        Debug.Log(newIdx);
    }

    /// <summary>
    /// Switch the main camera to perspective (or back to orthographic, the default)
    /// 
    /// Note: you can't view the volumetric data in the orthographic camera, hence this setting to switch back and forth
    /// </summary>
    /// <param name="perspective"></param>
    public void SwitchCameraMode(bool perspective)
    {
        
    }
}
