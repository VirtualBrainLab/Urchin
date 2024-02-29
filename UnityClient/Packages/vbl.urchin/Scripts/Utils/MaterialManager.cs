using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    [SerializeField] private List<Material> _meshMaterials;
    [SerializeField] private List<string> _materialNames;

    private static MaterialManager Instance;
    private Dictionary<string, Material> _materialsDict;

    private void Awake()
    {
        if (Instance != null)
            throw new Exception("Only a single instance of MaterialManager should exist in the scene.");

        if (_meshMaterials.Count != _materialNames.Count)
            Debug.LogWarning("Mesh material and name count should be equal");

        _materialsDict = new();
        foreach (var zip in _meshMaterials.Zip(_materialNames, (material, name) => (material, name)))
            _materialsDict.Add(zip.name, zip.material);
    }

    public static Dictionary<string, Material> MeshMaterials { get { return Instance._materialsDict; } }
}
