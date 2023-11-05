using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Control a Quad Mesh to display a Calcium Imaging "Field of View" texture
/// </summary>
public class FOVBehavior : MonoBehaviour
{

    #region Variables
    private int[] _order = { 3, 1, 2, 0 }; // for reordering vertices to match the mesh
                                           // vertex order given by user: start from top left, go clockwise
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
    #endregion

    #region Unity
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _renderer = GetComponent<Renderer>();
    }
    #endregion

    #region Properties
    /// <summary>
    /// Convert CCF coordinates to Unity world coordinates
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    static Vector3 CCFtoWorld(Vector3 vec)
    {
        Vector3 outVec = Vector3.zero;
        outVec.x = -vec.x + 5.7f; //ML=-x
        outVec.y = -vec.z + 4f; //DV=-y
        outVec.z = vec.y - 6.6f; //AP=z
        return outVec;
    }

    /// <summary>
    /// Set the position of the four vertex corners of the Quad
    /// </summary>
    /// <param name="vertices"></param>
    public void SetPosition(List<Vector3> vertices)
    {

        for (int i = 0; i < 4; i++)
        {
            vertices[i] = CCFtoWorld(vertices[i]);
        }
        // Compute the center of the quad
        Vector3 center = new Vector3(0f, 0f, 0f);
        for (int i = 0; i < 4; i++)
            center += vertices[i];
        center = center / 4f;
        Debug.Log("SetPosition: Setting center of quad " + name + " at: " + center);

        // Re-order vertices for MeshFilter
        for (int i = 0; i < 4; i++)
            vertices[i] = vertices[i] - center;
        Vector3[] verticesReordered = new Vector3[4];
        for (int i = 0; i < 4; i++)
            verticesReordered[i] = vertices[_order[i]];

        // Apply vertex positions
        _meshFilter.mesh.vertices = verticesReordered;
        transform.position = center;
    }

    /// <summary>
    /// Set an offset to move the Quad up/down; positive is up.
    /// </summary>
    /// <param name="offset"></param>
    // Apply an offset to the DV axis. Positive is upwards.
    public void SetOffset(float offset)
    {
        transform.position += new Vector3(0f, offset, 0f);
        Debug.Log("SetOffset: Setting center of quad " + name + " at: " + transform.position);
    }

    /// <summary>
    /// Set an image texture for the Quad
    /// </summary>
    /// <param name="texture"></param>
    public void SetTexture(Texture2D texture)
    {
        _renderer.material.SetTexture("_MeanImageTexture", texture);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Initiatze data buffer for an incoming new texture
    /// </summary>
    /// <param name="totalChunks"></param>
    public void SetTextureInit(int totalChunks, int width, int height, string type)
    {
        Debug.Log("SetTextureInit");
        Debug.Log(totalChunks);
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
        //Debug.Log($"SetTextureMeta,{chunkid} {immediateApply}");
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
        //Debug.Log($"SetTextureData,{nextChunk}");
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
            Texture2D fovTexture = new Texture2D(texture_width, texture_height);
            // Apply the texture to the material
            SetTexture(fovTexture);
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
                        fovTexture.SetPixel(x, y, c);
                    }
                }
            }
            // Load an image: every 3 bytes are a pixel in (R,G,B)
            else if (texture_type == "image")
            {
                fovTexture.LoadImage(_imageData);
            }
            else
            {
                Debug.LogError($"Texture type must be 'image' or 'array', got {texture_type}");
            }
            Debug.Log(fovTexture);

            // Refresh texture to GPU
            fovTexture.Apply();

            // Release buffer pool and texture to free up memory
            _imageDataChunks.Clear();
        }
    }

    #endregion


}