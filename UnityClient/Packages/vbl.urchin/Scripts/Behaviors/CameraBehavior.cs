using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BrainAtlas;
using Urchin.API;

namespace Urchin.Behaviors
{

    public class CameraBehavior : MonoBehaviour
    {
        #region Serialized
        [SerializeField] BrainCameraController _cameraControl;
        [SerializeField] RectTransform _cropWindowRT;

        [SerializeField] Camera _orthoCamera;
        [SerializeField] Camera _perspectiveCamera;

        #endregion

        public string Name;

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

        public Camera ActiveCamera
        {
            get
            {
                if (_orthoCamera.isActiveAndEnabled)
                {
                    return _orthoCamera;
                }
                else
                {
                    return _perspectiveCamera;
                }
            }
        }

        private void Update()
        {
            //Debug.Log(_cropWindowRT.rect.position);
        }

        #region Public functions

        public void SetCameraControl(bool controllable)
        {
            _cameraControl.UserControllable = controllable;
        }

        public void SetCameraRotation(Vector3 yawPitchRoll)
        {
            _cameraControl.SetBrainAxisAngles(yawPitchRoll);
        }

        public void SetCameraZoom(float obj)
        {
            _cameraControl.SetZoom(obj);
        }

        public void SetCameraMode(string mode)
        {
            SetCameraMode(mode.Equals("orthographic"));
        }

        public void SetCameraMode(bool orthographic)
        {
            if (orthographic)
            {
                _orthoCamera.gameObject.SetActive(true);
                _perspectiveCamera.gameObject.SetActive(false);

                _cameraControl.SetCamera(_orthoCamera);
            }
            else
            {
                _orthoCamera.gameObject.SetActive(false);
                _perspectiveCamera.gameObject.SetActive(true);
                _cameraControl.SetCamera(_perspectiveCamera);
            }
        }

        /// <summary>
        /// Take a screenshot and send it back via the ReceiveCameraImgMeta and ReceiveCameraImg messages
        /// </summary>
        /// <param name="size"></param>
        public void Screenshot(int[] size)
        {
            StartCoroutine(ScreenshotHelper(size));
        }

        /// <summary>
        /// Capture the output from this camera into a texture
        /// </summary>
        /// <returns></returns>
        private IEnumerator ScreenshotHelper(int[] size)
        {
            Debug.Log($"{size[0]},{size[1]}");

            RenderTexture originalTexture = ActiveCamera.targetTexture;

            RenderTexture captureTexture = new RenderTexture(size[0], size[1], 24);
            ActiveCamera.targetTexture = captureTexture;

            yield return new WaitForEndOfFrame();

            // Save to Texture2D
            Texture2D screenshotTexture = new Texture2D(size[0], size[1], TextureFormat.RGB24, false);
            RenderTexture.active = captureTexture;
            screenshotTexture.ReadPixels(new Rect(0, 0, size[0], size[1]), 0, 0);
            screenshotTexture.Apply();

            // return the camera
            ActiveCamera.targetTexture = originalTexture;
            RenderTexture.active = null;
            captureTexture.Release();

            // Convert to PNG
            byte[] bytes = screenshotTexture.EncodeToPNG();

            Debug.Log(bytes.Length);

            // Build the messages and send them
            ScreenshotReturnMeta meta = new();
            meta.name = Name;
            meta.totalBytes = bytes.Length;
            Client_SocketIO.Emit("ReceiveCameraImgMeta", JsonUtility.ToJson(meta));


            Debug.Log(JsonUtility.ToJson(meta));

            int nChunks = Mathf.CeilToInt((float)bytes.Length / (float)Client_SocketIO.SOCKET_IO_MAX_CHUNK_BYTES);

            Debug.Log(nChunks);

            for (int i = 0; i < nChunks; i++)
            {
                ScreenshotChunk chunk = new();
                chunk.name = Name;

                int cChunkSize = Mathf.Min(Client_SocketIO.SOCKET_IO_MAX_CHUNK_BYTES, bytes.Length - i * Client_SocketIO.SOCKET_IO_MAX_CHUNK_BYTES);
                chunk.data = new byte[cChunkSize];
                Buffer.BlockCopy(bytes, i * Client_SocketIO.SOCKET_IO_MAX_CHUNK_BYTES, chunk.data, 0, cChunkSize);
                Client_SocketIO.Emit("ReceiveCameraImg", JsonUtility.ToJson(chunk));
            }
        }

        [Serializable]
        private struct ScreenshotReturnMeta
        {
            public string name;
            public int totalBytes;
        }

        [Serializable, PreferBinarySerialization]
        private class ScreenshotChunk
        {
            public string name;
            public byte[] data;
        }

        public void SetCameraYAngle(float yaw)
        {
            Vector3 angles = _cameraControl.PitchYawRoll;
            angles.y += yaw;
            _cameraControl.SetBrainAxisAngles(angles);
        }

        public void SetCameraTarget(Vector3 coordAtlas)
        {
            // data comes in in um units in ml/ap/dv
            // note that (0,0,0) in world is the center of the brain
            // so offset by (-6.6 ap, -4 dv, -5.7 lr) to get to the corner
            // in world space, x = ML, y = DV, z = AP
            _cameraControl.SetCameraTarget(BrainAtlasManager.ActiveReferenceAtlas.Atlas2World(coordAtlas));
        }

        public void SetCameraPan(List<float> panXY)
        {
            throw new NotImplementedException();
            //_cameraControl.SetP(new Vector2(panXY[0], panXY[1]));
        }
        #endregion
    }
}