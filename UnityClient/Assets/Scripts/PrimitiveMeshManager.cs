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
        Debug.Log("function got called!");

        foreach (string meshName in meshPositions.Keys)
        {// running through whole dictionary:
            
            Vector3 position = meshPositions[meshName];
            MeshRenderer tempMesh = _primMeshRenderers[meshName];
            
          
            //tempLine.positionCount = meshPositions[meshName].Count;
            //tempLine.SetPositions(posvectors);

            //NOT FINISHED
        }
    }

    public void SetScale() 
    { //TRANSFORM.LOCALSCALE
    
    }

    public void SetColor()
    {

    }

}
