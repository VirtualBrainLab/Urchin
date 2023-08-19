using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FOVBehavior : MonoBehaviour
{

    #region Variables
    private int[] _order = { 3, 1, 2, 0 }; // for reordering vertices to match the mesh
                                          // vertex order given by user: start from top left, go clockwise
    private MeshFilter _meshFilter;
    #endregion

    #region
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }
    #endregion

    /// <summary>
    /// Set the position of the four vertex corners of the Quad
    /// </summary>
    /// <param name="vertices"></param>
    public void SetPosition(List<Vector3> vertices)
    {
        // Compute the center of the quad
        Vector3 center = new Vector3(0f, 0f, 0f);
        for (int i = 0; i < vertices.Count; i++)
            center += vertices[i];
        center = center / 4f;
        Debug.Log("Setting center of quad " + name + "at: " + center);

        // Re-order vertices for MeshFilter
        for (int i = 0; i < vertices.Count; i++)
            vertices[i] = vertices[i] - center;
        Vector3[] verticesReordered = new Vector3[4];
        for (int i = 0; i < vertices.Count; i++)
            verticesReordered[i] = vertices[_order[i]];

        // Apply vertex positions
        _meshFilter.mesh.vertices = verticesReordered;
        transform.position = center;
    }
}
