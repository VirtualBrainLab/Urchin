using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestEventAvgsAnimation : MonoBehaviour
{
    [SerializeField] private CCFModelControl modelControl;

    private List<CCFTreeNode> nodes;

    private Vector3 center = new Vector3(5.7f, 4f, -6.6f);

    private Vector3 teal = new Vector3(0f, 1f, 1f);
    private Vector3 magneta = new Vector3(1f, 0f, 1f);
    private Material brainMaterial;

    // Start is called before the first frame update
    void Start()
    {
        nodes = new List<CCFTreeNode>();

        modelControl.SetBeryl(true);
        modelControl.LateStart(false);

        StartCoroutine(DelayedColorChange(1f));

        Shader brainShader = Shader.Find("Shader Graphs/BrainRegionTransparent");
        brainMaterial = new Material(brainShader);
    }

    IEnumerator DelayedColorChange(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        // now change the color of the nodes

        // for now just do the full brain node (#8) for testing
        CCFTreeNode node = modelControl.tree.findNode(8);
        node.loadNodeModel(true);
        node.SetMaterial(brainMaterial);
        node.SetColor(Cool(0));
        nodes.Add(node);
    }

    private Color Cool(float perc)
    {
        Vector3 colorVector = Vector3.Lerp(teal, magneta, perc);
        return new Color(colorVector.x, colorVector.y, colorVector.z, 1f);
    }
}
