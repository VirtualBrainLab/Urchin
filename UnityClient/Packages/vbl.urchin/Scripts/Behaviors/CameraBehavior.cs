using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using Urchin.Cameras;

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

            // Build the messages and send them
            ScreenshotReturnMeta meta = new();
            meta.name = Name;
            meta.totalBytes = bytes.Length;
            throw new NotImplementedException();
            //Client_SocketIO.Emit("ReceiveCameraImgMeta", JsonUtility.ToJson(meta));

            int nChunks = Mathf.CeilToInt((float)bytes.Length / (float)SOCKET_IO_MAX_CHUNK_BYTES);

            for (int i = 0; i < nChunks; i++)
            {
                ScreenshotChunk chunk = new();
                chunk.name = Name;

                int cChunkSize = Mathf.Min(SOCKET_IO_MAX_CHUNK_BYTES, bytes.Length - i * SOCKET_IO_MAX_CHUNK_BYTES);
                chunk.data = new byte[cChunkSize];
                Buffer.BlockCopy(bytes, i * SOCKET_IO_MAX_CHUNK_BYTES, chunk.data, 0, cChunkSize);
                throw new NotImplementedException();
                //Client.Emit("ReceiveCameraImg", JsonUtility.ToJson(chunk));
            }
        }

        [Serializable]
        private struct ScreenshotReturnMeta
        {
            public string name;
            public int totalBytes;
        }

        [Serializable]
        private struct ScreenshotChunk
        {
            public string name;
            public byte[] data;
        }

        public void SetCameraPosition(List<float> obj)
        {
            // position in ml/ap/dv relative to ccf 0,0,0
            throw new NotImplementedException();
            //Client.LogError("Setting camera position not implemented yet. Use set_camera_target and set_camera_rotation instead.");
            //Vector3 ccfPosition25 = new Vector3(obj[0]/25, obj[1]/25, obj[2]/25);
            //cameraControl.SetOffsetPosition(Utils.apdvlr2World(ccfPosition25));
        }

        public void SetCameraYAngle(float obj)
        {
            _cameraControl.SetSpin(obj);
        }

        public void SetCameraTargetArea(string obj)
        {
            throw new NotImplementedException();
            //(int ID, bool full, bool leftSide, bool rightSide) = AreaManager.GetID(obj);
            //CCFTreeNode node = ModelControl.tree.findNode(ID);
            //if (node != null)
            //{
            //    Vector3 center;
            //    if (full)
            //        center = node.GetMeshCenterFull();
            //    else
            //        center = node.GetMeshCenterSided(leftSide);
            //    _cameraControl.SetCameraTarget(center);
            //}
            //else
            //    RendererManager.Log("Failed to find node to set camera target: " + obj);
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
}