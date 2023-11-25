using BrainAtlas;
using System;
using UnityEngine;
using Urchin.API;

public class VolumeSliceBehavior : MonoBehaviour
{
    [SerializeField] private VolumeRenderer _volumeRenderer;
    [SerializeField] private Vector3 _xAxis;
    [SerializeField] private Vector3 _yAxis;
    [SerializeField] private Vector3 _missingDim;

    private void OnMouseDown()
    {
        // Get the click position in screen space
        Vector3 clickPosition = Input.mousePosition;

        // Create a ray from the camera through the click position
        Ray ray = Camera.main.ScreenPointToRay(clickPosition);

        // Declare a variable to store the hit information
        RaycastHit hit;

        // Check if the ray hits the collider
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 coordIdxU = BrainAtlasManager.ActiveReferenceAtlas.World2AtlasIdx(hit.point);

            VolumeClickData data = new();
            data.ap = coordIdxU.x;
            data.ml = coordIdxU.y;
            data.dv = coordIdxU.z;

            Client_SocketIO.Emit("VolumeClick", JsonUtility.ToJson(data));
        }
    }

    [Serializable]
    private struct VolumeClickData
    {
        public float ap;
        public float ml;
        public float dv;
    }
}
