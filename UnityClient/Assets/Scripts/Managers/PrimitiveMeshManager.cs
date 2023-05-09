using CoordinateSpaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimitiveMeshManager : MonoBehaviour
{
    //Keeping a dictionary mapping names of objects to the game object in schene
    private Dictionary<string, MeshRenderer> _primMeshRenderers; 
    //log warning if string id alr exists (containskey())
    //send warning back to python 
    //Client.logwarning("message");
    [SerializeField] private GameObject _cubePrefab;
    [SerializeField] private List<Material> _materials;
    [SerializeField] private List<string> _materialNames;
    private Dictionary<string, Material> _materialDictionary;

    private void Awake()
    {
        _primMeshRenderers= new Dictionary<string, MeshRenderer>();
        _materialDictionary = new Dictionary<string, Material>();
        if (_materials.Count != _materialNames.Count)
            throw new System.Exception("List of materials and material names must be the same length");
        for(int i = 0; i< _materials.Count; i++)
        {
            _materialDictionary.Add(_materialNames[i], _materials[i]);
        }

    }
    // Start is called before the first frame update
    void Start() 
    {
        
    }

    public void CreateMesh(List<string> meshes) //instantiates cube as default
    {
        foreach(string mesh in meshes)
        {
            if (_primMeshRenderers.ContainsKey(mesh))
                Client.LogWarning($"Mesh with id = {mesh} already exists.");

            GameObject tempObject = Instantiate(_cubePrefab);
            tempObject.name = $"primMesh_{mesh}";
            _primMeshRenderers.Add(mesh, tempObject.GetComponent<MeshRenderer>());
            tempObject.GetComponent<MeshRenderer>().material = _materialDictionary["default"];
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
            //if (_primMeshRenderers.ContainsKey(meshName))
                //Client.LogWarning($"Mesh with id = {meshName} does not exist.");

            List<float> data = meshPositions[meshName];
            Vector3 position = new Vector3 (data[0], data[1], data[2]);

            //CHECK IF KEY MESH NAME EXISTS (IF NOT SEND WARNING)
        
            MeshRenderer tempMesh = _primMeshRenderers[meshName];

            // Example of how a CoordinateSpace could be used to position this mesh
            // CCFSpace tempCoordinateSpace = new CCFSpace();
            // Example 1: User passed coordinates in CCF space
            // Vector3 positionWorld = tempCoordinateSpace.Space2World(position);

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

    public void SetMaterial(Dictionary<string, string> meshMaterials)
    { 
        foreach(var kvp in meshMaterials)
        {
            Material newMaterial = _materialDictionary[kvp.Value];
            MeshRenderer tempMesh = _primMeshRenderers[kvp.Key];
            Color color = tempMesh.material.color;
            tempMesh.material = newMaterial;
            tempMesh.material.color = color;

        }
    }

}