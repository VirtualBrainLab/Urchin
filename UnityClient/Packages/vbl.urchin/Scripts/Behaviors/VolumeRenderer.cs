using BrainAtlas;
using System;
using UnityEngine;
using Urchin.Utils;
using System.IO;
using BestHTTP.Decompression.Zlib;

public class VolumeRenderer : MonoBehaviour
{
    public Texture3D _volumeTexture;

    [SerializeField] GameObject _cubeGO;
    [SerializeField] Transform _sliceParentT;
    [SerializeField] GameObject _coronalSliceGO;
    [SerializeField] GameObject _sagittalSliceGO;

    private Material _coronalMaterial;
    private Material _sagittalMaterial;
    private Vector3[] _coronalOrigWorldU;
    private Vector3[] _sagittalOrigWorldU;
    private bool _slicesActive;

    private Vector3 _prevSlicePosition;
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
        _sagittalMaterial = _sagittalSliceGO.GetComponent<Renderer>().material;

        Vector3 dims = BrainAtlasManager.ActiveReferenceAtlas.Dimensions;
        Vector3 dimsWorld = BrainAtlasManager.ActiveReferenceAtlas.Atlas2World_Vector(dims);
        _sliceParentT.localScale = new Vector3(Mathf.Abs(dimsWorld.x), Mathf.Abs(dimsWorld.y), Mathf.Abs(dimsWorld.z));
        _cubeGO.transform.localScale = dims;

        (width, height, depth) = BrainAtlasManager.ActiveReferenceAtlas.DimensionsIdx;

        _volumeTexture = new Texture3D(width, height, depth, TextureFormat.RGBA32, false);
        _volumeTexture.filterMode = FilterMode.Point;
        _volumeTexture.wrapMode = TextureWrapMode.Clamp;

        _cubeGO.GetComponent<Renderer>().material.mainTexture = _volumeTexture;
        _coronalMaterial.SetTexture("_Volume", _volumeTexture);
        _sagittalMaterial.SetTexture("_Volume", _volumeTexture);
        _volumeTexture.Apply();

        // x = ml
        // y = dv
        // z = ap

        // (dims.y, dims.z, dims.x)

        // vertex order -x-y, +x-y, -x+y, +x+y

        // -x+y, +x+y, +x-y, -x-y
        _sagittalOrigWorldU = new Vector3[] {
                new Vector3(0f, -0.5f, -0.5f),
                new Vector3(0f, -0.5f, 0.5f),
                new Vector3(0f, 0.5f, -0.5f),
                new Vector3(0f, 0.5f, 0.5f)
            };

        // -z+y, +z+y, +z-y, -z-y
        _coronalOrigWorldU = new Vector3[] {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f)
            };
    }

    private void Start()
    {
        SetSliceVisibility(true);
    }

    private void Update()
    {
        if (_slicePosition != _prevSlicePosition)
        {
            _prevSlicePosition = _slicePosition;
            UpdateSlicePosition();
        }
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

    public void SetVolumeVisibility(bool visible)
    {
        Debug.Log("(UM_VolRend) Volume: " + name + " is now visible: " + visible);
        _cubeGO.GetComponent<Renderer>().enabled = visible;
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

    public void SetSliceVisibility(bool visible)
    {
        _slicesActive = visible;

        _coronalSliceGO.SetActive(_slicesActive);
        _sagittalSliceGO.SetActive(_slicesActive);

        UpdateVolumeSlicing();
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
            newCoronalVerts[i] = new Vector3(_coronalOrigWorldU[i].x, _coronalOrigWorldU[i].y, -_slicePosition.x);
            newSagittalVerts[i] = new Vector3(_slicePosition.z, _sagittalOrigWorldU[i].y, _sagittalOrigWorldU[i].z);
        }

        _coronalSliceGO.GetComponent<MeshFilter>().mesh.vertices = newCoronalVerts;
        _sagittalSliceGO.GetComponent<MeshFilter>().mesh.vertices = newSagittalVerts;

        _coronalMaterial.SetFloat("_SlicePercentage", 0.5f - _slicePosition.x);
        _sagittalMaterial.SetFloat("_SlicePercentage", _slicePosition.z + 0.5f);

        UpdateVolumeSlicing();
    }

    private void UpdateVolumeSlicing()
    {
        float min = -0.5f;
        float max = 0.5f;
        if (_slicesActive)
        {

            // camYBack means the camera is looking from the back

            // the clipping goes from -0.5 to 0.5 on each dimension
            // _slicePosition goes from -0.5 to 0.5 on each dimension
            if (camYBack)
                // if we're looking from the back, we want to show the brain in the front
                _cubeGO.GetComponent<Renderer>().material.SetVector("_APClip", new Vector2(min, _slicePosition.x));
            else
                _cubeGO.GetComponent<Renderer>().material.SetVector("_APClip", new Vector2(_slicePosition.x, max));

            if (camXLeft)
                // clip from mlPosition forward
                _cubeGO.GetComponent<Renderer>().material.SetVector("_MLClip", new Vector2(_slicePosition.z, max));
            else
                _cubeGO.GetComponent<Renderer>().material.SetVector("_MLClip", new Vector2(min, _slicePosition.z));
        }
        else
        {
            _cubeGO.GetComponent<Renderer>().material.SetVector("_APClip", new Vector2(min, max));
            _cubeGO.GetComponent<Renderer>().material.SetVector("_MLClip", new Vector2(min, max));
        }
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
