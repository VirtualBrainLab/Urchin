using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PrimitiveMeshManager : MonoBehaviour
{
    //Keeping a dictionary mapping names of objects to the game object in schene
    private Dictionary<string, MeshRenderer> _primMeshRenderers;
    [SerializeField] private GameObject _primMeshManagerPrefab;

    private void Awake()
    {
        _primMeshManagerPrefab = new();
    }
    // Start is called before the first frame update
    void Start() 
    {
        
    }

    public void CreateMesh(List<string> meshes) //instantiates cube as default
    {
        foreach(string mesh in meshes)
        {
            GameObject tempObject = Instantiate(_primMeshManagerPrefab);
            tempObject.name = $"primMesh_{mesh}";
            _primMeshRenderers.Add(mesh, tempObject.GetComponent<MeshRenderer>());
            //creates new entry to dictionary w name meshes[mesh] and associates it w a new Game Object (cube as of 1/25/23)
            //adds mesh renderer componenet to the mesh renderer manager 

        }
    }

    public void DeleteMesh(List<string> meshes)
    {
        foreach (string mesh in meshes)
        {
            Destroy(_primMeshRenderers[mesh].gameObject);
            _primMeshRenderers.Remove(mesh);

        }
    }

    public void SetPosition(Dictionary<string, List<float>> meshPositions)
    {
        foreach (string meshName in meshPositions.Keys)
        {// running through whole dictionary:
            List<float> data = meshPositions[meshName];
            Vector3 position = new Vector3 (data[0], data[1], data[2]);
            MeshRenderer tempMesh = _primMeshRenderers[meshName];
            tempMesh.transform.position = position;
        }
    }

    public void SetScale(Dictionary<string, List<float>> meshScale) 
    { 
        foreach( string meshName in meshScale.Keys)
        {
            List<float> data = meshScale[meshName];
            Vector3 scaleChange = new Vector3(data[0], data[1], data[2]);
            MeshRenderer tempMesh = _primMeshRenderers[meshName];
            tempMesh.transform.localScale = scaleChange;
        }
    }

    public void SetColor(Dictionary<string,string> meshColors)
    {
        foreach(string meshName in meshColors.Keys)
        {
            Color newColor;
            if (ColorUtility.TryParseHtmlString(meshColors[meshName], out newColor))
            {
                //Debug.Log($"Setting Color of {meshName} to {meshColors[meshName]}");
                SetColor(meshName, newColor);
            }
            else
            {
                //Debug.Log($"Color {meshColors[meshName]} does not exist.");
            }
        }
    }

    private void SetColor(string meshName, Color color)
    {
        MeshRenderer tempMesh = _primMeshRenderers[meshName];
        tempMesh.material.color = color;
    }

}
