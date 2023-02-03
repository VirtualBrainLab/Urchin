using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProbeManager : MonoBehaviour
{
    #region Variables
    [SerializeField] private List<GameObject> _probePrefabOptions;
    [SerializeField] private List<string> _probePrefabNames;
    [SerializeField] private string _defaultProbeStyle;
    [SerializeField] private Transform _probeParentT;

    // Dictionary of string -> GO keeping track of probe style options
    private Dictionary<string, GameObject> _probeOpts;
    private GameObject _defaultPrefab;

    // Actual objects
    private Dictionary<string, GameObject> _probes;
    private Dictionary<string, Vector3[]> _probeCoordinates;
    #endregion

    #region Unity

    private void Awake()
    {
        // Initialize variables
        _probeOpts = new Dictionary<string, GameObject>();
        _probes = new Dictionary<string, GameObject>();
        _probeCoordinates = new Dictionary<string, Vector3[]>();

        _defaultPrefab = _probePrefabOptions[_probePrefabNames.IndexOf(_defaultProbeStyle)];

        // Fill dictionaries
        if (_probePrefabOptions.Count == _probePrefabNames.Count)
        {
            for (int i = 0; i < _probePrefabOptions.Count; i++)
                _probeOpts.Add(_probePrefabNames[i], _probePrefabOptions[i]);
        }
        else
            throw new System.Exception("Number of prefab options and names must match");
    }

    #endregion

    #region Public object functions

    public void CreateProbes(List<string> probeNames)
    {
        foreach (string probeName in probeNames)
        {
            if (!_probes.ContainsKey(probeName))
            {
                GameObject newProbe = Instantiate(_defaultPrefab, _probeParentT);
                newProbe.name = $"probe_{probeName}";
                _probes.Add(probeName, newProbe);
                _probeCoordinates.Add(probeName, new Vector3[2]);
                SetProbePositionAndAngles(probeName);
                UM_Launch.Log("Created probe: " + probeName);
            }
            else
            {
                Client.LogError(string.Format("Probe {0} already exists in the scene", probeName));
            }
        }
    }

    public void DeleteProbes(List<string> probeNames)
    {
        foreach (string probeName in probeNames)
        {
            Destroy(_probes[probeName]);
            _probes.Remove(probeName);
        }
    }

    public void SetProbeColor(Dictionary<string, string> probeColors)
    {
        foreach (KeyValuePair<string, string> kvp in probeColors)
        {
            if (_probes.ContainsKey(kvp.Key))
            {
                Color newColor;
                if (ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
                {
                    Debug.Log("Setting " + kvp.Key + " to " + kvp.Value);
                    _probes[kvp.Key].GetComponentInChildren<Renderer>().material.color = newColor;
                }
            }
            else
                UM_Launch.Log("Probe " + kvp.Key + " not found");
        }
    }

    public void SetProbePosition(Dictionary<string, List<float>> probePositions)
    {
        foreach (KeyValuePair<string, List<float>> kvp in probePositions)
        {
            if (_probes.ContainsKey(kvp.Key))
            {
                // store coordinates in mlapdv       
                _probeCoordinates[kvp.Key][0] = new Vector3(kvp.Value[0], kvp.Value[1], kvp.Value[2]);
                SetProbePositionAndAngles(kvp.Key);
            }
            else
                UM_Launch.Log("Probe " + kvp.Key + " not found");
        }
    }

    public void SetProbeAngle(Dictionary<string, List<float>> probeAngles)
    {
        foreach (KeyValuePair<string, List<float>> kvp in probeAngles)
        {
            if (_probes.ContainsKey(kvp.Key))
            {
                // store coordinates in mlapdv       
                _probeCoordinates[kvp.Key][1] = new Vector3(kvp.Value[0], kvp.Value[1], kvp.Value[2]);
                SetProbePositionAndAngles(kvp.Key);
            }
            else
                UM_Launch.Log("Probe " + kvp.Key + " not found");
        }
    }

    public void SetProbeStyle(Dictionary<string, string> probeStyles)
    {
        foreach (KeyValuePair<string, string> kvp in probeStyles)
        {
            if (_probes.ContainsKey(kvp.Key))
            {
                Destroy(_probes[kvp.Key]);
                _probes[kvp.Key] = Instantiate(_probePrefabOptions[_probePrefabNames.IndexOf(kvp.Value)], _probeParentT);
                _probes[kvp.Key].name = $"probe_{kvp.Key}";

                SetProbePositionAndAngles(kvp.Key);
            }
        }
    }

    public void SetProbeScale(Dictionary<string, List<float>> probeScales)
    {
        foreach (KeyValuePair<string, List<float>> kvp in probeScales)
        {
            if (_probes.ContainsKey(kvp.Key))
            {
                // store coordinates in mlapdv       
                _probes[kvp.Key].transform.GetChild(0).localScale = new Vector3(kvp.Value[0], kvp.Value[1], kvp.Value[2]);
                _probes[kvp.Key].transform.GetChild(0).localPosition = new Vector3(0f, kvp.Value[1] / 2, 0f);
            }
            else
                UM_Launch.Log("Probe " + kvp.Key + " not found");
        }
    }

    public void ClearProbes()
    {
        Debug.Log("(Client) Clearing probes");
        foreach (GameObject probe in _probes.Values)
            Destroy(probe);
        _probes = new Dictionary<string, GameObject>();
        _probeCoordinates = new Dictionary<string, Vector3[]>();
    }

    #endregion

    #region Private helpers

    private void SetProbePositionAndAngles(string probeName)
    {
        Vector3 pos = _probeCoordinates[probeName][0];
        Vector3 angles = _probeCoordinates[probeName][1];
        Transform probeT = _probes[probeName].transform;

        // reset position and angles
        probeT.transform.localPosition = Vector3.zero;
        probeT.localRotation = Quaternion.identity;

        // then translate
        probeT.Translate(new Vector3(-pos.x / 1000f, -pos.z / 1000f, pos.y / 1000f));
        // rotate around azimuth first
        probeT.RotateAround(probeT.position, Vector3.up, -angles.x - 90f);
        // then elevation
        probeT.RotateAround(probeT.position, probeT.right, angles.y);
        // then spin
        probeT.RotateAround(probeT.position, probeT.up, angles.z);

    }
    #endregion
}
