using BrainAtlas;
using System;
using UnityEngine;
using Urchin.Utils;
using System.IO;
using BestHTTP.Decompression.Zlib;

public class VolumeRenderer : MonoBehaviour
{
    public Texture3D _volumeTexture;
    private byte[] _compressedData;
    private Color[] _colormap;

    private int width;
    private int height;
    private int depth;

    private void Awake()
    {
        _colormap = new Color[256];
        for (int i = 0; i < 255; i++)
            _colormap[i] = Color.black;
        _colormap[255] = new Color(0f, 0f, 0f, 0f);

        transform.localScale = BrainAtlasManager.ActiveReferenceAtlas.Dimensions;

        (width, height, depth) = BrainAtlasManager.ActiveReferenceAtlas.DimensionsIdx;

        _volumeTexture = new Texture3D(width, height, depth, TextureFormat.RGBA32, false);
        _volumeTexture.filterMode = FilterMode.Point;
        _volumeTexture.wrapMode = TextureWrapMode.Clamp;

        GetComponent<Renderer>().material.mainTexture = _volumeTexture;
        _volumeTexture.Apply();
    }

    public void Apply()
    {
        using (MemoryStream compressedStream = new MemoryStream(_compressedData))
        using (MemoryStream decompressedStream = new MemoryStream())
        using (DeflateStream decompressor = new DeflateStream(compressedStream, CompressionMode.Decompress, BestHTTP.Decompression.Zlib.CompressionLevel.BestCompression))
        {
            // Drop the zlib header, which is not read by .NET
            compressedStream.Seek(2, SeekOrigin.Begin);
            decompressor.CopyTo(decompressedStream);

            decompressedStream.Position = 0;

            using (BinaryReader reader = new BinaryReader(decompressedStream))
            for (int x = 0; x < width; x++)
                for (int z = 0; z < depth; z++)
                    for (int y = 0; y < height; y++)
                            _volumeTexture.SetPixel(x, y, z, _colormap[reader.ReadByte()]);
        }

        _volumeTexture.Apply();
        }

    public void SetColormap(string[] hexColors)
    {
        Debug.Log("(UM_VolRend) Creating new colormap for: " + name);
        for (int i = 0; i < 255; i++)
        {
            if (i < hexColors.Length)
                _colormap[i] = Utils.ParseHexColor(hexColors[i]);
            else
                _colormap[i] = Color.black;
        }
    }

    public void SetVisibility(bool visible)
    {
        Debug.Log("(UM_VolRend) Volume: " + name + " is now visible: " + visible);
        GetComponent<Renderer>().enabled = visible;
    }

    private int _nextOffset;

    public void SetMetadata(int nCompressedBytes)
    {
        _compressedData = new byte[nCompressedBytes];
        _nextOffset = 0;

        Debug.Log($"Waiting to receive {_compressedData.Length} bytes");
    }

    public void SetData(byte[] newData)
    {
        Debug.Log($"Received {newData.Length} bytes putting in offset {_nextOffset}");
        Buffer.BlockCopy(newData, 0, _compressedData, _nextOffset, newData.Length);
        _nextOffset += newData.Length;

        if (_nextOffset == _compressedData.Length)
            Apply();
    }
}
