using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeManager : MonoBehaviour
{
    #region Serialized
    [SerializeField] private UM_VolumeRenderer volRenderer;
    #endregion

    #region Public functions
    public void SetVisibility(List<object> data)
    {
        if (((string)data[0]).Equals("allen"))
            volRenderer.DisplayAllenVolume((bool)data[1]);
        else
            volRenderer.SetVolumeVisibility((string)data[0], (bool)data[1]);
    }

    public void SetVolumeColormap(List<string> obj)
    {
        string name = obj[0];
        obj.RemoveAt(0);
        volRenderer.SetVolumeColormap(name, obj);
    }

    public void Create(string name)
    {
        volRenderer.CreateVolume(name);
    }

    public void Delete(string name)
    {
        volRenderer.DeleteVolume(name);
    }

    public void SetMetadata(List<object> data)
    {
        volRenderer.AddVolumeMeta((string)data[0], (int)data[1], (bool)data[2]);
    }
    public void SetData(byte[] bytes)
    {
        volRenderer.AddVolumeData(bytes);
    }
    public void SetAnnotationColor(Dictionary<string, string> data)
    {
        UM_Launch.Log("Not implemented");
    }

    public void SetSlice(List<float> obj)
    {
        UM_Launch.Log("Not implemented");
    }

    public void Clear()
    {
        Debug.Log("(Client) Clearing volumes");
        volRenderer.Clear();
    }
    #endregion
}
