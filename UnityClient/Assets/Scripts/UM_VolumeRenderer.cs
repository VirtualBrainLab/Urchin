using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UM_VolumeRenderer : MonoBehaviour
{
    private Texture3D volumeTexture;

    public async void DisplayAllenVolume()
    {
        Task<Texture3D> volumeTexTask = AddressablesRemoteLoader.LoadAnnotationTexture();
        await volumeTexTask;

        volumeTexture = volumeTexTask.Result;

        GetComponent<Renderer>().material.mainTexture = volumeTexture;
        GetComponent<Renderer>().enabled = true;
    }

    public async void ChangeVolumeData()
    {

    }

    public void Clear()
    {
        GetComponent<Renderer>().enabled = false;
    }
}
