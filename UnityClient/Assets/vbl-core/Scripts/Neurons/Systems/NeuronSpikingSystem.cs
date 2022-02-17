using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class NeuronSpikingSystem : SystemBase
{
    private NeuronEntityManager nemanager;

    protected override void OnStartRunning()
    {
        nemanager = GameObject.Find("main").GetComponent<NeuronEntityManager>();
    }

    protected override void OnUpdate()
    {
        float smallScale = nemanager.GetNeuronScale();
        float spikeTimeChange = Time.DeltaTime * nemanager.GetMaxFiringRate();

        Entities
            .ForEach((ref Scale scale, ref MaterialColor color, ref SpikingComponent spikeComp, in SpikingColorComponent spikeColorComp) =>
            {
                if (spikeComp.spiking > 0f)
                {
                    // check whether we are finished spiking
                    spikeComp.spiking -= spikeTimeChange;
                    if (spikeComp.spiking < 0f)
                    {
                        color.Value = spikeColorComp.color;
                        scale.Value = smallScale;
                    }
                    else
                    {
                        // reduce the scale
                        if (scale.Value > smallScale)
                            scale.Value *= 0.9f;
                        else
                            scale.Value = smallScale;
                    }
                }
            }).ScheduleParallel();
    }
}