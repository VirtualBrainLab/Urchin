using System;
using System.Collections.Generic;
using UnityEngine;

public class CCFTree
{
    public CCFTreeNode root;
    private Material brainRegionMaterial;
    private float scale;
    private Dictionary<int, CCFTreeNode> fastSearchDictionary;

    public CCFTree(int rootID, int atlasID, string rootName, float scale, Color color, Material material)
    {
        this.scale = scale;
        root = new CCFTreeNode(rootID, atlasID, 0, scale, null, rootName, "", color, material);
        brainRegionMaterial = material;

        fastSearchDictionary = new Dictionary<int, CCFTreeNode>();
        fastSearchDictionary.Add(rootID, root);
    }

    public CCFTreeNode addNode(int parentID, int id, int atlasID, int depth, string name, string acronym, Color color)
    {
        // find the parent ID node
        CCFTreeNode parentNode = findNode(parentID);

        // return if you fail to find it
        if (parentNode==null) {Debug.Log("Can't add new node: parent not found");return null;}

        // add the node if you succeeded
        CCFTreeNode newNode = new CCFTreeNode(id, atlasID, depth, scale, parentNode, name, acronym, color, brainRegionMaterial);
        parentNode.appendNode(newNode);

        fastSearchDictionary.Add(id, newNode);

        return newNode;
    }

    public int nodeCount()
    {
        return root.nodeCount();
    }

    public CCFTreeNode findNode(int ID)
    {
        return fastSearchDictionary[ID];
    }

    [Obsolete("Deprecated in favor of findNode with dictionary")]
    public CCFTreeNode findNodeRecursive(int ID)
    {
        return root.findNode(ID);
    }
}

public class CCFTreeNode
{
    private CCFTreeNode parent;
    private List<CCFTreeNode> childNodes;
    public int ID { get;}
    public int atlasID { get; }
    public string Name { get; }
    public string ShortName { get; }
    public int Depth { get; }
    private Color color;
    private float scale;

    private GameObject ontologyToggle;

    private bool singleModel;
    private GameObject nodeModelGO;
    private Vector3 nodeMeshCenter;
    private Vector3 originalPosition;

    private GameObject nodeModelLeftGO;
    private GameObject nodeModelRightGO;
    Vector3 nodeMeshCenterLeft;
    Vector3 nodeMeshCenterRight;
    Vector3 originalPositionLeft;
    Vector3 originalPositionRight;

    private GameObject brainModelParent;
    private Material material;


    private Vector3 explodeScale = new Vector3(1f, 1f, 1f);

    private bool loaded;

    // Mesh properties
    // each mesh has a left and right half, we want to separate these
    Mesh localMesh;

    public CCFTreeNode(int ID, int atlasID, int depth, float scale, CCFTreeNode parent, string Name, string ShortName, Color color, Material material)
    {
        this.ID = ID;
        this.atlasID = atlasID;
        this.Name = Name;
        this.parent = parent;
        this.Depth = depth;
        this.scale = scale;
        this.ShortName = ShortName;
        color.a = 1.0f;
        this.color = color;
        this.material = material;
        childNodes = new List<CCFTreeNode>();
        brainModelParent = GameObject.Find("BrainModel");

        loaded = false;
    }

    public bool IsLoaded()
    {
        return loaded;
    }

