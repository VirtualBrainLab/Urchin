using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public GameObject spikePanelPrefab;
    public GameObject spikeMinPanelPrefab;

    public bool debugModeEnabled;
    private float lastInterval = 0;
    private float debugFPSInterval = 1.0f;
    private int frames = 0;

    private Text UILabel;
    private Text SpikeRate;
    private Slider fpsSliderTest;

    // Start is called before the first frame update
    void Start()
    {
        if (debugModeEnabled)
        {
            UILabel = GameObject.Find("FPS").GetComponent<Text>();
        } else
        {
            GameObject.Find("FPS").SetActive(false);
        }

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("MinPanels"))
        {
            obj.SetActive(false);
        }

        // Set the default state for some toggles that are already visible
        //GameObject.Find("ViewerMinPanel").GetComponentInChildren<CustomToggle>().SetSelected(true);
        //GameObject.Find("StimMinPanel").GetComponentInChildren<CustomToggle>().SetSelected(true);
    }

    private void Update()
    {
        if (debugModeEnabled)
        {
            // FPS Debug mode code
            float timeNow = Time.realtimeSinceStartup;
            frames++;
            if (timeNow > (lastInterval + debugFPSInterval))
            {
                UILabel.text = "FPS: " + Mathf.RoundToInt(frames / (timeNow - lastInterval));
                frames = 0;
                lastInterval = timeNow;
            }
        }
    }

    public GameObject[] AddNewProbePanels(GameObject probe)
    {
        GameObject spikePanel = Instantiate(spikePanelPrefab, GameObject.Find("FullPanels").transform);
        GameObject spikeMinPanel = Instantiate(spikeMinPanelPrefab, GameObject.Find("SpikeMinPanels").transform);
        // Tell the min panel which main panel will get turned on/off
        CustomToggle minPanelToggle = spikeMinPanel.GetComponentInChildren<CustomToggle>();
        minPanelToggle.SetPanel(spikePanel);
        minPanelToggle.SetSelected(true);

        return new GameObject[] { spikePanel, spikeMinPanel };
    }
}
