using Unity.Entities;
using Unity.Mathematics;
public struct SpikingComponent : IComponentData
{
    public float spiking;
}

public struct SpikingColorComponent : IComponentData
{
    public float4 color;
}

public struct SpikingRandomComponent : IComponentData
{
    public Random rand;
}