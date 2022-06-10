using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UM_VolumeRenderer : MonoBehaviour
{
    private Texture3D volumeTexture;
    private Dictionary<string, Color[]> volumeData;
    private Dictionary<string, Color[]> colormaps;

    private Color[] defaultColormap;
    private Color transparentBlack;

    private void Awake()
    {
        volumeTexture = new Texture3D(528, 320, 456, TextureFormat.RGBA32, false);
        GetComponent<Renderer>().material.mainTexture = volumeTexture;
        volumeData = new Dictionary<string, Color[]>();
        colormaps = new Dictionary<string, Color[]>();

        transparentBlack = new Color(0, 0, 0, 0);

        defaultColormap = new Color[256];
        for (int i = 0; i < 255; i++)
            defaultColormap[i] = Color.black;
        defaultColormap[255] = transparentBlack;
    }

    public void CreateVolume(string name)
    {
        Debug.Log("(UM_VolRend) Creating new volume: " + name);
        // Size is currently hard-coded
        if (volumeData.ContainsKey(name))
            volumeData[name] = new Color[528 * 320 * 456];
        else
            volumeData.Add(name, new Color[528*320*456]);
        // Initialize everything with transparent black
        for (int i = 0; i < volumeData.Count; i++)
            volumeData[name][i] = transparentBlack;

        // Reset the colormap
        if (colormaps.ContainsKey(name))
            colormaps[name] = defaultColormap;
        else
            colormaps.Add(name, defaultColormap);
    }

    public void SetVolumeColormap(string name, List<string> hexColors)
    {
        Debug.Log("(UM_VolRend) Creating new colormap for: " + name);
        Color[] newMap = new Color[256];
        for (int i = 0; i < 255; i++)
        {
            if (i < hexColors.Count)
                newMap[i] = Utils.ParseHexColor(hexColors[i]);
            else
                newMap[i] = Color.black;
        }
        newMap[255] = transparentBlack;
        if (colormaps.ContainsKey(name))
            colormaps[name] = newMap;
        else
            colormaps.Add(name,newMap);
    }

    public void SetVolumeVisibility(string name, bool visible)
    {
        Debug.Log("(UM_VolRend) Volume: " + name + " is now visible: " + visible);
        if (volumeData.ContainsKey(name) && visible)
        {
            volumeTexture.SetPixels(volumeData[name]);
            volumeTexture.Apply();
            GetComponent<Renderer>().enabled = true;
        }
        else if (!visible)
            GetComponent<Renderer>().enabled = false;
    }

    public async void DisplayAllenVolume(bool visible)
    {
        Debug.Log("(UM_VolRend) Volume: allen is now visible: " + visible);
        if (visible)
        {
            Task<Texture3D> volumeTexTask = AddressablesRemoteLoader.LoadAnnotationTexture();
            await volumeTexTask;

            volumeTexture = volumeTexTask.Result;

            GetComponent<Renderer>().enabled = true;
        }
        else
            GetComponent<Renderer>().enabled = false;
    }

    private string nextVol;
    private int nextSlice;

    public void AddVolumeMeta(string name, int slice)
    {
        nextVol = name;
        nextSlice = slice;
    }

    public void AddVolumeData(byte[] newData)
    {
        for (int i = 0; i < newData.Length; i++)
        {
            int pos = i + nextSlice * 456;
            volumeData[nextVol][pos] = colormaps[nextVol][newData[i]];
        }
    }

    public void Clear()
    {
        GetComponent<Renderer>().enabled = false;
    }
}
