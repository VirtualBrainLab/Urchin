using System;
using System.Collections.Generic;
using UnityEngine;
using BrainAtlas;
using UnityEngine.Events;

namespace Urchin.Managers
{
    public class AtlasManager : MonoBehaviour
    {
        #region Serialized fields
        #endregion

        #region static
        public static HashSet<OntologyNode> VisibleNodes { get; private set; }
        public static AtlasManager Instance;
        #endregion

        #region Data
        private Dictionary<int, List<float>> _areaData;
        private int _areaDataIndex;

        private Dictionary<int, (bool, bool)> _areaSides;
        //private int[] _missing = { 738, 995 };
        #endregion

        #region Events
        public UnityEvent<OntologyNode> NodeVisibleEvent;
        #endregion

        #region Unity
        private void Awake()
        {
            _areaSides = new();
            _areaData = new();
            VisibleNodes = new();

            if (Instance != null)
                throw new Exception("Only one AreaManager can exist in the scene!");
            Instance = this;
        }
        #endregion

        #region Public

        public void LoadAtlas(string atlasName)
        {
            switch (atlasName)
            {
                case "ccf25":
                    BrainAtlasManager.LoadAtlas(BrainAtlasManager.AtlasNames[0]);
                    break;
                case "waxholm39":
                    BrainAtlasManager.LoadAtlas(BrainAtlasManager.AtlasNames[1]);
                    break;
            }
        }

        public void SetAreaVisibility(Dictionary<string, bool> areaVisibility)
        {
            foreach (KeyValuePair<string, bool> kvp in areaVisibility)
            {
                (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);
                OntologyNode node = BrainAtlasManager.ActiveReferenceAtlas.Ontology.ID2Node(ID);

                if (node == null)
                    return;

                //if (_missing.Contains(node.ID))
                //{
                //    Client.LogWarning("The mesh file for area " + node.ID + " does not exist, we can't load it");
                //    continue;
                //}

                bool set = false;

                if (full && node.FullLoaded.IsCompleted)
                {
                    node.SetVisibility(kvp.Value, OntologyNode.OntologyNodeSide.Full);
                    VisibleNodes.Add(node);
                    set = true;
#if UNITY_EDITOR
                    Debug.Log("Setting full model visibility to true");
#endif
                }
                if (leftSide && node.SideLoaded.IsCompleted)
                {
                    node.SetVisibility(kvp.Value, OntologyNode.OntologyNodeSide.Left);
                    VisibleNodes.Add(node);
                    set = true;
#if UNITY_EDITOR
                    Debug.Log("Setting left model visibility to true");
#endif
                }
                if (rightSide && node.SideLoaded.IsCompleted)
                {
                    node.SetVisibility(kvp.Value, OntologyNode.OntologyNodeSide.Right);
                    VisibleNodes.Add(node);
                    set = true;
#if UNITY_EDITOR
                    Debug.Log("Setting right model visibility to true");
#endif
                }

                if (set)
                    NodeVisibleEvent.Invoke(node);
                else
                    LoadIndividualArea(node, full, leftSide, rightSide, kvp.Value);
            }
        }

