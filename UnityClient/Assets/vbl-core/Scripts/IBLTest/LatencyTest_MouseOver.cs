using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LatencyTest_MouseOver : MonoBehaviour
{
    private CCFTreeNode node;
    private GameObject tooltipPanelGO;
    private bool hasControl;

    Vector3 offset = new Vector3(75f, 20f);

    // Start is called before the first frame update
    private void OnMouseOver()
    {
        if (gameObject.activeSelf)
        {
            hasControl = true;
            tooltipPanelGO.SetActive(true);
            tooltipPanelGO.transform.position = Input.mousePosition + offset;
            tooltipPanelGO.GetComponentInChildren<TextMeshProUGUI>().text = node.ShortName;
        }
    }

    private void OnMouseExit()
    {
        if (hasControl && gameObject.activeSelf)
        {
            hasControl = false;
            tooltipPanelGO.SetActive(false);
        }
    }

    public void SetNode(CCFTreeNode node)
    {
        this.node = node;
    }

    public void SetTooltip(GameObject tooltipPanel)
    {
        tooltipPanelGO = tooltipPanel;
    }
}
