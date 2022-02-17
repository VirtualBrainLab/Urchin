using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Electrode sites 
 */
public class ElectrodeSite : MonoBehaviour
{
    private Dictionary<GameObject,float> nearbyNeurons;
    private ElectrodeManager emanager;
    private bool active;
    private float[] voltageHistory;

    public GameObject spikePlot;
    private SpikePlotRenderer spikePlotRenderer;

    const int historyLength = 500;
    const float spikeSoundThreshold = 0.85f;

    // Note: we use Awake() here because Start() only gets called on the first call to Update()
    // So this means that it might not run before we need the info here
    void Awake()
    {
        emanager = GameObject.Find("BrainDataManager").GetComponent<ElectrodeManager>();

        nearbyNeurons = new Dictionary<GameObject, float>();
        voltageHistory = new float[historyLength];
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            ComputeVoltage();
        }
    }

    public void FindNearbyNeurons()
    {
        //Debug.Log("Finding neurons");
        // We don't run this in Update() because it only needs to fire when the electrode
        // gets moved 

        // get the max distance, note that this is in ap coordinates, so to convert back /40?
        //float maxDistance = GameObject.Find("ElectrodeManager").GetComponent<ElectrodeManager>().neuronMaxRange; // / 40f;

        // Clear the list
        nearbyNeurons.Clear();
        // Go through all Neurons find those that are close to this site
        foreach (GameObject neuron in GameObject.FindGameObjectsWithTag("Neuron"))
        {
            Transform neuronTransform = neuron.transform;
            float distance = Vector3.Distance(transform.position, neuronTransform.position);

            // [TODO] I think this code doesn't work properly!
            // Replace 1.0f with maxDistance when it's working correctly
            if (distance < 1.0f)
            {
                // Neuron is in range of this electrode site, store it's gameObject here
                float amplitude = ComputeNeuronAmplitude(distance);
                nearbyNeurons.Add(neuronTransform.gameObject, amplitude);

                // [TODO] ElectrodeSite broken by ECS!!

                // lets also change the Material alpha according to distance
                // [TODO] When there are multiple electrode sites this will cause an issue since the last value
                // will get used, better would be to take the maximum value?
                // To do that it would need to be reset to 0f before calling FindNearbyNeurons (in EM)
                //neuron.GetComponent<Neuron>().SetAlpha(1.0f - 3 * distance);
            }
        }
    }

    public void ComputeVoltage()
    {
        //float voltage = 0;
        
        foreach (GameObject neuron in nearbyNeurons.Keys)
        {
            // TODO: Electrodesite broken by ECS!!
            //if (neuron.GetComponent<Neuron>().SpikedThisFramed())
            //{
            //    voltage += nearbyNeurons[neuron];
            //}
        }
        // TODO: re-enable!!
        //if (voltage > spikeSoundThreshold)
        //{
        //    emanager.Spike();
        //    spikePlotRenderer.Spike();
        //}

        //if (voltage > 0)
        //{
        //    Debug.Log(gameObject.GetInstanceID() + ": " + voltage);
        //}
    } 

    /*
     * Takes a distance in um and returns the neuron amplitude
     */
    private float ComputeNeuronAmplitude(float distance)
    {
        //Debug.Log(distance * 1000f);
        //Debug.Log(Mathf.Pow(0.99f, distance * 1000f));
        //return Mathf.Pow(0.99f, distance*1000f);
        float amp = 3.1421f / (0.3888f * Mathf.Pow(1.0854f, distance*1000f) + 2.3298f);
        return amp;
    }

    public void Enable()
    {
        active = true;
    }

    public void Disable()
    {
        active = false;
    }

    public void LinkSpikePlotRenderer(SpikePlotRenderer spikePlotRenderer)
    {
        this.spikePlotRenderer = spikePlotRenderer;
    }
}
