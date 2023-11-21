public struct VolumeMeta
{
    public string name;
    public int nCompressedBytes;
    public bool visible;
    public string[] colormap;
}

public struct VolumeDataChunk
{
    public string name;
    public int offset;
    public byte[] compressedByteChunk;
}