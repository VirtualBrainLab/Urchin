using System;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;


// Note that all systems that modify neuron color need this -- otherwise neuron color *could*
// get reset on the same frame when the FPS is low
[UpdateAfter(typeof(NeuronSpikingSystem))]
public class IBLEventAverageSystem : SystemBase
{
    IBLTask iblTask;
    private float trialTimeIndex;
    private NeuronEntityManager nemanager;

    protected override void OnStartRunning()
    {
        trialTimeIndex = 0;
        iblTask = GameObject.Find("main").GetComponent<ExperimentManager>().GetIBLTask();
        nemanager = GameObject.Find("main").GetComponent<NeuronEntityManager>();
    }

    protected override void OnUpdate()
    {

        trialTimeIndex += 0.1f;
        float deltaTime = Time.DeltaTime;
        double curTime = Time.ElapsedTime;
        bool corr = iblTask.GetCorrect();
        int curIndex = (int) trialTimeIndex;
        float smallScale = nemanager.GetNeuronScale();

        int trialStartIdx;
        if (iblTask.GetSide() == -1)
        {
            trialStartIdx = corr ? 0 : 250;
        }
        else
        {
            trialStartIdx = corr ? 500 : 750;
        }

        //int trialTimeIdx = trialStartIdx + iblTask.GetTimeIndex();

        float4 white = new float4(1f, 1f, 1f, 1f);

        Entities
            .ForEach((ref Scale scale, ref MaterialColor color, ref SpikingComponent spikeComp, ref SpikingRandomComponent randComp, in IBLEventAverageComponent eventAverage) =>
            {
                float neuronFiringRate = eventAverage.spikeRate.ElementAt(curIndex) * deltaTime;

                // check if a random value is lower than this
                if (randComp.rand.NextFloat(1f) < neuronFiringRate)
                {
                    spikeComp.spiking = 1f;
                    color.Value = white;
                    scale.Value = 1f;
                }
            }).ScheduleParallel();
    }
}
