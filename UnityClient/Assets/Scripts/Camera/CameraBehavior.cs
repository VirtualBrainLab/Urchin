using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    #region Serialized
    [SerializeField] BrainCameraController _cameraControl;
    [SerializeField] UM_CameraController _umCameraControl;
    [SerializeField] AreaManager _areaManager;
    [SerializeField] CCFModelControl _modelControl;
    [SerializeField] RectTransform _cropWindowRT;

    [SerializeField] Camera _orthoCamera;
    [SerializeField] Camera _perspectiveCamera;

    #endregion

    #region Public properties

    private RenderTexture _renderTexture;
    public RenderTexture RenderTexture
    {
        get
        {
            return _renderTexture;
        }

        set
        {
            _renderTexture = value;
            _orthoCamera.targetTexture = _renderTexture;
            _perspectiveCamera.targetTexture = _renderTexture;
        }
    }
    #endregion

    #region private var
    private const int SOCKET_IO_MAX_CHUNK_BYTES = 1000000;
    #endregion

    private void Update()
    {
        //Debug.Log(_cropWindowRT.rect.position);
    }

    #region Public functions

    public void SetCameraControl(bool controllable)
    {
        _cameraControl.UserControllable = controllable;
    }
    
    public void SetCameraRotation(List<float> obj)
    {
        _cameraControl.SetBrainAxisAngles(new Vector3(obj[1], obj[0], obj[2]));
    }

    public void SetCameraZoom(float obj)
    {
        _cameraControl.SetZoom(obj);
    }

    public void SetCameraMode(string mode)
    {
        _umCameraControl.SwitchCameraMode(mode.Equals("orthographic"));
    }

    /// <summary>
    /// Take a single screenshot at the end of this frame and send that to the client
    /// via the ReceiveCameraImg streaming API
    /// </summary>
    public void Screenshot()
    {
        StartCoroutine(ScreenshotHelper());
    }

    private IEnumerator ScreenshotHelper()
    {
        yield return new WaitForEndOfFrame();
        var texture = ScreenCapture.CaptureScreenshotAsTexture();
        //texture

        byte[] data = texture.EncodeToPNG();
        Debug.Log(data.Length);

        int nChunks = Mathf.CeilToInt((float)data.Length / SOCKET_IO_MAX_CHUNK_BYTES);

        // tell the client how many messages to expect
        Client.Emit("ReceiveCameraImgMeta", nChunks);

        // split the texture byte[] into 1000 byte chunks
        for (int i = 0; i < nChunks; i++)
        {
            int cChunkSize = Mathf.Min(SOCKET_IO_MAX_CHUNK_BYTES, data.Length - i * SOCKET_IO_MAX_CHUNK_BYTES);
            byte[] chunkData = new byte[cChunkSize];
            Buffer.BlockCopy(data, i * SOCKET_IO_MAX_CHUNK_BYTES, chunkData, 0, cChunkSize);
            Client.Emit("ReceiveCameraImg", chunkData);
        }

    }

    public void SetCameraPosition(List<float> obj)
    {
        // position in ml/ap/dv relative to ccf 0,0,0
        Client.LogError("Setting camera position not implemented yet. Use set_camera_target and set_camera_rotation instead.");
        //Vector3 ccfPosition25 = new Vector3(obj[0]/25, obj[1]/25, obj[2]/25);
        //cameraControl.SetOffsetPosition(Utils.apdvlr2World(ccfPosition25));
    }

    public void SetCameraYAngle(float obj)
    {
        _cameraControl.SetSpin(obj);
    }

    public void SetCameraTargetArea(string obj)
    {
        (int ID, bool full, bool leftSide, bool rightSide) = _areaManager.GetID(obj);
        CCFTreeNode node = _modelControl.tree.findNode(ID);
        if (node != null)
        {
            Vector3 center;
            if (full)
                center = node.GetMeshCenterFull();
            else
                center = node.GetMeshCenterSided(leftSide);
            _cameraControl.SetCameraTarget(center);
        }
        else
            UM_Launch.Log("Failed to find node to set camera target: " + obj);
    }

    public void SetCameraTarget(List<float> mlapdv)
    {
        // data comes in in um units in ml/ap/dv
        // note that (0,0,0) in world is the center of the brain
        // so offset by (-6.6 ap, -4 dv, -5.7 lr) to get to the corner
        // in world space, x = ML, y = DV, z = AP

        Vector3 worldCoords = new Vector3(5.7f - mlapdv[0] / 1000f, 4f - mlapdv[2] / 1000f, mlapdv[1] / 1000f - 6.6f);
        _cameraControl.SetCameraTarget(worldCoords);
    }

    public void SetCameraPan(List<float> panXY)
    {
        _cameraControl.SetCameraPan(new Vector2(panXY[0], panXY[1]));
    }
    #endregion
}
