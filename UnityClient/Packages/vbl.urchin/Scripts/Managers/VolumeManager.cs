using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Urchin.API;

namespace Urchin.Managers
{
    public class VolumeManager : MonoBehaviour
    {
        #region Serialized
        [SerializeField] private Transform _volumeParentT;
        [SerializeField] private GameObject _volumePrefab;
        [SerializeField] private GameObject _volumeUIGO;
        #endregion

        #region Properties

        private float _alpha;
        public UnityEvent<float> AlphaChangedEvent;
        public float Alpha
        {
            get
            {
                return _alpha;
            }
            set
            {
                _alpha = value;
                AlphaChangedEvent.Invoke(_alpha);
                foreach (VolumeRenderer vr in _volumes.Values)
                    vr.SetRayMarchAlpha(_alpha);
            }
        }

        private float _stepSize;
        public UnityEvent<float> StepSizeChangedEvent;
        public float StepSize
        {
            get
            {
                return _stepSize;
            }
            set
            {
                _stepSize = value;
                StepSizeChangedEvent.Invoke(_stepSize);
                foreach (VolumeRenderer vr in _volumes.Values)
                    vr.SetRayMarchStepSize(_stepSize);
            }
        }
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
                _volumeUIGO.SetActive(true);
                GameObject newVolume = Instantiate(_volumePrefab, _volumeParentT);

                volRenderer = newVolume.GetComponent<VolumeRenderer>();
                _volumes.Add(volumeMeta.name, volRenderer);
            }
            else
                volRenderer = _volumes[volumeMeta.name];

            volRenderer.SetMetadata(volumeMeta.nCompressedBytes);
            volRenderer.SetColormap(volumeMeta.colormap);
            volRenderer.SetVolumeVisibility(volumeMeta.visible);
            volRenderer.UpdateSlicePosition();
        }

        public void SetVisibility(List<object> data)
        {
            string volumeName = (string)data[0];
            bool visibility = (bool)data[1];

            _volumes[volumeName].SetVolumeVisibility(visibility);
        }

        public void Delete(string name)
        {
            GameObject volumeGO = _volumes[name].gameObject;
            Destroy(volumeGO);
            _volumes.Remove(name);

            if (_volumes.Count == 0)
                _volumeUIGO.SetActive(false);
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

        public void UpdateCoronalSlicePos(float percentage)
        {
            foreach (VolumeRenderer vr in _volumes.Values)
            {
                vr._slicePosition.x = percentage - 0.5f;
            }
        }

        public void UpdateSagittalSlicePos(float percentage)
        {
            foreach (VolumeRenderer vr in _volumes.Values)
            {
                vr._slicePosition.y = percentage - 0.5f;
            }
        }

        public void Toggle2DSlices(bool visible)
        {
            foreach (VolumeRenderer vr in _volumes.Values)
            {
                vr.SetSliceVisibility(visible);
            }
        }
    }
}