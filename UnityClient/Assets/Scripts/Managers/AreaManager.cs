using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class AreaManager : MonoBehaviour
{
    #region Serialized fields
    [SerializeField] private CCFModelControl _modelControl;
    [SerializeField] private UM_Launch _main;
    #endregion

    #region static
    public static HashSet<CCFTreeNode> VisibleNodes { get; private set; }
    #endregion

    private Dictionary<int, Task> _nodeTasks;
    private Dictionary<int, (bool, bool)> _areaSides;
    private int[] _missing = { 738, 995 };

    #region Data
    private Dictionary<int, List<float>> _areaData;
    private int _areaDataIndex;
    #endregion

    #region Unity
    private void Awake()
    {
        _nodeTasks = new Dictionary<int, Task>();
        _areaSides = new();
        _areaData = new();
        VisibleNodes = new();
    }
    #endregion

    #region Public

    public void SetAreaVisibility(Dictionary<string, bool> areaVisibility)
    {
        foreach (KeyValuePair<string, bool> kvp in areaVisibility)
        {
            (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);
            CCFTreeNode node = _modelControl.tree.findNode(ID);

            if (node == null)
                return;

            if (_missing.Contains(node.ID))
            {
                Client.LogWarning("The mesh file for area " + node.ID + " does not exist, we can't load it");
                continue;
            }
            if (_nodeTasks.ContainsKey(node.ID))
            {
                UM_Launch.Log("Node " + node.ID + " is already being loaded, did you send duplicate instructions?");
                continue;
            }

            bool set = false;

            if (full && node.IsLoaded(true))
            {
                node.SetNodeModelVisibility_Full(kvp.Value);
                _main.RegisterNode(node);
                VisibleNodes.Add(node);
                set = true;
#if UNITY_EDITOR
                Debug.Log("Setting full model visibility to true");
#endif
            }
            if (leftSide && node.IsLoaded(false))
            {
                node.SetNodeModelVisibility_Left(kvp.Value);
                _main.RegisterNode(node);
                VisibleNodes.Add(node);
                set = true;
#if UNITY_EDITOR
                Debug.Log("Setting left model visibility to true");
#endif
            }
            if (rightSide && node.IsLoaded(false))
            {
                node.SetNodeModelVisibility_Right(kvp.Value);
                _main.RegisterNode(node);
                VisibleNodes.Add(node);
                set = true;
#if UNITY_EDITOR
                Debug.Log("Setting right model visibility to true");
#endif
            }

            if (!set)
                LoadIndividualArea(ID, full, leftSide, rightSide, kvp.Value);
        }
    }

    public async void SetAreaColor(Dictionary<string, string> areaColor)
    {
        foreach (KeyValuePair<string, string> kvp in areaColor)
        {
            (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);
            CCFTreeNode node = _modelControl.tree.findNode(ID);

            Color newColor = Color.black;
            if (node != null && ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
            {
                if (WaitingOnTask(node.ID))
                    await _nodeTasks[node.ID];

                if (full)
                {
                    if (!_main.colorLeftOnly)
                        node.SetColor(newColor, true);
                    else
                        node.SetColorOneSided(newColor, true, true);
                }
                else if (leftSide)
                    node.SetColorOneSided(newColor, true, true);
                else if (rightSide && !_main.colorLeftOnly)
                    node.SetColorOneSided(newColor, false, true);
            }
            else
                UM_Launch.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

    public async void SetAreaMaterial(Dictionary<string, string> areaMaterial)
    {
        foreach (KeyValuePair<string, string> kvp in areaMaterial)
        {
            (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);
            if (WaitingOnTask(ID))
                await _nodeTasks[ID];

            if (full)
                _modelControl.ChangeMaterial(ID, kvp.Value);
            else if (leftSide)
                _modelControl.ChangeMaterialOneSided(ID, kvp.Value, true);
            else if (rightSide)
                _modelControl.ChangeMaterialOneSided(ID, kvp.Value, false);
        }
    }

    public async void SetAreaAlpha(Dictionary<string, float> areaAlpha)
    {
        foreach (KeyValuePair<string, float> kvp in areaAlpha)
        {
            (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);
            CCFTreeNode node = _modelControl.tree.findNode(ID);

            if (node != null)
            {
                if (WaitingOnTask(node.ID))
                    await _nodeTasks[node.ID];

                if (full)
                    node.SetShaderProperty("_Alpha", kvp.Value);
                else if (leftSide)
                    node.SetShaderPropertyOneSided("_Alpha", kvp.Value, true);
                else if (rightSide)
                    node.SetShaderPropertyOneSided("_Alpha", kvp.Value, false);
            }
            else
                UM_Launch.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

    // Area colormaps
    public void SetAreaColormap(string colormapName)
    {
        _main.ChangeColormap(colormapName);
    }


    // Area data
    public void SetAreaData(Dictionary<string, List<float>> areaData)
    {
        foreach (KeyValuePair<string, List<float>> kvp in areaData)
        {
            (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);

            Debug.LogWarning("Might be broken with new data loading");
            if (_areaData.ContainsKey(ID))
            {
                _areaData[ID] = kvp.Value;
                _areaSides[ID] = (leftSide, rightSide);
            }
            else
            {
                _areaData.Add(ID, kvp.Value);
                _areaSides.Add(ID, (leftSide, rightSide));
            }
        }
    }

    public void SetAreaDataIndex(int areaDataIdx)
    {
        _areaDataIndex = areaDataIdx;
        UpdateAreaDataIntensity();
    }

    // Area intensity colormaps
    public async void SetAreaColorIntensity(Dictionary<string, float> areaIntensity)
    {
        foreach (KeyValuePair<string, float> kvp in areaIntensity)
        {
            (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);
            CCFTreeNode node = _modelControl.tree.findNode(ID);

            if (node != null)
            {
                if (WaitingOnTask(node.ID))
                    await _nodeTasks[node.ID];

                if (full)
                {
                    if (!_main.colorLeftOnly)
                        node.SetColor(_main.GetColormapColor(kvp.Value), true);
                    else
                        node.SetColorOneSided(_main.GetColormapColor(kvp.Value), true, true);
                }
                else if (leftSide)
                    node.SetColorOneSided(_main.GetColormapColor(kvp.Value), true, true);
                else if (rightSide && !_main.colorLeftOnly)
                    node.SetColorOneSided(_main.GetColormapColor(kvp.Value), false, true);
            }
            else
                UM_Launch.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

    // Auto-loaders
    public async void LoadDefaultAreas(string defaultName)
    {
        Task<List<CCFTreeNode>> nodeTask;
        if (defaultName.Equals("cosmos"))
            nodeTask = _modelControl.LoadCosmosNodes(false);
        else if (defaultName.Equals("beryl"))
            nodeTask = _modelControl.LoadBerylNodes(false);
        else
        {
            UM_Launch.Log("Failed to load nodes: " + defaultName);
            Client.LogError("Node group " + defaultName + " does not exist.");
            return;
        }

        await nodeTask;

        foreach (CCFTreeNode node in nodeTask.Result)
        {
            node.SetNodeModelVisibility_Left(true);
            node.SetNodeModelVisibility_Right(true);
            VisibleNodes.Add(node);
            _main.RegisterNode(node);
        }
    }

    #endregion

    #region Public helpers
    public void ClearAreas()
    {
        Debug.Log("(Client) Clearing areas");
        foreach (CCFTreeNode node in VisibleNodes)
        {
            Debug.Log("Clearing: " + node.Name);
            node.SetNodeModelVisibility_Full(false);
            node.SetNodeModelVisibility_Left(false);
            node.SetNodeModelVisibility_Right(false);
        }
        VisibleNodes = new();
    }

    /// <summary>
    /// Convert an acronym or ID label into the Allen CCF area ID
    /// </summary>
    /// <param name="idOrAcronym">An ID number (e.g. 0) or an acronym (e.g. "root")</param>
    /// <returns>(int Allen CCF ID, bool left side model, bool right side model)</returns>
    public (int ID, bool full, bool leftSide, bool rightSide) GetID(string idOrAcronym)
    {
        // Check whether a suffix was included
        int leftIndex = idOrAcronym.IndexOf("-lh");
        int rightIndex = idOrAcronym.IndexOf("-rh");
        bool leftSide = leftIndex > 0;
        bool rightSide = rightIndex > 0;
        bool full = !(leftSide || rightSide);

        //Remove the suffix
        if (leftSide)
            idOrAcronym = idOrAcronym.Substring(0, leftIndex);
        if (rightSide)
            idOrAcronym = idOrAcronym.Substring(0, rightIndex);

        // Lowercase
        string lower = idOrAcronym.ToLower();

        // Check for root (special case, which we can't currently handle)
        if (lower.Equals("root") || lower.Equals("void"))
            return (-1, full, leftSide, rightSide);

        // Figure out what the acronym was by asking CCFModelControl
        if (_modelControl.IsAcronym(idOrAcronym))
            return (_modelControl.Acronym2ID(idOrAcronym), full, leftSide, rightSide);
        else
        {
            // It wasn't an acronym, so it has to be an integer
            int ret;
            if (int.TryParse(idOrAcronym, out ret))
                return (ret, full, leftSide, rightSide);
        }

        // We failed to figure out what this was
        return (-1, full, leftSide, rightSide);
    }

    #endregion

    #region Private helpers

    private async void LoadIndividualArea(int ID, bool full, bool leftSide, bool rightSide, bool visibility)
    {
#if UNITY_EDITOR
        Debug.Log("Loading model");
#endif
        CCFTreeNode node = _modelControl.tree.findNode(ID);
        VisibleNodes.Add(node);

        Debug.Log((full, leftSide, rightSide));
        node.LoadNodeModel(full, leftSide || rightSide);

        await node.GetLoadedTask(full);

        _main.RegisterNode(node);

        if (full)
            node.SetNodeModelVisibility_Full(visibility);
        if (leftSide)
            node.SetNodeModelVisibility_Left(visibility);
        if (rightSide)
            node.SetNodeModelVisibility_Right(visibility);
    }

    private async void UpdateAreaDataIntensity()
    {
        foreach (KeyValuePair<int, List<float>> kvp in _areaData)
        {
            int ID = kvp.Key;
            (bool leftSide, bool rightSide) = _areaSides[ID];

            CCFTreeNode node = _modelControl.tree.findNode(ID);

            if (WaitingOnTask(node.ID))
                await _nodeTasks[node.ID];

            float currentValue = kvp.Value[_areaDataIndex];

            if (node != null)
            {
                if (leftSide && rightSide)
                    node.SetColor(_main.GetColormapColor(currentValue), true);
                else if (leftSide)
                    node.SetColorOneSided(_main.GetColormapColor(currentValue), true, true);
                else if (rightSide)
                    node.SetColorOneSided(_main.GetColormapColor(currentValue), false, true);
            }
            else
                UM_Launch.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
        }
    }

    /// <summary>
    /// Check whether a brain area is still actively being loaded
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private bool WaitingOnTask(int id)
    {
        if (_nodeTasks.ContainsKey(id))
        {
            if (_nodeTasks[id].IsCompleted || _nodeTasks[id].IsFaulted || _nodeTasks[id].IsCanceled)
            {
                _nodeTasks.Remove(id);
                return false;
            }
            return true;
        }
        return false;
    }

    #endregion
}
