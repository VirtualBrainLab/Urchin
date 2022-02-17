using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningVideoAnimator : MonoBehaviour
{
    [SerializeField] private CCFModelControl mcontrol;
    [SerializeField] private ExperimentManager expmanager;
    [SerializeField] private NeuronEntityManager nemanager;

    // Start is called before the first frame update
    void Start()
    {
        mcontrol.LateStart(true);
        expmanager.ChangeExperiment(0);
        nemanager.RemoveAllNeurons();
        Invoke("DelayedExperimentPause", 7);
        Invoke("DelayedExperimentStart", 17);
        Invoke("DelayedExperimentPause", 31);
    }

    private void DelayedExperimentPause()
    {
        expmanager.ChangeExperiment(-1);
    }

    private void DelayedExperimentStart()
    {
        expmanager.ChangeExperiment(0);
        nemanager.RemoveAllNeurons();
    }
}
