using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using Unity.Mathematics;

// Note that all systems that modify neuron color need this -- otherwise neuron color *could* 
// get reset on the same frame when the FPS is low
[UpdateAfter(typeof(NeuronSpikingSystem))]
public class IBLGLMSystem : SystemBase
{
    IBLTask iblTask;

    protected override void OnStartRunning()
    {
        iblTask = GameObject.Find("main").GetComponent<ExperimentManager>().GetIBLTask();
    }

    protected override void OnUpdate()
    {
        //NativeList<float4> colors = lerpColors;
        float deltaTime = Time.DeltaTime;
        // wheel = x, stimOn = y, feedback = z;
        float wheelVelocity = iblTask.GetWheelVelocity();
        float stimOn = iblTask.GetStimOnFrame();
        float feedback = iblTask.GetFeedback();

        float4 white = new float4(1f, 1f, 1f, 1f);
        float4 green = new float4(0.15f, 0.6f, 0f, 0.5f);

        // base rate 0.5 hz
        float baseFiringRate = 0f; // 0.02f * Time.DeltaTime;

        Entities
            .WithName("SpikingComponent")
            //.WithReadOnly(colors)
            .ForEach((ref MaterialColor color, ref SpikingComponent spikeComp, ref SpikingRandomComponent randComp, in IBLGLMComponent neuronData) =>
            {
                // start with baseline firing rates
                float neuronFiringRate = baseFiringRate;
                // add on the GLM factors that can drive firing
                neuronFiringRate += wheelVelocity * neuronData.wheel * deltaTime;
                neuronFiringRate += stimOn * -neuronData.stimOnL * deltaTime;
                neuronFiringRate += stimOn * neuronData.stimOnR * deltaTime;
                if (feedback > 0)
                    neuronFiringRate += neuronData.correct * deltaTime;
                if (feedback < 0)
                    neuronFiringRate += neuronData.incorrect * deltaTime;

                // check if a random value is lower than this
                if (randComp.rand.NextFloat(1f) < neuronFiringRate)
                {
                    spikeComp.spiking = 1f;
                    color.Value = white;
                }

            }).ScheduleParallel();
    }
}
