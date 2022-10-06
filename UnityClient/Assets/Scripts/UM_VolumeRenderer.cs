using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UM_VolumeRenderer : MonoBehaviour
{
    [SerializeField] GameObject volumeLoadingUIGO;
    [SerializeField] TMP_Text volumeLoadingUIText;

    [SerializeField] UM_CameraController cameraControl;

    private Texture3D volumeTexture;
    private Dictionary<string, Color32[]> volumeData;
    private Dictionary<string, Color32[]> colormaps;

    private Color32[] defaultColormap;
    private Color32 black;
    private Color32 transparentBlack;

    private void Awake()
    {
        volumeTexture = new Texture3D(528, 320, 456, TextureFormat.RGBA32, false);
        GetComponent<Renderer>().material.mainTexture = volumeTexture;
        volumeData = new Dictionary<string, Color32[]>();
        colormaps = new Dictionary<string, Color32[]>();

        black = new Color32(0, 0, 0, 1);
        transparentBlack = new Color32(0, 0, 0, 0);

        defaultColormap = new Color32[256];
        for (int i = 0; i < 255; i++)
            defaultColormap[i] = black;
        defaultColormap[255] = transparentBlack;
    }

    public void CreateVolume(string name)
    {
        Debug.Log("(UM_VolRend) Creating new volume: " + name);
        // Size is currently hard-coded
        if (volumeData.ContainsKey(name))
            volumeData[name] = new Color32[528 * 320 * 456];
        else
            volumeData.Add(name, new Color32[528*320*456]);
        // Initialize everything with transparent black
        for (int i = 0; i < volumeData.Count; i++)
            volumeData[name][i] = transparentBlack;

        // Reset the colormap
        if (colormaps.ContainsKey(name))
            colormaps[name] = defaultColormap;
        else
            colormaps.Add(name, defaultColormap);
    }

    public void DeleteVolume(string name)
    {
        volumeData.Remove(name);
    }

    public void SetVolumeColormap(string name, List<string> hexColors)
    {
        Debug.Log("(UM_VolRend) Creating new colormap for: " + name);
        Color32[] newMap = new Color32[256];
        for (int i = 0; i < 255; i++)
        {
            if (i < hexColors.Count)
                newMap[i] = Utils.ParseHexColor(hexColors[i]);
            else
                newMap[i] = black;
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
            volumeTexture.SetPixels32(volumeData[name]);
            volumeTexture.Apply();
            GetComponent<Renderer>().enabled = true;

            // Force the camera to perspective
            cameraControl.SwitchCameraMode(false);
        }
        else if (!visible)
            GetComponent<Renderer>().enabled = false;
    }

    public async void DisplayAllenVolume(bool visible)
    {
        if (visible)
        {
            Task<Texture3D> volumeTexTask = AddressablesRemoteLoader.LoadAnnotationTexture();
            await volumeTexTask;

            volumeTexture = volumeTexTask.Result;
            GetComponent<Renderer>().material.mainTexture = volumeTexture;
            volumeTexture.Apply();

            GetComponent<Renderer>().enabled = true;

            // Force the camera to perspective
            cameraControl.SwitchCameraMode(false);
        }
        else
            GetComponent<Renderer>().enabled = false;

        Debug.Log("(UM_VolRend) Volume: allen is now visible: " + visible);
    }

    private string nextVol;
    private int nextSlice;
    private bool nextApply;

    public void AddVolumeMeta(string name, int slice, bool immediateApply)
    {
        volumeLoadingUIGO.SetActive(true);
        nextVol = name;
        nextSlice = slice;
        nextApply = immediateApply;
    }

    public void AddVolumeData(byte[] newData)
    {
        Debug.Log("Adding slice data for " + nextSlice);
        int slicePos = nextSlice * 528 * 320;
        for (int i = 0; i < newData.Length; i++)
            volumeData[nextVol][i + slicePos] = colormaps[nextVol][newData[i]];

        // slices count down, so invert that
        volumeLoadingUIText.text = (456-nextSlice+1) + "/456";

        if (nextApply)
        {
            volumeTexture.SetPixels32(volumeData[nextVol]);
            volumeTexture.Apply();
            volumeLoadingUIGO.SetActive(false);
        }
    }

    public void Clear()
    {
        GetComponent<Renderer>().enabled = false;
    }
}
