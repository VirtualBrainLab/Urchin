using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Urchin.Cameras;

namespace BrainAtlas.Cameras
{
    public class UIControlClickDetector : MonoBehaviour
    {
        public Camera renderTextureCamera;
        public LayerMask layerMask;
        private RectTransform rawImageRectTransform;

        void Start()
        {
            rawImageRectTransform = GetComponent<RectTransform>();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 localMousePosition;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImageRectTransform, Input.mousePosition, Camera.main, out localMousePosition))
                {
                    Vector2 normalizedPosition = Rect.PointToNormalized(rawImageRectTransform.rect, localMousePosition);
                    Ray ray = renderTextureCamera.ViewportPointToRay(new Vector3(normalizedPosition.x, normalizedPosition.y, 0));
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                    {
                        CameraMiniControllerHandle handle;
                        if (hit.collider.gameObject.TryGetComponent(out handle))
                            handle.Click();
                    }
                }
            }
        }
    }
}