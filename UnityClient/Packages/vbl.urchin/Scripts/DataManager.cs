using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Urchin.Managers;

public class DataManager : MonoBehaviour
{
    [SerializeField] PrimitiveMeshManager _primitiveMeshManager;

    [SerializeField] private string apiURL;

    private List<Manager> _managers;

    #region Unity
    private void Awake()
    {
        _managers = new();
        _managers.Add(_primitiveMeshManager);
    }
    #endregion

    public void Save(string sceneName)
    {
        FlattenedData data = new();
        data.Data = new ManagerData[_managers.Count];

        for (int i = 0; i < _managers.Count; i++)
        {
            data.Data[i].Type = _managers[i].Type;
            data.Data[i].Data = _managers[i].ToSerializedData();
        }

        // Push data up to the REST API
        throw new System.NotImplementedException(); 
    }

    public void Load(string targetURL)
    {
        throw new System.NotImplementedException();

        // Get the manager data back from the API
        FlattenedData data = new();

        foreach (ManagerData managerData in data.Data)
        {
            switch (managerData.Type)
            {
                case ManagerType.PrimitiveMeshManager:
                    _primitiveMeshManager.FromSerializedData(managerData.Data);
                    break;
            }
        }
    }

    private struct FlattenedData
    {
        public ManagerData[] Data;
    }


    private struct ManagerData
    {
        public ManagerType Type;
        public string Data;
    }
}
