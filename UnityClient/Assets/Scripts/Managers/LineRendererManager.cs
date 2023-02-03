using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class LineRendererManager : MonoBehaviour
{
    //Keep a dictionary that maps string names to line renderer components 
    private Dictionary<string, LineRenderer> _lineRenderers;
    [SerializeField] private GameObject _lineRendererPrefab;

    private void Awake()
    {
        _lineRenderers = new();
    }

    private void Start()
    {
       /* CreateLine(new List<string> { "l1", "l2" });
        DeleteLine(new List<string> { "l2" });

        Dictionary<string, List<Vector3>> temp = new();
        List<Vector3> tempPositions = new();
        tempPositions.Add(new Vector3(0, 0, 0));
        tempPositions.Add(new Vector3(1,2,3));
        tempPositions.Add(new Vector3(4, 4, 4));
        temp.Add("l1", tempPositions);

        SetLinePosition(temp);


        SetLineColor("l1", Color.blue);*/
    }

    public void CreateLine(List<string> lines)
    {
        //instantiating game object w line renderer component
        foreach(string line in lines)
        {
            GameObject tempObject = Instantiate(_lineRendererPrefab);
            tempObject.name = $"lineRenderer_{line}";
            _lineRenderers.Add(line, tempObject.GetComponent<LineRenderer>());
            //in theory, creates new entry to the dictionary with the name of the line [line] and associates it with a new Game Object
            
            //adds the line renderer component to the line renderer manager (actually creates the line of the empty object)
        }

    }

    public void DeleteLine(List<string> lines) { 
    
        //calls destroy (the one specific line)
        foreach(string line in lines)
        {
            Destroy(_lineRenderers[line].gameObject);
            _lineRenderers.Remove(line);
        }
    }

    public void SetLinePosition(Dictionary<string, List<List<float>>> linePositions)
    {
        //set line renderer position
        Debug.Log("function got called!");

        foreach (string lineName in linePositions.Keys) {// running through whole dictionary:
            List<List<float>> data = linePositions[lineName];
            Vector3[] posvectors = new Vector3[data.Count];
            
            for(int i= 0; i<data.Count; i++)
            {
                List<float> position = data[i];
                Vector3 tempVector = new Vector3(position[0], position[1], position[2]);
                posvectors[i] = tempVector;
            }
            LineRenderer tempLine = _lineRenderers[lineName];
            tempLine.positionCount = linePositions[lineName].Count;
            tempLine.SetPositions(posvectors);
        }
    }
    public void SetLineColor(Dictionary<string, string> lineColors)
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

    public void SetLineColor (string lineName, Color color)
    {
        LineRenderer tempLine = _lineRenderers[lineName];
        tempLine.material.color = color;
    }
}
