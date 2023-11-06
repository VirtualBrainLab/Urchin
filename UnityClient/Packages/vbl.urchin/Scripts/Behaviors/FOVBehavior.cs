using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Control a Quad Mesh to display a Calcium Imaging "Field of View" texture
/// </summary>
public class FOVBehavior : MonoBehaviour
{

    #region Variables
    private int[] _order = { 3, 2, 0, 1 }; // the vertex order of a quad is bl, br, tl, tr, the input order is clockwise from tl
    private MeshFilter _meshFilter;
    private Renderer _renderer;
    private List<byte[]> _imageDataChunks;

    #endregion

    #region Internal
    private int nextChunk;
    private bool nextApply;
    private int texture_width;
    private int texture_height;
    private string texture_type;

    private Texture2D _activeTexture;
    private Vector3[] _verticesWorldU;
    private Vector3 _center;
    private Vector3 _offset;
    #endregion

    #region Unity
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _renderer = GetComponent<Renderer>();
    }
    #endregion

    #region Public Setters

    /// <summary>
    /// Set the position of the four vertex corners of the Quad
    /// </summary>
    /// <param name="verticesWorldU"></param>
    public void SetPosition(Vector3[] verticesWorldU)
    {
        _verticesWorldU = verticesWorldU;
        RecalculateCenter();
        SetPosition();
    }

    /// <summary>
    /// Set an offset to move the Quad up/down; positive is up.
    /// </summary>
    /// <param name="offset"></param>
    public void SetOffset(float offset)
    {
        _offset = Vector3.up * offset;
        SetPosition();
    }

    #endregion

    #region Helpers

    private void RecalculateCenter()
    {
        // Compute the center of the quad
        Vector3 center = new Vector3(0f, 0f, 0f);
        for (int i = 0; i < 4; i++)
            center += _verticesWorldU[i];
        center = center / 4f;
        _center = center;
    }

    private void SetPosition()
    {
        // Re-order vertices for Quad
        Vector3[] verticesReordered = new Vector3[4];
        for (int i = 0; i < 4; i++)
            verticesReordered[i] = _verticesWorldU[_order[i]] - _center;

        // Apply vertex positions
        _meshFilter.mesh.vertices = verticesReordered;

        transform.position = _center + _offset;
    }

    /// <summary>
    /// Initiatze data buffer for an incoming new texture
    /// </summary>
    /// <param name="totalChunks"></param>
    public void SetTextureInit(int totalChunks, int width, int height, string type)
    {
        Debug.Log("SetTextureInit");
        _activeTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        _activeTexture.wrapMode = TextureWrapMode.Clamp;
        _activeTexture.filterMode = FilterMode.Point;
        _renderer.material.SetTexture("_Texture2D", _activeTexture);

        texture_width = width;
        texture_height = height;
        texture_type = type;
        _imageDataChunks = new List<byte[]>(totalChunks);
        for (int i = 0; i < totalChunks; i++)
        {
            _imageDataChunks.Add(new byte[0]);
        }
    }

    /// <summary>
    /// Receive metadata about the next texture image chunk
    /// </summary>
    /// <param name="chunkid"></param>
    /// <param name="immediateApply"></param>
    public void SetTextureMeta(int chunkid, bool immediateApply)
    {
        Debug.Log($"SetTextureMeta,{chunkid} {immediateApply}");
        nextChunk = chunkid;
        nextApply = immediateApply;
    }

    /// <summary>
    /// Receive a chunk of texture data
    /// </summary>
    /// <param name="chunkid"></param>
    /// <param name="immediateApply"></param>
    /// <param name="imageBytes"></param>
    public void SetTextureData(byte[] imageBytes)
    {
        Debug.Log($"SetTextureData,{nextChunk}");
        // Put chunk in temporary array
        _imageDataChunks[nextChunk] = imageBytes;

        // Apply texture if all chunks are received
        if (nextApply)
        {
            // Load chunks of image into one array
            int size = _imageDataChunks.Sum(chunk => chunk.Length);
            byte[] _imageData = new byte[size];

            int offset = 0;
            foreach (byte[] chunk in _imageDataChunks)
            {
                foreach (byte b in chunk)
                {
                    _imageData[offset++] = b;
                }
            }
            // Load the image bytes into a new Texture2D
            //              if doesn't work: .SetPixel(). Was .LoadImage() before.
            offset = 0;
            // Load an array: each byte is a pixel
            if (texture_type == "array")
            {
                for (int y = 0; y < texture_width; y++)
                {
                    for (int x = 0; x < texture_height; x++)
                    {
                        float value = _imageData[offset++] / 255f;
                        Color c = new Color(value, value, value);
                        _activeTexture.SetPixel(x, y, c);
                    }
                }
            }
            // Load an image: every 3 bytes are a pixel in (R,G,B)
            else if (texture_type == "image")
            {
                _activeTexture.LoadImage(_imageData);
            }
            else
            {
                Debug.LogError($"Texture type must be 'image' or 'array', got {texture_type}");
            }

            // Refresh texture to GPU
            _activeTexture.Apply();

            // Release buffer pool and texture to free up memory
            _imageDataChunks.Clear();
        }
    }

    #endregion


}