using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class PA_NeuronVisibilitySystem : SystemBase
{
    private NeuronEntityManager nemanager;
    private PA_Launch palaunch;
    protected override void OnStartRunning()
    {
        nemanager = GameObject.Find("main").GetComponent<NeuronEntityManager>();
        palaunch = GameObject.Find("main").GetComponent<PA_Launch>();

    }

    protected override void OnUpdate()
    {
        int[] labOrder = { 0, 6, 4, 2, 5, 3, 8, 7, 1, 9 };
        float smallScale = nemanager.GetNeuronScale();
        int clab = palaunch.GetCurrentLab();
        NativeArray<int> prev = palaunch.GetPreviousLabs().ToNativeArray<int>(Allocator.TempJob);
        float4 gray = new float4(0.5f, 0.5f, 0.5f, 1.0f);

        Entities
            .ForEach((ref Scale scale, ref MaterialColor color, in PAComponent labComp) =>
            {
                // If the labComp.lab is equal to the current visible lab, make this neuron big
                if (labComp.lab == clab)
                    scale.Value = 0.05f;
                else if (prev.Contains(labComp.lab))
                {
                    scale.Value = 0.04f;
                    color.Value = gray;
                }
                else
                    scale.Value = 0.005f;

            })
            .WithDisposeOnCompletion(prev)
            .ScheduleParallel();
    }
}