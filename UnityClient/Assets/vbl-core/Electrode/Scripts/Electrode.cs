using UnityEngine;
using MLAPI;
using UnityEngine.UI;
using MLAPI.Messaging;
using Unity.Mathematics;
using TMPro;

public class Electrode : NetworkBehaviour {
    public GameObject electrodeSitePrefab;
    public Camera tipCamera;

    private Collider rootCollider;
    private Collider tipCollider;

    private Vector2[] sitePositions;
    private GameObject[] sites;

    private NetworkObject netObject;

    private ElectrodeManager emanager;
    private PlayerManager pmanager;
    private PanelController pcontroller;

    private Electrode electrode;

    private bool inBrain;
    private bool activeElectrode;

    private float defaultSpeed = 5.0f;
    private float rotSpeed = 0.25f;
    private float curSpeed;

    private Text apLabel;
    private Text dvLabel;
    private Text lrLabel;
    private TextMeshProUGUI areaLabel;

    // Plot information
    private GameObject spikeTracePlot;
    private GameObject spikeRasterPlot;
    private GameObject probeImage;

    // Panel information
    private GameObject spikePanel;
    private GameObject spikeMinPanel;

    private void Start()
    {
        // Get some msising references
        rootCollider = GameObject.FindGameObjectWithTag("BrainRoot").GetComponent<Collider>();
        tipCollider = gameObject.GetComponentInChildren<Collider>();

        // Set managers
        emanager = GameObject.Find("BrainDataManager").GetComponent<ElectrodeManager>();
        pmanager = GameObject.Find("BrainDataManager").GetComponent<PlayerManager>();
        pcontroller = GameObject.Find("main").GetComponent<PanelController>();

        inBrain = false;
        curSpeed = defaultSpeed;

        electrode = gameObject.GetComponent<Electrode>();

        // Create UI panels
        GameObject[] panelHolder = pcontroller.AddNewProbePanels(gameObject);
        spikePanel = panelHolder[0];
        spikeMinPanel = panelHolder[1];

        Transform spikePanelTransform = spikePanel.transform;

        SpikePanelData panelData = spikePanelTransform.GetComponent<SpikePanelData>();
        apLabel = panelData.InfoPanelAP().GetComponent<Text>();
        dvLabel = panelData.InfoPanelDV().GetComponent<Text>();
        lrLabel = panelData.InfoPanelLR().GetComponent<Text>();
        spikeTracePlot = panelData.SpikeTracePlot();
        spikeRasterPlot = panelData.SpikeRasterPlot();
        probeImage = panelData.ProbeImage();
        areaLabel = panelData.AreaPanel().GetComponent<TextMeshProUGUI>();

        InitializeSitePositions();
        InitializeSites();

        netObject = GetComponent<NetworkObject>();

        // Hide my probe from all other clients
        SetProbeVisibilityServerRpc();

        // Now go through the sites and for each one make a spikes trace plot
        for (int si = 0; si < sites.Length; si++)
        {
            LinkSiteToSpikePlot(si, sites[si], spikePanel);
        }
    }

    private void FixedUpdate()
    {
        // COLLISION CHECK
        // Note that this runs on the Fixed timestep, which we've set to be 10hz
        // this is quite slow! But it saves a lot of calls to the physics system
        inBrain = tipCollider.bounds.Intersects(rootCollider.bounds);
    }

    private void Update()
    {
        // MOVEMENT CONTROL
        if (!GetComponent<NetworkObject>().IsPlayerObject)
        {
            // TODO
            Debug.LogError("IsPlayerObject==false, this means that the activeElectrode workaround is no longer needed");
        }

        //if (IsLocalPlayer && GetComponent<NetworkObject>().IsPlayerObject)
        // activeElectrode gets set below, controlled by the PlayerManager script. This is a workaround while IsPlayerObject is not working
        if (IsLocalPlayer && activeElectrode)
        {
            bool moved = false;
            if (!inBrain)
            {
                float xRot = -Input.GetAxis("X-rotation");
                float zRot = Input.GetAxis("Z-rotation");
                float zInput = Input.GetAxis("Vertical");
                float xInput = Input.GetAxis("Horizontal");

                if ((xRot != 0) || (zRot != 0) || (zInput != 0) || (xInput != 0))
                {
                    moved = true;
                    transform.Rotate(Vector3.right * xRot * rotSpeed, Space.Self);
                    transform.Rotate(Vector3.forward * zRot * rotSpeed, Space.Self);
                    transform.Translate(Vector3.right * zInput * curSpeed * Time.deltaTime);
                    transform.Translate(Vector3.forward * xInput * curSpeed * Time.deltaTime);
                }
            }

            float yInput = Input.GetAxis("Depth"); // Z/X
            if (yInput != 0)
            {
                moved = true;
                transform.Translate(Vector3.up * yInput * curSpeed * Time.deltaTime);
            }

            if (moved)
            {
                ElectrodeMoved();
            }
        }
    }

