using UnityEngine;

public class ProbeBehavior : MonoBehaviour
{
    [SerializeField] private GameObject _probeModelGO;
    [SerializeField] private bool _sizeSettable;

    public void SetPosition(Vector3 tipWorldU)
    {
        transform.localPosition = tipWorldU;
    }

    public void SetAngles(Vector3 angles)
    {
        transform.rotation = Quaternion.identity;
        // rotate around azimuth first
        transform.RotateAround(transform.position, Vector3.up, angles.x);
        // then elevation
        transform.RotateAround(transform.position, transform.right, angles.y);
        // then spin
        transform.RotateAround(transform.position, transform.forward, angles.z);
    }

    public void SetStyle(string style)
    {
        //todo
    }

    public void SetColor(Color color)
    {
        _probeModelGO.GetComponent<Renderer>().material.color = color;
    }

    public void SetScale(Vector3 scale)
    {
        _probeModelGO.transform.localScale = scale;
    }

    public bool SetSize(Vector3 size)
    {
        if (_sizeSettable)
        {
            _probeModelGO.transform.localScale = size;

            _probeModelGO.transform.localPosition = new Vector3(0f, 0f, -size.y / 2);

            return true;
        }
        else
            return false;
    }
}
