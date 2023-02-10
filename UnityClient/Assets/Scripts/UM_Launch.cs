using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UM_Launch : MonoBehaviour
{
    #region Static
    public static UM_Launch Instance;
    #endregion

    [SerializeField] private CCFModelControl modelControl;
    [SerializeField] private BrainCameraController cameraController;
    [SerializeField] private Client client;
    [SerializeField] private ColormapPanel _colormapPanel;

    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Transform brainControlsT;

    [SerializeField] private GameObject consolePanel;
    [SerializeField] private TextMeshProUGUI consoleText;

    [SerializeField] private GameObject idPanel;
    [SerializeField] private GameObject infoPanelText;

    [SerializeField] private bool loadDefaults;

    // Axes + Grid
    [SerializeField] private GameObject axesGO;
    [SerializeField] private GameObject gridGO;

    // Exploding
    [Range(0,1), SerializeField] private float percentageExploded = 0f;
    private float prevPerc = 0f;
    private bool explodeLeftOnly;
    private bool exploded;
    public bool colorLeftOnly { get; private set; }

    private Vector3 center = new Vector3(5.7f, 4f, -6.6f);

    // Neuron materials
    [SerializeField] private Dictionary<string, Material> neuronMaterials;

    // Colormaps
    private List<Converter<float, Color>> colormaps;
    private List<string> colormapOptions = new List<string>{"cool","gray","grey-green","grey-purple","grey-red","grey-rainbow"};
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
    
    // COSMOS
    [SerializeField] private List<GameObject> cosmosParentObjects;
    private int cosmosParentIdx = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            throw new Exception("There should only be on UM_Launch component in the scene");

        colormaps = new List<Converter<float, Color>>();
        colormaps.Add(Cool);
        colormaps.Add(Grey);
        colormaps.Add(GreyGreen);
        colormaps.Add(GreyPurple);
        colormaps.Add(GreyRed);
        colormaps.Add(GreyRainbow);
        activeColormap = Cool;

        originalTransformPositionsLeft = new Dictionary<int, Vector3>();
        originalTransformPositionsRight = new Dictionary<int, Vector3>();
    }

    // Start is called before the first frame update
    void Start()
    {
        modelControl.SetBeryl(true);
        modelControl.LateStart(loadDefaults);

        if (loadDefaults)
            DelayedStart();

        RecomputeCosmosCenters();
    }

    private async void DelayedStart()
    {
        await modelControl.GetDefaultLoadedTask();

        foreach (CCFTreeNode node in modelControl.GetDefaultLoadedNodes())
        {
            RegisterNode(node);
            node.SetNodeModelVisibility_Full(true);
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
            UpdateExploded();
        }

        // Before checking for keydown events, return if the user is typing in the input box
        if (idPanel.GetComponent<TMP_InputField>().isFocused)
            return;

        // Check for key down events
        if (Input.GetKeyDown(KeyCode.C))
        {
            consolePanel.SetActive(!consolePanel.activeSelf);
        }

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

        if (Input.GetKeyDown(KeyCode.I))
        {
            idPanel.SetActive(!idPanel.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            KeyExplodeImplode(2f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            KeyExplodeImplode(5f);
        }
    }

    public void RegisterNode(CCFTreeNode node)
    {
        if (node != null && node.NodeModelLeftGO != null)
        {
            if (!originalTransformPositionsLeft.ContainsKey(node.ID))
                originalTransformPositionsLeft.Add(node.ID, node.NodeModelLeftGO.transform.localPosition);
            if (!originalTransformPositionsRight.ContainsKey(node.ID))
                originalTransformPositionsRight.Add(node.ID, node.NodeModelRightGO.transform.localPosition);
        }
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

        _colormapPanel.SetColormap(activeColormap);
        _colormapPanel.SetColormapVisibility(true);
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

    public Color GreyRainbow(float perc)
    {
        if (perc == 0)
            return Color.grey;
        else
        {
            // Interpolate rainbow map
            float red = Mathf.Abs(2f * perc - 0.5f);
            float green = Mathf.Sin(perc * Mathf.PI);
            float blue = Mathf.Cos(perc * Mathf.PI / 2);
            return new Color(red, green, blue);
        }
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

    public static void Log(string text)
    {
        // Todo: deal with log running off the screen
        Debug.Log(text);
        Instance.consoleText.text += "\n" + text;
    }

    public void SetLeftExplodeOnly(bool state)
    {
        explodeLeftOnly = state;
        UpdateExploded();
    }

    public void UpdateExploded(float newPercExploded)
    {
        percentageExploded = newPercExploded;
        UpdateExploded();
    }

    public void SetLeftColorOnly(bool state)
    {
        colorLeftOnly = state;
        foreach (CCFTreeNode node in AreaManager.VisibleNodes)
            if (state)
                node.SetColorOneSided(node.DefaultColor, false, false);
            else
                node.SetColor(node.color);
    }

    private void UpdateExploded()
    {
        cameraController.SetControlBlock(true);

        Vector3 flipVector = new Vector3(1f, 1f, -1f);
        foreach (CCFTreeNode node in AreaManager.VisibleNodes)
        {
            int cosmos = modelControl.GetCosmosID(node.ID);
            Transform nodeTLeft = node.NodeModelLeftGO.transform;
            Transform nodeTright = node.NodeModelRightGO.transform;

            nodeTLeft.localPosition = originalTransformPositionsLeft[node.ID] +
                cosmosVectors[cosmos] * percentageExploded;

            if (explodeLeftOnly)
                nodeTright.localPosition = originalTransformPositionsRight[node.ID];
            else
                nodeTright.localPosition = originalTransformPositionsRight[node.ID] +
                        Vector3.Scale(cosmosVectors[cosmos], flipVector) * percentageExploded;
        }
    }

    public void UpdateDataIndex(float newIdx)
    {
        Debug.Log(newIdx);
    }

    public void UpdateTextColor(bool state)
    {
        foreach (TMP_Text text in infoPanelText.GetComponentsInChildren<TMP_Text>())
            text.color = state ? Color.black : Color.white;
    }

    public void KeyExplodeImplode(float explodeTime)
    {
        exploded = !exploded;
        if (exploded)
            StartCoroutine(AnimateExplodeHelper(explodeTime));
        else
            StartCoroutine(AnimateImplodeHelper(explodeTime));
    }

    public void AnimateExplode(float explodeTime)
    {
        StartCoroutine(AnimateExplodeHelper(explodeTime));
    }
    public void AnimateImplode(float explodeTime)
    {
        StartCoroutine(AnimateImplodeHelper(explodeTime));
    }

    private IEnumerator AnimateExplodeHelper(float explodeTime)
    {
        float start = Time.realtimeSinceStartup;
        float end = explodeTime + start;

        while (Time.realtimeSinceStartup <= end)
        {
            yield return null;
            percentageExploded = Mathf.InverseLerp(start, end, Time.realtimeSinceStartup);
        }
    }
    private IEnumerator AnimateImplodeHelper(float explodeTime)
    {
        float start = Time.realtimeSinceStartup;
        float end = explodeTime + start;

        while (Time.realtimeSinceStartup <= end)
        {
            yield return null;
            percentageExploded = 1f - Mathf.InverseLerp(start, end, Time.realtimeSinceStartup);
        }
    }
}
