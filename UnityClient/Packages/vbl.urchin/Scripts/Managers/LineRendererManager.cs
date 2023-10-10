using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Urchin.API;

namespace Urchin.Managers
{
    public class LineRendererManager : MonoBehaviour
    {
        #region Public
        [SerializeField] private GameObject _lineRendererPrefabGO;
        [SerializeField] private Transform _lineRendererParentT;
        #endregion

        #region Private

        #endregion
        //Keep a dictionary that maps string names to line renderer components 
        private Dictionary<string, LineRenderer> _lineRenderers;

        private void Awake()
        {
            _lineRenderers = new();
        }

        

        private void Start()
        {
            Client_SocketIO.CreateLine += Create;
            Client_SocketIO.SetLinePosition += SetPosition;
            Client_SocketIO.DeleteLine += Delete;
            Client_SocketIO.SetLineColor += SetColors;
        }

        public void Create(List<string> lines)
        {
            //instantiating game object w line renderer component
            foreach (string line in lines)
            {
                GameObject tempObject = Instantiate(_lineRendererPrefabGO, _lineRendererParentT);
                tempObject.name = $"lineRenderer_{line}";
                _lineRenderers.Add(line, tempObject.GetComponent<LineRenderer>());
                //in theory, creates new entry to the dictionary with the name of the line [line] and associates it with a new Game Object

                //adds the line renderer component to the line renderer manager (actually creates the line of the empty object)
            }

        }

        public void Delete(List<string> lines)
        {

            //calls destroy (the one specific line)
            foreach (string line in lines)
            {
                Destroy(_lineRenderers[line].gameObject);
                _lineRenderers.Remove(line);
            }
        }

        public void SetPosition(Dictionary<string, List<List<float>>> linePositions)
        {
            //set line renderer position
            Debug.Log("function got called!");

            foreach (string lineName in linePositions.Keys)
            {// running through whole dictionary:
                List<List<float>> data = linePositions[lineName];
                Vector3[] posvectors = new Vector3[data.Count];

                for (int i = 0; i < data.Count; i++)
                {
                    List<float> position = data[i];
                    posvectors[i] = new Vector3(-position[0] / 1000f, -position[2] / 1000f, position[1] / 1000f) + _lineRendererParentT.position;
                }
                LineRenderer tempLine = _lineRenderers[lineName];
                tempLine.positionCount = linePositions[lineName].Count;
                tempLine.SetPositions(posvectors);
            }
        }
        public void SetColors(Dictionary<string, string> lineColors)
        {
            foreach (string lineName in lineColors.Keys)
            {
                Color newColor;
                if (ColorUtility.TryParseHtmlString(lineColors[lineName], out newColor))
                {
                    Debug.Log($"Setting Color of {lineName} to {lineColors[lineName]}");
                    SetLineColor(lineName, newColor);
                }
                else
                {
                    Debug.Log($"Color {lineColors[lineName]} does not exist.");
                }

            }
        }

        public void SetLineColor(string lineName, Color color)
        {
            LineRenderer tempLine = _lineRenderers[lineName];
            tempLine.material.color = color;
        }
    }
}