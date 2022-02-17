using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
 * Note: the perlin noise values are very very finicky; changing
 * the offsets or subtractions may cause plots to repeat.
 */
public class SpikePlotRenderer : MonoBehaviour {
    private Utils util;

    private LineRenderer lr;
    [SerializeField] private LineRenderer verticalLR;

    //private float spikeScale = 0.17f;
    //private float yScale = 15.0f;
    private float noiseScale = 0.6f;
    private int maxPos = 100;
    private float xScale;
    private float xOffset;

    private GameObject linkedSite;
    private bool linked;

    // Tracking
    private int pos;

    // Values we'll use when displaying a spike
    private int isSpiking;
    private int[] spikeAmplitude = { -25, 5, 2 };

    private Vector3[] vertLRpos;

    private void Start()
    {
        util = GameObject.Find("main").GetComponent<Utils>();

        linked = true;

        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = maxPos;

        vertLRpos = new Vector3[2];
        verticalLR.GetPositions(vertLRpos);

        isSpiking = -1;

        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {

        // wait for one frame to pass
        yield return null;

        RectTransform rt = GetComponent<RectTransform>();
        xScale = rt.rect.width / maxPos;
        xOffset = 0;
        // Set all data to zeros
        Vector3[] startPositions = new Vector3[maxPos];
        for (int i = 0; i < maxPos; i++)
        {
            startPositions[i] = Vector3.right * (i * xScale + xOffset);
        }
        lr.SetPositions(startPositions);
    }

    // Update is called once per frame
    void Update() 
    {
        if (linked)
        {

            //// Pull out our current position data
            //Vector3[] curPositions = new Vector3[lr.positionCount];
            //lr.GetPositions(curPositions);

            float yValue = 0;

            // Check if we are spiking
            if (isSpiking>=0)
            {
                if (isSpiking++ >= spikeAmplitude.Length)
                    isSpiking = -1;
                else
                    yValue += spikeAmplitude[isSpiking];
            }

            // Add noise
            yValue += util.GaussianNoise() * noiseScale;

            // Bring the index back to the front if we ran off the end
            if (++pos >= maxPos)
                pos -= maxPos;

            // Update our current position with whatever value we have right now
            lr.SetPosition(pos, new Vector3(pos * xScale + xOffset, yValue, 0f));

            // Set vertical line position
            for (int i = 0; i < 2; i++)
                vertLRpos[i].x = pos * xScale + xOffset;
            verticalLR.SetPositions(vertLRpos);
        }
        
    }

    public void Spike() 
    {
        isSpiking = 0;
    }

    public void LinkElectrodeSite(GameObject electrodeSite)
    {
        if (electrodeSite != null)
        {
            linkedSite = electrodeSite;
            linked = true;
        }
    }

    public void UnlinkElectrodeSite()
    {
        linkedSite = null;
        linked = false;
    }

    public bool IsLinked()
    {
        return linked;
    }
}