    private void LinkSiteToSpikePlot(int count, GameObject site, GameObject spikePanel)
    {
        // Get the spike trace plot and link the electrode site
        SpikePlotRenderer spr = spikeTracePlot.GetComponent<SpikePlotRenderer>();
        spr.LinkElectrodeSite(site);
        ElectrodeSite es = site.GetComponent<ElectrodeSite>();
        es.LinkSpikePlotRenderer(spr);
        es.Enable();

        // At this point we also need to create a RenderTexture for the TipCamera and tie that 
        // to the RawImage in the SpikePlotPanel [TODO]
        RenderTexture tipCameraTexture = new RenderTexture(100, 512, 16, RenderTextureFormat.ARGB2101010);
        tipCamera.targetTexture = tipCameraTexture;
        // And finally set the RawImage to get this texture
        probeImage.GetComponent<RawImage>().texture = tipCameraTexture;
    }

    public void SetActive(bool active)
    {
        activeElectrode = active;
    }

    // ** ?? ** //

    public GameObject[] GetSites()
    {
        return sites;
    }

    [ServerRpc]
    private void SetProbeVisibilityServerRpc()
    {
        if (!IsServer) { return; }
        // Set this probe so that it is only visible to the client who created it and nobody else
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            Debug.Log("Object owner: " + netObject.OwnerClientId);
            if (clientId != netObject.OwnerClientId)
            {
                Debug.Log("Hiding from: " + clientId);
                if (netObject.IsNetworkVisibleTo(clientId))
                {
                    netObject.NetworkHide(clientId);
                }
            }
        }
    }

    private void InitializeSitePositions()
    {
        var dataset = Resources.Load<TextAsset>("Datasets/site_positions_test");
        var dataLines = dataset.text.Split('\n');
        int numSites = dataLines.Length;
        sitePositions = new Vector2[numSites];
        for (int i = 0; i < numSites; i++)
        {
            var pos = dataLines[i].Split(',');
            sitePositions[i] = new Vector2(float.Parse(pos[0]), float.Parse(pos[1]));
        }
    }

    private void InitializeSites()
    {
        sites = new GameObject[sitePositions.Length];
        for (int i = 0; i < sitePositions.Length; i++)
        {
            //Debug.Log(sitePositions[i]);
            //Vector3 sitePos = new Vector3(sitePositions[i].x, sitePositions[i].y + 10 + 4, -5.7f);
            // Note: (0f,14f,-5.7f) puts it right on the tip of the probe -- it should just be +10y, I'm not sure why it spawns the origin
            Vector3 sitePos = new Vector3(0f, 14f + sitePositions[i].y, -5.7f - sitePositions[i].x);
            Debug.Log("Adding site with SitePos: " + sitePos);
            GameObject site = Instantiate(electrodeSitePrefab, transform, false);
            site.transform.localPosition += sitePos;
            sites[i] = site;
        }
    }

    public void ElectrodeMoved()
    {

        if (!IsOwner && IsClient) { return; }

        // Anytime the electrode moves we need to call the C# server
        foreach (GameObject site in electrode.GetSites())
        {
            // This fairly inefficient
            // A better solution would be to find the min/max of all sites in x/y/z coordinates
            // and send that information to the server
            Vector3 sitePosition = emanager.ConvertBrainPositionToCCF(site.transform.position);
            apLabel.text = "AP: " + Mathf.Round(sitePosition.x * 100f) / 100f;
            dvLabel.text = "DV: " + Mathf.Round(sitePosition.y * 100f) / 100f;
            lrLabel.text = "LR: " + Mathf.Round(sitePosition.z * 100f) / 100f;
            int3 siteCoords = emanager.SitePosition2apdvlr(sitePosition);
            areaLabel.text = "Area: " + emanager.GetAnnotationAcronym(siteCoords.x, siteCoords.y, siteCoords.z);
            emanager.ElectrodeMovedCallback(OwnerClientId, siteCoords);
        }
    }

    public void OnDestroy()
    {
        Destroy(spikePanel);
        Destroy(spikeMinPanel);
    }
}
