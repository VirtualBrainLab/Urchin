using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Urchin.API;

public class MeshBehavior : MonoBehaviour
{
    private string ID;

    #region Unity
    private Vector3 _originalScale;
    private Color _originalColor;
    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    private void OnMouseEnter()
    {
        _originalScale = transform.localScale;
        transform.localScale = _originalScale * 4f;

        _originalColor = _renderer.material.color;
        _renderer.material.color = Color.red;
    }

    private void OnMouseExit()
    {
        transform.localScale = _originalScale;
        _renderer.material.color = _originalColor;
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // left mouse click
            Client_SocketIO.Emit("NeuronCallback", ID);
        }
    }
    #endregion

    public void SetID(string ID)
    {
        this.ID = ID;
    }
}
