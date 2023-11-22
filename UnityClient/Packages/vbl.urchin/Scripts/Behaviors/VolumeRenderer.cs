using BrainAtlas;
using System;
using UnityEngine;
using Urchin.Utils;
using System.IO;
using BestHTTP.Decompression.Zlib;

public class VolumeRenderer : MonoBehaviour
{
    public Texture3D _volumeTexture;

    [SerializeField] GameObject _coronalSliceGO;
    [SerializeField] GameObject _sagittalSliceGO;

    private Material _coronalMaterial;
    private Material _sagittalMaterial;
    private Vector3[] _coronalOrigWorldU;
    private Vector3[] _sagittalOrigWorldU;

    public Vector3 _slicePosition;

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

        _coronalMaterial = _coronalSliceGO.GetComponent<Renderer>().material;
        _sagittalMaterial = _coronalSliceGO.GetComponent<Renderer>().material;

        transform.localScale = BrainAtlasManager.ActiveReferenceAtlas.Dimensions;

        (width, height, depth) = BrainAtlasManager.ActiveReferenceAtlas.DimensionsIdx;

        _volumeTexture = new Texture3D(width, height, depth, TextureFormat.RGBA32, false);
        _volumeTexture.filterMode = FilterMode.Point;
        _volumeTexture.wrapMode = TextureWrapMode.Clamp;

        GetComponent<Renderer>().material.mainTexture = _volumeTexture;
        _coronalMaterial.SetTexture("_Volume", _volumeTexture);
        _sagittalMaterial.SetTexture("_Volume", _volumeTexture);
        _volumeTexture.Apply();

        // x = ml
        // y = dv
        // z = ap

        // (dims.y, dims.z, dims.x)

        // vertex order -x-y, +x-y, -x+y, +x+y

        Vector3 dims = BrainAtlasManager.ActiveReferenceAtlas.Dimensions / 2f;

        // -x+y, +x+y, +x-y, -x-y
        _coronalOrigWorldU = new Vector3[] {
                new Vector3(-dims.y, -dims.z, 0f),
                new Vector3(dims.y, -dims.z, 0f),
                new Vector3(-dims.y, dims.z, 0f),
                new Vector3(dims.y, dims.z, 0f)
            };

        // -z+y, +z+y, +z-y, -z-y
        _sagittalOrigWorldU = new Vector3[] {
                new Vector3(0f, -dims.z, -dims.x),
                new Vector3(0f, -dims.z, dims.x),
                new Vector3(0f, dims.z, -dims.x),
                new Vector3(0f, dims.z, dims.x)
            };
    }

    private void Update()
    {
        UpdateSlicePosition();
    }

    public void Apply()
    {
        using (MemoryStream compressedStream = new MemoryStream(_compressedData))
        using (MemoryStream decompressedStream = new MemoryStream())
        using (DeflateStream decompressor = new DeflateStream(compressedStream, CompressionMode.Decompress))
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

    /// <summary>
    /// Shift the position of the sagittal and coronal slices to match the tip of the active probe
    /// </summary>
    public void UpdateSlicePosition()
    {
        // vertex order -x-y, +x-y, -x+y, +x+y

        // compute the world vertex positions from the raw coordinates
        // then get the four corners, and warp these according to the active warp
        Vector3[] newCoronalVerts = new Vector3[4];
        Vector3[] newSagittalVerts = new Vector3[4];
        for (int i = 0; i < _coronalOrigWorldU.Length; i++)
        {
            newCoronalVerts[i] = BrainAtlasManager.WorldU2WorldT(new Vector3(_coronalOrigWorldU[i].x, _coronalOrigWorldU[i].y, _slicePosition.z), false);
            newSagittalVerts[i] = BrainAtlasManager.WorldU2WorldT(new Vector3(_slicePosition.x, _sagittalOrigWorldU[i].y, _sagittalOrigWorldU[i].z), false);
        }

        _coronalSliceGO.GetComponent<MeshFilter>().mesh.vertices = newCoronalVerts;
        _sagittalSliceGO.GetComponent<MeshFilter>().mesh.vertices = newSagittalVerts;

        // Use that coordinate to render the actual slice position
        Vector3 dims = BrainAtlasManager.ActiveReferenceAtlas.Dimensions;

        float apWorldmm = dims.x / 2f - _slicePosition.z;
        float mlWorldmm = dims.y / 2f + _slicePosition.x;

        _coronalMaterial.SetFloat("_SlicePosition", apWorldmm / dims.x);
        _sagittalMaterial.SetFloat("_SlicePosition", mlWorldmm / dims.y);

        UpdateVolumeSlicing();
    }

    private void UpdateVolumeSlicing()
    {
        Vector3 dims = BrainAtlasManager.ActiveReferenceAtlas.Dimensions;

        // camYBack means the camera is looking from the back

        // the clipping goes from -0.5 to 0.5 on each dimension
        if (camYBack)
            // if we're looking from the back, we want to show the brain in the front
            GetComponent<Renderer>().material.SetVector("APClip", new Vector2(-dims.x / 2f, _slicePosition.z));
        else
            GetComponent<Renderer>().material.SetVector("APClip", new Vector2(_slicePosition.z, dims.x / 2f));

        if (!camXLeft)
            // clip from mlPosition forward
            GetComponent<Renderer>().material.SetVector("MLClip", new Vector2(_slicePosition.x, dims.y / 2f));
        else
            GetComponent<Renderer>().material.SetVector("MLClip", new Vector2(-dims.y / 2f, _slicePosition.x));
    }

    private bool camXLeft;
    private bool camYBack;
    public void UpdateCameraPosition()
    {
        Vector3 camPosition = Camera.main.transform.position;
        bool changed = false;
        if (!camXLeft && camPosition.x < 0)
        {
            camXLeft = true;
            changed = true;
        }
        else if (camXLeft && camPosition.x > 0)
        {
            camXLeft = false;
            changed = true;
        }
        else if (!camYBack && camPosition.z < 0)
        {
            camYBack = true;
            changed = true;
        }
        else if (camYBack && camPosition.z > 0)
        {
            camYBack = false;
            changed = true;
        }
        if (changed)
            UpdateVolumeSlicing();
    }
}
