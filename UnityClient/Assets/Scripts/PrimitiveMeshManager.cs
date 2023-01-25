using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimitiveMeshManager : MonoBehaviour
{
    //Keeping a dictionary mapping names of objects to the game object in schene
    private Dictionary<string, MeshRenderer> _PrimMeshRenderers;
    [SerializeField] private GameObject _PrimMeshManagerPrefab;

    private void Awake()
    {
        _PrimMeshManagerPrefab = new();
    }
    // Start is called before the first frame update
    void Start() 
    {
        
    }

    public void CreateMesh(List<string> meshes) //instantiates cube as default
    {
        foreach(string mesh in meshes)
        {
            GameObject tempObject = Instantiate(_PrimMeshManagerPrefab);
            tempObject.name = $"primMesh_{mesh}";
            _PrimMeshRenderers.Add(mesh, tempObject.GetComponent<MeshRenderer>());
            //creates new entry to dictionary w name meshes[mesh] and associates it w a new Game Object (cube as of 1/25/23)
            //adds mesh renderer componenet to the mesh renderer manager 

        }
    }

    public void DeleteMesh()
    {
        
    }

    public void SetPosition()
    {
        
    }

    public void SetScale() 
    { //TRANSFORM.LOCALSCALE
    
    }

    public void SetColor()
    {

    }

}
