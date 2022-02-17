using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikePanelData : MonoBehaviour
{
    [SerializeField] private Transform infoPanelAP;
    [SerializeField] private Transform infoPanelDV;
    [SerializeField] private Transform infoPanelLR;
    [SerializeField] private GameObject spikeTraceGO;
    [SerializeField] private GameObject spikeRasterGO;
    [SerializeField] private GameObject probeImageGO;
    [SerializeField] private Transform areaPanel;

    public Transform InfoPanelAP()
    {
        return infoPanelAP;
    }

    public Transform InfoPanelDV()
    {
        return infoPanelDV;
    }

    public Transform InfoPanelLR()
    {
        return infoPanelLR;
    }

    public GameObject SpikeTracePlot()
    {
        return spikeTraceGO;
    }

    public GameObject SpikeRasterPlot()
    {
        return spikeRasterGO;
    }

    public GameObject ProbeImage()
    {
        return probeImageGO;
    }

    public Transform AreaPanel()
    {
        return areaPanel;
    }
}
