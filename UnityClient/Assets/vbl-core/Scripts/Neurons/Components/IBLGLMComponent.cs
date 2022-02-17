using Unity.Entities;

public struct IBLGLMComponent : IComponentData
{
    public float rf_x;
    public float rf_y;
    public float rf_sig;
    public float stimOnL;
    public float stimOnR;
    public float correct;
    public float incorrect;
    public float wheel;
}
