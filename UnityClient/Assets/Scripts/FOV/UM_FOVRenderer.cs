using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class UM_FOVRenderer : MonoBehaviour
{

    private Dictionary<string, List<byte[]>> imageDataChunks;
    private Dictionary<string, int> imageDataRemainingSize;
    private Dictionary<string, GameObject> fovs;
    private Dictionary<string, Vector3[]> fovPositions;
    private Dictionary<string, Vector3> fovOffsets;
    private Dictionary<string, Material> fovMaterials;
    private int[] order = { 3, 1, 2, 0 }; // for reordering vertices to match the mesh
                                          // vertex order given by user: start from top left, go clockwise
    [SerializeField] private Material material;
    private void Awake()
    {
        imageDataChunks = new Dictionary<string, List<byte[]>>();
    }

    private string nextFOV;
    private int nextChunk;
    private bool nextApply;

    // Texture2D fovTexture2d = Resources.Load<Texture2D>(folder + "/fov");

    public void SetTextureDataMetaInit(string name, int totalSize)
    {
        imageDataChunks[name] = new List<byte[]>(totalSize);
    }

    public void SetTextureDataMeta(string name, int chunk, bool immediateApply)
    {
        nextFOV = name;
        nextChunk = chunk;
        nextApply = immediateApply;
    }

    public void SetTextureData(byte[] imageBytes)
    {
        if (imageDataChunks.ContainsKey(nextFOV))
        imageDataChunks[nextFOV][nextChunk]=imageBytes;

        if (nextApply)
        {
            int size = imageDataChunks[nextFOV].Sum(chunk => chunk.Length);
            byte[] imageData = new byte[size];

            int offset = 0;
            foreach (byte[] chunk in imageDataChunks[nextFOV])
            {
                chunk.CopyTo(imageData, offset);
                offset += chunk.Length;
            }

            // Create a new Texture2D
            Texture2D fovTexture = new Texture2D(2, 2);

            // Load the image bytes into the Texture2D
            fovTexture.LoadImage(imageData);

            // Apply the texture to the material
            fovMaterials[nextFOV].SetTexture("_MeanImageTexture", fovTexture);

            // Remove data chunks pool for this FOV
            imageDataChunks.Remove(nextFOV);

            // Release the texture to free up memory
            Destroy(fovTexture);
        }
    }

    public void Create(string name)
    {
        if (!fovs.ContainsKey(name))
        {
            GameObject _fov = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _fov.GetComponent<Renderer>().material = material;
            _fov.transform.SetParent(transform); //set fov manager as parent
            fovs.Add(name, _fov);
            Debug.Log($"(UM_FOVRend) Creating new FOV: {name}");
        }
        else
        {
            Debug.Log($"UM_FOVRenderer.Create(): FOV {name} already exists in the scene");
        }
    }

    public void Delete(string name)
    {
        if (fovs.ContainsKey(name))
        {
            Destroy(fovs[name]);
            fovs.Remove(name);
        }
        else
        {
            Debug.Log($"UM_FOVRenderer.Delete(): FOV not found: {name}");
        }
    }

    public void SetPosition(string name, Vector3[] vertices)
    {
        if (fovs.ContainsKey(name))
        {
            Vector3 center = new Vector3(0f, 0f, 0f);
            for (int i = 0; i < vertices.Length; i++)
                center += vertices[i]; 
            center = center / 4f;
            Debug.Log("Setting center of quad " + name + "at: " + center);

            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = vertices[i] - center;
            Vector3[] verticesReordered = new Vector3[4];
            for (int i = 0; i < vertices.Length; i++)
                verticesReordered[i] = vertices[order[i]];
            fovs[name].GetComponent<MeshFilter>().mesh.vertices = verticesReordered;
            fovs[name].transform.position = center;
        }
        else
        {
            Debug.Log("UM_FOVRenderer.SetPosition(): FOV not found: " + name);
        }

    }

    // Apply an offset to the DV axis. Positive is upwards.
    public void SetOffset(string name, float offset)
    {
        if (fovs.ContainsKey(name))
        {
            Vector3 offsetVec = new Vector3(0f, offset, 0f);
            fovs[name].transform.position += offsetVec;
        }
        else
        {
            Debug.Log("UM_FOVRenderer.SetOffset(): FOV not found: "+name);
        }
    }

    public void SetVisibility(string name, bool visible)
    {
        if (fovs.ContainsKey(name))
        {
            fovs[name].GetComponent<Renderer>().enabled = visible;
        }
        else
        {
            Debug.Log("UM_FOVRenderer.SetVisibility(): FOV not found: " + name);
        }

    }


    // receive meta
    // count down data size
    // create 2d texture when all data are received
    // just for setting up the texture
}