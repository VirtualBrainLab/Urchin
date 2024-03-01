using BrainAtlas;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using Urchin.API;

namespace Urchin.Managers
{
    public class CustomMeshManager : MonoBehaviour
    {
        #region Serialized
        [SerializeField] private Transform _customMeshParentT;
        #endregion

        #region Private
        private Dictionary<string, GameObject> _customMeshGOs;
        private BlenderSpace _blenderSpace;
        #endregion

        private void Start()
        {
            _customMeshGOs = new();

            _blenderSpace = new();

            Client_SocketIO.CustomMeshCreate += Create;
            Client_SocketIO.CustomMeshDestroy += Destroy;
            Client_SocketIO.CustomMeshPosition += SetPosition;

            Client_SocketIO.ClearCustomMeshes += Clear;
        }

        public void Create(CustomMeshData data)
        {
            GameObject go = new GameObject(data.ID);
            go.transform.SetParent(_customMeshParentT);

            Mesh mesh = new Mesh();

            // the vertices are assumed to have been passed in ap/ml/dv directions
            mesh.vertices = data.vertices.Select(x => _blenderSpace.Space2World_Vector(x)).ToArray();

            mesh.triangles = data.triangles;

            if (data.normals != null)
                mesh.normals = data.normals;

            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().material = MaterialManager.MeshMaterials["opaque-lit"];
            go.GetComponent<MeshRenderer>().material.color = Color.gray;

            _customMeshGOs.Add(data.ID, go);
        }

        public void Destroy(CustomMeshDestroy data)
        {
            if (_customMeshGOs.ContainsKey(data.ID))
            {
                Destroy(_customMeshGOs[data.ID]);
                _customMeshGOs.Remove(data.ID);
            }
            else
                Client_SocketIO.LogWarning($"Custom mesh {data.ID} does not exist in the scene, cannot destroy");

        }

        public void SetPosition(CustomMeshPosition data)
        {
            if (_customMeshGOs.ContainsKey(data.ID))
            {
                Transform transform = _customMeshGOs[data.ID].transform;

                transform.position = BrainAtlasManager.ActiveReferenceAtlas.Atlas2World(data.Position, data.UseReference? data.UseReference : true);
            }
            else
                Client_SocketIO.LogWarning($"Custom mesh {data.ID} does not exist in the scene, cannot set position");
        }

        public void Clear()
        {
            foreach (var kvp in _customMeshGOs)
                Destroy(kvp.Value);

            _customMeshGOs.Clear();
        }
    }

}