    public GameObject loadNodeModel(bool loadSeparatedModels)
    {
        singleModel = !loadSeparatedModels;

        nodeModelGO = new GameObject(Name);
        nodeModelGO.transform.parent = brainModelParent.transform;


        Mesh fullMesh = (loadSeparatedModels) ? Resources.Load<Mesh>("AllenCCF/" + this.ID + "L") : Resources.Load<Mesh>("AllenCCF/" + this.ID);
        // Copy the mesh so that we can modify it without modifying the original
        localMesh = new Mesh();
        localMesh.vertices = fullMesh.vertices;
        localMesh.triangles = fullMesh.triangles;
        //localMesh.uv = fullMesh.uv;
        localMesh.normals = fullMesh.normals;
        //localMesh.colors = fullMesh.colors;
        localMesh.tangents = fullMesh.tangents;

        if (loadSeparatedModels)
        {
            // Create the left/right meshes
            nodeModelLeftGO = new GameObject(Name + "_L");
            nodeModelLeftGO.transform.parent = nodeModelGO.transform;
            nodeModelLeftGO.transform.localScale = new Vector3(scale, scale, scale);
            nodeModelLeftGO.transform.Translate(5.7f, 4f, -6.6f);
            nodeModelLeftGO.transform.Rotate(0f, -90f, -180f);
            nodeModelLeftGO.AddComponent<MeshFilter>();
            nodeModelLeftGO.AddComponent<MeshRenderer>();
            nodeModelLeftGO.layer = 13;
            nodeModelLeftGO.tag = "BrainRegion";
            Renderer leftRend = nodeModelLeftGO.GetComponent<Renderer>();
            leftRend.material = material;
            leftRend.material.SetColor("_Color", color);
            leftRend.receiveShadows = false;
            leftRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            nodeModelLeftGO.GetComponent<MeshFilter>().mesh = localMesh;

            nodeMeshCenterLeft = leftRend.bounds.center;
            originalPositionLeft = nodeModelLeftGO.transform.localPosition;

            // Create the right meshes
            nodeModelRightGO = new GameObject(Name + "_R");
            nodeModelRightGO.transform.parent = nodeModelGO.transform;
            nodeModelRightGO.transform.localScale = new Vector3(scale, scale, -scale);
            nodeModelRightGO.transform.Translate(-5.7f, 4f, -6.6f);
            nodeModelRightGO.transform.Rotate(0f, -90f, -180f);
            nodeModelRightGO.AddComponent<MeshFilter>();
            nodeModelRightGO.AddComponent<MeshRenderer>();
            nodeModelRightGO.layer = 13;
            nodeModelRightGO.tag = "BrainRegion";
            Renderer rightRend = nodeModelRightGO.GetComponent<Renderer>();
            rightRend.material = material;
            rightRend.material.SetColor("_Color", color);
            rightRend.receiveShadows = false;
            rightRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            nodeModelRightGO.GetComponent<MeshFilter>().mesh = localMesh;

            nodeMeshCenterRight = rightRend.bounds.center;
            originalPositionRight = nodeModelRightGO.transform.localPosition;
        }
        else
        {
            nodeModelGO.transform.localScale = new Vector3(scale, scale, scale);
            nodeModelGO.transform.Translate(5.7f, 4f, -6.6f);
            nodeModelGO.transform.Rotate(0f, -90f, -180f);
            originalPosition = nodeModelGO.transform.localPosition;
            nodeModelGO.AddComponent<MeshFilter>();
            nodeModelGO.AddComponent<MeshRenderer>();
            nodeModelGO.layer = 13;
            nodeModelGO.tag = "BrainRegion";
            Renderer rend = nodeModelGO.GetComponent<Renderer>();
            rend.material = material;
            rend.material.SetColor("_Color", color);
            rend.receiveShadows = false;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            nodeModelGO.GetComponent<MeshFilter>().mesh = localMesh;

            nodeMeshCenter = rend.bounds.center;
        }



        // [TODO] remove these lines adding a collider and script --
        // they're here for transparency demo purposes atm
        //nodeModelGO.AddComponent<MeshCollider>();
        //nodeModelGO.AddComponent<BrainRegionSelector>();

        loaded = true;
        return nodeModelGO;
    }

    public void ExplodeModel(Vector3 center, float percentage)
    {
        if (singleModel)
            nodeModelGO.transform.localPosition = originalPosition + Vector3.Scale(nodeMeshCenter - center, explodeScale) * percentage;
        else
        {
            Vector3 leftDistance = nodeMeshCenterLeft - center;
            Debug.DrawRay(center, leftDistance * 10f);
            leftDistance = Vector3.Scale(leftDistance, leftDistance);
            nodeModelLeftGO.transform.localPosition = originalPositionLeft + Vector3.Scale(leftDistance, explodeScale) * percentage;
            nodeModelRightGO.transform.localPosition = originalPositionRight + Vector3.Scale(nodeMeshCenterRight - center, explodeScale) * percentage;
        }
    }

