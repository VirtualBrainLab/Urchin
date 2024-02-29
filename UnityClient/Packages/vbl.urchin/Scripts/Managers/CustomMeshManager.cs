using BrainAtlas;
using System.Collections;
using System.Collections.Generic;
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
        Dictionary<string, GameObject> _customMeshGOs;
        #endregion

        private void Start()
        {
            _customMeshGOs = new();

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
            mesh.vertices = data.vertices;

            int[] triangles = new int[data.triangles.Length * 3];
            for (int i = 0; i < data.triangles.Length; i++)
            {
                triangles[i + 0] = data.triangles[i].x;
                triangles[i + 1] = data.triangles[i].y;
                triangles[i + 2] = data.triangles[i].z;
            }
            mesh.triangles = triangles;

            if (data.normals != null)
                mesh.normals = data.normals;

            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().material = MaterialManager.MeshMaterials["default"];

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