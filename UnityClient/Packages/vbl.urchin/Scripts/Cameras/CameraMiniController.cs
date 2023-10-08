using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Urchin.Cameras
{
    public class CameraMiniController : MonoBehaviour
    {
        public void UpdateRotation(Vector3 pitchYawRoll)
        {
            transform.localRotation = Quaternion.Euler(new Vector3(-pitchYawRoll.x, -pitchYawRoll.z, pitchYawRoll.y));
        }
    }
}