    public Color GetColor()
    {
        return color;
    }

    public void SetColor(Color newColor)
    {
        if (!loaded)
        {
            Debug.LogError("Node model needs to be loaded before color can be set");
            return;
        }
        this.color = newColor;
        if (singleModel)
            nodeModelGO.GetComponent<Renderer>().material.SetColor("_Color", color);
        else
        {
            nodeModelLeftGO.GetComponent<Renderer>().material.SetColor("_Color", color);
            nodeModelRightGO.GetComponent<Renderer>().material.SetColor("_Color", color);
        }
    }

    public void SetMaterial(Material newMaterial)
    {
        if (!loaded)
        {
            Debug.LogError("Node model needs to be loaded before material can be set");
            return;
        }
        this.material = newMaterial;
        if (singleModel)
            nodeModelGO.GetComponent<Renderer>().material = newMaterial;
        else
        {
            nodeModelLeftGO.GetComponent<Renderer>().material = newMaterial;
            nodeModelRightGO.GetComponent<Renderer>().material = newMaterial;
        }

        SetColor(color);
    }

    public void SetNodeModelVisibility(bool visible)
    {
        if (singleModel)
        {
            nodeModelGO.SetActive(visible);
        }
        else
        {
            nodeModelLeftGO.SetActive(visible);
            nodeModelRightGO.SetActive(visible);
        }
    }

    public void SetNodeModelVisibility(bool leftVisible, bool rightVisible)
    {
        if (singleModel)
        {
            Debug.LogWarning("Node model visibility cannot be set separately when running in single model mode.");
        }
        else
        {
            nodeModelLeftGO.SetActive(leftVisible);
            nodeModelRightGO.SetActive(rightVisible);
        }
    }

    //public int buildToggleContent(GameObject toggleContentPanel, GameObject togglePrefab, int[] regionIDs, int curCount)
    //{
    //    // Instantiate this toggle if it's ID is in regionIDs
    //    if (Array.IndexOf(regionIDs, this.ID) > -1) {
    //        // Build my toggle content
    //        GameObject toggleElement = GameObject.Instantiate(togglePrefab, toggleContentPanel.transform);
    //        // Set the background color
    //        toggleElement.GetComponent<Graphic>().color = color;
    //        // Set the area name
    //        Text label = toggleElement.transform.Find("Label").GetComponent<Text>();
    //        label.text = Name;
    //        curCount++;
    //    }
    //    foreach (CCFTreeNode node in childNodes)
    //    {
    //        curCount = node.buildToggleContent(toggleContentPanel, togglePrefab, regionIDs, curCount);
    //    }
    //    return curCount;
    //}

    public int nodeCount()
    {
        int count = childNodes.Count;
        foreach (CCFTreeNode node in childNodes)
        {
            count += node.nodeCount();
        }
        return count;
    }

    public CCFTreeNode findNode(int ID)
    {
        if (this.ID == ID) { return this; }
        foreach (CCFTreeNode node in childNodes)
        {
            CCFTreeNode found = node.findNode(ID);
            if (found != null) { return found; }
        }
        return null;
    }

    public CCFTreeNode Parent()
    {
        return parent;
    }

    public List<CCFTreeNode> Nodes()
    {
        return childNodes;
    }

    public void appendNode(CCFTreeNode newNode)
    {
        childNodes.Add(newNode);
    }

    public void DebugPrint()
    {
        Debug.Log(this.ID);
        Debug.Log(this.color);
        Debug.Log(this.Name);
    }

    public Transform GetNodeTransform()
    {
        return nodeModelGO.transform;
    }

    public Vector3 GetMeshCenter()
    {
        return nodeMeshCenter;
    }

    public GameObject MainGameObject()
    {
        return nodeModelGO;
    }
    public GameObject LeftGameObject()
    {
        return nodeModelLeftGO;
    }
    public GameObject RightGameObject()
    {
        return nodeModelRightGO;
    }
}