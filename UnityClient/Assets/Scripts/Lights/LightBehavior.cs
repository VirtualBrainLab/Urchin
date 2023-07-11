using UnityEngine;

/// <summary>
/// Control system for the scene lighting
/// </summary>
public class LightBehavior : MonoBehaviour
{
    [SerializeField] Transform _cameraTransform;

    private bool _cameraControl = true;

    private void Update()
    {
        if (_cameraControl)
        {
            transform.localPosition = _cameraTransform.localPosition;
            transform.rotation = _cameraTransform.rotation;
        }
    }

    public void SetCamera(GameObject cameraGO)
    {
        _cameraControl = true;
        _cameraTransform = cameraGO.transform;
    }

    public void SetRotation(Vector3 eulerAngles)
    {
        _cameraControl = false;
        transform.rotation = Quaternion.Euler(eulerAngles);
    }
}
