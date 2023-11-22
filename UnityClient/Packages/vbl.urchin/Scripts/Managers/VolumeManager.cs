using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Urchin.API;

namespace Urchin.Managers
{
    public class VolumeManager : MonoBehaviour
    {
        #region Serialized
        [SerializeField] private Transform _volumeParentT;
        [SerializeField] private GameObject _volumePrefab;
        #endregion

        #region Variables
        private Dictionary<string, VolumeRenderer> _volumes;
        #endregion

        #region Unity
        private void Awake()
        {
            _volumes = new();
        }

        private void Start()
        {
            Client_SocketIO.SetVolumeData += SetData;
            Client_SocketIO.UpdateVolume += UpdateOrCreate;
            Client_SocketIO.DeleteVolume += Delete;

            Client_SocketIO.ClearVolumes += Clear;
        }

        #endregion

        #region Public functions

        public void UpdateOrCreate(VolumeMeta volumeMeta)
        {
            VolumeRenderer volRenderer;
            if (!_volumes.ContainsKey(volumeMeta.name))
            {
                GameObject newVolume = Instantiate(_volumePrefab, _volumeParentT);

                volRenderer = newVolume.GetComponent<VolumeRenderer>();
                _volumes.Add(volumeMeta.name, volRenderer);
            }
            else
                volRenderer = _volumes[volumeMeta.name];

            volRenderer.SetMetadata(volumeMeta.nCompressedBytes);
            volRenderer.SetColormap(volumeMeta.colormap);
            volRenderer.SetVisibility(volumeMeta.visible);
        }

        public void SetVisibility(List<object> data)
        {
            string volumeName = (string)data[0];
            bool visibility = (bool)data[1];

            _volumes[volumeName].SetVisibility(visibility);
        }

        public void Delete(string name)
        {
            GameObject volumeGO = _volumes[name].gameObject;
            Destroy(volumeGO);
            _volumes.Remove(name);
        }

        public void SetData(VolumeDataChunk chunk)
        {
            _volumes[chunk.name].SetData(chunk.compressedByteChunk);
        }
        public void SetAnnotationColor(Dictionary<string, string> data)
        {
            throw new NotImplementedException();
        }

        public void SetSlice(List<float> obj)
        {
            throw new NotImplementedException();
            //Debug.Log("Not implemented");
        }

        public void Clear()
        {
            Debug.Log("(Client) Clearing volumes");
            foreach (var kvp in _volumes)
                Destroy(kvp.Value.gameObject);

            _volumes.Clear();
        }
        #endregion

        public void UpdateCameraRotation()
        {
            foreach (VolumeRenderer vr in _volumes.Values)
            {
                vr.UpdateCameraPosition();
            }
        }
    }
}