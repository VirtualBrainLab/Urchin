using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

public struct IBLEventAverageComponent : IComponentData
{
    public FixedListFloat4096 spikeRate;
}