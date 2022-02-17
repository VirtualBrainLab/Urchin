using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

public struct SimulatedNeuronComponent : IComponentData
{
    public float3 apdvlr;
}

public struct SimRFComponent : IComponentData
{
    public float rf_x;
    public float rf_y;
    public float rf_sigma;
}
//public struct SimWheelComponent : IComponentData
//{
//    public float wheel;
//}
//public struct SimAuditoryComponent : IComponentData
//{
//    public float aud;
//}
//public struct SimRewardComponent : IComponentData
//{
//    public float reward;
//}