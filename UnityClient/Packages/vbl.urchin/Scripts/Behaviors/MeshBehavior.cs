using BrainAtlas;
using UnityEngine;
using UnityEngine.UIElements;
using Urchin.API;

public class MeshBehavior : MonoBehaviour
{
    #region Properties
    public MeshModel Data;
    public MeshRenderer Renderer { get { return GetComponent<MeshRenderer>(); } }
    #endregion

    #region Unity

    private void OnMouseEnter()
    {
        if (Data.interactive)
        {
            transform.localScale = Data.scale * 4f;
            Renderer.material.color = Color.red;
        }
    }

    private void OnMouseExit()
    {
        if (Data.interactive)
        {
            transform.localScale = Data.scale;
            Renderer.material.color = Data.color;
        }
    }

    private void OnMouseDown()
    {
        if (Data.interactive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // left mouse click
                Client_SocketIO.Emit("NeuronCallback", Data.id);
            }
        }
    }
    #endregion

    #region Settings
    public void UpdateAll()
    {
        // Update all settings
        _SetPosition();
        _SetScale();
        _SetColor();
        _SetMaterial();
    }

    public void SetPosition(Vector3 coordAtlasU)
    {
        Data.position = coordAtlasU;
        _SetPosition();
    }

    private void _SetPosition()
    {
        transform.localPosition = BrainAtlasManager.ActiveReferenceAtlas.Atlas2World(Data.position);
    }

    public void SetScale(Vector3 scale)
    {
        Data.scale = scale;
        _SetScale();
    }

    private void _SetScale()
    {
        transform.localScale = Data.scale;
    }

    public void SetColor(Color color)
    {
        Data.color = color;
        _SetColor();
    }

    private void _SetColor()
    {
        Renderer.material.color = Data.color;
    }

    public void SetMaterial(string materialName)
    {
        Data.material = materialName;
        _SetMaterial();
    }

    private void _SetMaterial()
    {
        Renderer.material = MaterialManager.GetMaterial(Data.material);
    }
    #endregion
}
