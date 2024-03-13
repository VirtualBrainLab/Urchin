using BrainAtlas;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Urchin.API;

namespace Urchin.Managers
{
   
    public class PrimitiveMeshManager : MonoBehaviour
    {
        [SerializeField] private Transform _primitiveMeshParentT;
        [SerializeField] private GameObject _primitivePrefabGO;

        //Keeping a dictionary mapping names of objects to the game object in schene
        private Dictionary<string, MeshBehavior> _meshBehaviors;

        private void Awake()
        {
            _meshBehaviors = new();
        }

        private void Start()
        {
            // singular
            Client_SocketIO.MeshUpdate += UpdateData;

            // plural
            Client_SocketIO.MeshDelete += Delete;
            Client_SocketIO.MeshDeletes += DeleteList;

            Client_SocketIO.MeshSetPositions += SetPositions;
            Client_SocketIO.MeshSetScales += SetScales;
            Client_SocketIO.MeshSetColors += SetColors;
            Client_SocketIO.MeshSetMaterials += SetMaterials;

            // clear
            Client_SocketIO.ClearMeshes += Clear;
        }

        public void UpdateData(MeshModel data)
        {
            if (_meshBehaviors.ContainsKey(data.id))
            {
                // Update
                _meshBehaviors[data.id].Data = data;
                _meshBehaviors[data.id].UpdateAll();
            }
            else
            {
                // Create
                Create(data);
            }
        }

        public void Create(MeshModel data) //instantiates cube as default
        {
            GameObject go = Instantiate(_primitivePrefabGO, _primitiveMeshParentT);
            go.name = $"primMesh_{data.id}";

            MeshBehavior meshBehavior = go.GetComponent<MeshBehavior>();

            meshBehavior.Data = data;
            meshBehavior.UpdateAll();

            _meshBehaviors.Add(data.id, meshBehavior);
        }

        public void Clear()
        {
            foreach (var kvp in _meshBehaviors)
            {
                Destroy(kvp.Value.gameObject);
            }
            _meshBehaviors.Clear();
        }

        #region Delete
        public void Delete(IDData data)
        {
            _Delete(data.id);
        }

        public void DeleteList(IDList data)
        {
            foreach (string id in data.ids)
                _Delete(id);
        }

        private void _Delete(string id)
        {
            if (_meshBehaviors.ContainsKey(id))
            {
                Destroy(_meshBehaviors[id].gameObject);
                _meshBehaviors.Remove(id);
            }
            else
                Debug.LogError($"Mesh {id} does not exist, can't delete");
        }
        #endregion

        #region Plural setters
        public void SetPositions(IDListVector3List data)
        {
            for (int i = 0; i < data.ids.Length; i++)
                _meshBehaviors[data.ids[i]].SetPosition(data.values[i]);
        }

        public void SetScales(IDListVector3List data)
        {
            for (int i = 0; i < data.ids.Length; i++)
                _meshBehaviors[data.ids[i]].SetScale(data.values[i]);
        }

        public void SetColors(IDListColorList data)
        {
            for (int i = 0; i < data.ids.Length; i++)
                _meshBehaviors[data.ids[i]].SetColor(data.values[i]);
        }

        public void SetMaterials(IDListStringList data)
        {
            for (int i = 0; i < data.ids.Length; i++)
                _meshBehaviors[data.ids[i]].SetMaterial(data.values[i]);
        }
        #endregion
    }
}