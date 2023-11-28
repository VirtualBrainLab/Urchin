using BrainAtlas;
using System;
using System.Collections.Generic;
using UnityEngine;
using Urchin.API;

namespace Urchin.Managers
{
   
    public class PrimitiveMeshManager : MonoBehaviour
    {
        [SerializeField] private List<Material> _materials;
        [SerializeField] private List<string> _materialNames;
        [SerializeField] private Transform _primitiveMeshParentT;
        [SerializeField] private GameObject _primitivePrefabGO;

        //Keeping a dictionary mapping names of objects to the game object in schene
        private Dictionary<string, MeshRenderer> _primMeshRenderers;
        private Dictionary<string, Material> _materialDictionary;

        private void Awake()
        {
            _primMeshRenderers = new Dictionary<string, MeshRenderer>();
            _materialDictionary = new Dictionary<string, Material>();
            if (_materials.Count != _materialNames.Count)
                throw new System.Exception("List of materials and material names must be the same length");
            for (int i = 0; i < _materials.Count; i++)
            {
                _materialDictionary.Add(_materialNames[i], _materials[i]);
            }

        }

        private void Start()
        {

            Client_SocketIO.CreateMesh += CreateMesh;
            Client_SocketIO.DeleteMesh += DeleteMesh;
            Client_SocketIO.SetPosition += SetPosition;
            Client_SocketIO.SetScale += SetScale;
            Client_SocketIO.SetColor += SetColor;
            Client_SocketIO.SetMaterial += SetMaterial;

            Client_SocketIO.ClearMeshes += Clear;
        }

        public void CreateMesh(List<string> meshIDs) //instantiates cube as default
        {
            foreach (string meshID in meshIDs)
            {
                if (_primMeshRenderers.ContainsKey(meshID))
                    Debug.LogWarning($"Mesh with id = {meshID} already exists.");

                GameObject tempObject = Instantiate(_primitivePrefabGO, _primitiveMeshParentT);
                tempObject.GetComponent<MeshBehavior>().SetID(meshID);
                tempObject.name = $"primMesh_{meshID}";
                _primMeshRenderers.Add(meshID, tempObject.GetComponent<MeshRenderer>());
                tempObject.GetComponent<MeshRenderer>().material = _materialDictionary["default"];
            }
        }

        public void Clear()
        {
            foreach (var kvp in _primMeshRenderers)
            {
                Destroy(kvp.Value.gameObject);
            }
            _primMeshRenderers.Clear();
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
            {
                List<float> data = meshPositions[meshName];
                Vector3 position = new Vector3(data[0], data[1], data[2]);

                MeshRenderer tempMesh = _primMeshRenderers[meshName];

                tempMesh.transform.localPosition = BrainAtlasManager.ActiveReferenceAtlas.Atlas2World(position);
            }
        }

        public void SetScale(Dictionary<string, List<float>> meshScale)
        {
            foreach (string meshName in meshScale.Keys)
            {
                List<float> data = meshScale[meshName];
                Vector3 scaleChange = new Vector3(data[0], data[1], data[2]);
                MeshRenderer tempMesh = _primMeshRenderers[meshName];
                tempMesh.transform.localScale = scaleChange;
            }
        }

        public void SetColor(Dictionary<string, string> meshColors)
        {
            foreach (string meshName in meshColors.Keys)
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
            foreach (var kvp in meshMaterials)
            {
                Material newMaterial = _materialDictionary[kvp.Value];
                MeshRenderer tempMesh = _primMeshRenderers[kvp.Key];
                Color color = tempMesh.material.color;
                tempMesh.material = newMaterial;
                tempMesh.material.color = color;

            }
        }

    }
}