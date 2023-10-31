using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urchin.Cameras
{
    public class CameraMiniController : MonoBehaviour
    {
        Quaternion _initialRotation;

        private void Awake()
        {
            _initialRotation = transform.localRotation;
        }

        public void UpdateRotation(Vector3 pitchYawRoll)
        {
            transform.localRotation = _initialRotation * Quaternion.Euler(pitchYawRoll);
        }
    }
}