        public async void SetAreaColor(Dictionary<string, string> areaColor)
        {
            foreach (KeyValuePair<string, string> kvp in areaColor)
            {
                (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);
                OntologyNode node = BrainAtlasManager.ActiveReferenceAtlas.Ontology.ID2Node(ID);

                Color newColor;
                if (node != null && ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
                {

                    if (full)
                    {
                        await node.FullLoaded;
                        node.SetColor(newColor, OntologyNode.OntologyNodeSide.Full);
                    }
                    else
                    {
                        await node.SideLoaded;
                        if (leftSide)
                            node.SetColor(newColor, OntologyNode.OntologyNodeSide.Left);
                        if (rightSide)
                            node.SetColor(newColor, OntologyNode.OntologyNodeSide.Right);
                    }
                }
                else
                    Debug.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
            }
        }

        public async void SetAreaMaterial(Dictionary<string, string> areaMaterial)
        {
            foreach (KeyValuePair<string, string> kvp in areaMaterial)
            {
                (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);

                OntologyNode node = BrainAtlasManager.ActiveReferenceAtlas.Ontology.ID2Node(ID);
                if (full)
                {
                    await node.FullLoaded;
                    node.SetMaterial(BrainAtlasManager.BrainRegionMaterials[kvp.Value], OntologyNode.OntologyNodeSide.Full);
                }
                else
                {
                    await node.SideLoaded;
                    if (leftSide)
                        node.SetMaterial(BrainAtlasManager.BrainRegionMaterials[kvp.Value], OntologyNode.OntologyNodeSide.Left);
                    if (rightSide)
                        node.SetMaterial(BrainAtlasManager.BrainRegionMaterials[kvp.Value], OntologyNode.OntologyNodeSide.Right);
                }
            }
        }

        public async void SetAreaAlpha(Dictionary<string, float> areaAlpha)
        {
            foreach (KeyValuePair<string, float> kvp in areaAlpha)
            {
                (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);

                OntologyNode node = BrainAtlasManager.ActiveReferenceAtlas.Ontology.ID2Node(ID);
                if (full)
                {
                    await node.FullLoaded;
                    node.SetShaderProperty("_Alpha", kvp.Value, OntologyNode.OntologyNodeSide.Full);
                }
                else
                {
                    await node.SideLoaded;
                    if (leftSide)
                        node.SetShaderProperty("_Alpha", kvp.Value, OntologyNode.OntologyNodeSide.Left);
                    if (rightSide)
                        node.SetShaderProperty("_Alpha", kvp.Value, OntologyNode.OntologyNodeSide.Right);
                }

            }
        }

        // Area colormaps
        public void SetAreaColormap(string colormapName)
        {
            throw new NotImplementedException();
            //_main.ChangeColormap(colormapName);
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
            throw new NotImplementedException();
            //foreach (KeyValuePair<string, float> kvp in areaIntensity)
            //{
            //    (int ID, bool full, bool leftSide, bool rightSide) = GetID(kvp.Key);
            //    CCFTreeNode node = _modelControl.tree.findNode(ID);

            //    if (node != null)
            //    {
            //        if (WaitingOnTask(node.ID))
            //            await _nodeTasks[node.ID];

            //        if (full)
            //        {
            //            if (!_main.colorLeftOnly)
            //                node.SetColor(_main.GetColormapColor(kvp.Value), true);
            //            else
            //                node.SetColorOneSided(_main.GetColormapColor(kvp.Value), true, true);
            //        }
            //        else if (leftSide)
            //            node.SetColorOneSided(_main.GetColormapColor(kvp.Value), true, true);
            //        else if (rightSide && !_main.colorLeftOnly)
            //            node.SetColorOneSided(_main.GetColormapColor(kvp.Value), false, true);
            //    }
            //    else
            //        Debug.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
            //}
        }

        // Auto-loaders
        public async void LoadDefaultAreas(string defaultName)
        {
            throw new NotImplementedException();
            //Task<List<CCFTreeNode>> nodeTask;
            //if (defaultName.Equals("cosmos"))
            //    nodeTask = _modelControl.LoadCosmosNodes(false);
            //else if (defaultName.Equals("beryl"))
            //    nodeTask = _modelControl.LoadBerylNodes(false);
            //else
            //{
            //    Debug.Log("Failed to load nodes: " + defaultName);
            //    Client.LogError("Node group " + defaultName + " does not exist.");
            //    return;
            //}

            //await nodeTask;

            //foreach (CCFTreeNode node in nodeTask.Result)
            //{
            //    node.SetNodeModelVisibility_Left(true);
            //    node.SetNodeModelVisibility_Right(true);
            //    VisibleNodes.Add(node);
            //    _main.RegisterNode(node);
            //}
        }

        #endregion

        #region Public helpers
        public void ClearAreas()
        {
            Debug.Log("(Client) Clearing areas");
            foreach (OntologyNode node in VisibleNodes)
                node.SetVisibility(false, OntologyNode.OntologyNodeSide.All);
            VisibleNodes.Clear();
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
            if (lower.Equals("void"))
                return (-1, full, leftSide, rightSide);

            // Figure out what the acronym was by asking CCFModelControl
            Debug.Log(idOrAcronym);
            try
            {
                int ID = BrainAtlasManager.ActiveReferenceAtlas.Ontology.Acronym2ID(idOrAcronym);
                return (ID, full, leftSide, rightSide);
            }
            catch
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

        private async void LoadIndividualArea(OntologyNode node, bool full, bool leftSide, bool rightSide, bool visibility)
        {
#if UNITY_EDITOR
            Debug.Log("Loading model");
#endif
            VisibleNodes.Add(node);

            await node.LoadMesh(OntologyNode.OntologyNodeSide.All);

            if (full)
                node.SetVisibility(visibility, OntologyNode.OntologyNodeSide.Full);

            await node.SideLoaded;
            if (leftSide)
                node.SetVisibility(visibility, OntologyNode.OntologyNodeSide.Left);
            if (rightSide)
                node.SetVisibility(visibility, OntologyNode.OntologyNodeSide.Right);

            NodeVisibleEvent.Invoke(node);
        }

        private async void UpdateAreaDataIntensity()
        {
            throw new NotImplementedException();
            //foreach (KeyValuePair<int, List<float>> kvp in _areaData)
            //{
            //    int ID = kvp.Key;
            //    (bool leftSide, bool rightSide) = _areaSides[ID];

            //    CCFTreeNode node = _modelControl.tree.findNode(ID);

            //    if (WaitingOnTask(node.ID))
            //        await _nodeTasks[node.ID];

            //    float currentValue = kvp.Value[_areaDataIndex];

            //    if (node != null)
            //    {
            //        if (leftSide && rightSide)
            //            node.SetColor(_main.GetColormapColor(currentValue), true);
            //        else if (leftSide)
            //            node.SetColorOneSided(_main.GetColormapColor(currentValue), true, true);
            //        else if (rightSide)
            //            node.SetColorOneSided(_main.GetColormapColor(currentValue), false, true);
            //    }
            //    else
            //        Debug.Log("Failed to set " + kvp.Key + " to " + kvp.Value);
            //}
        }

        #endregion
    }
}