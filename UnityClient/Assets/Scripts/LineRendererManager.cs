using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererManager : MonoBehaviour
{
    //Keep a dictionary that maps string names to line renderer components 
    private Dictionary<string, LineRenderer> _lineRenderers;

    private void Awake()
    {
        _lineRenderers = new();
    }


    public void CreateLine(List<string> lines)
    {
        //instantiating game object w line renderer component
        foreach(string line in lines)
        {
            _lineRenderers.Add(line, new GameObject());
            //in theory, creates new entry to the dictionary with the name of the line [line] and associates it with a new Game Object
            _lineRenderers[line].AddComponent(LineRendererManager)
            //adds the line renderer component to the line renderer manager (actually creates the line of the empty object)
        }

    }

    public void DeleteLine(List<string> lines) { 
    
        //calls destroy (the one specific line)
        foreach(string line in lines)
        {
            Destroy(_lineRenderers[line]);
        }
    }

    public void SetLinePosition(Dictionary<string, List<Vector3>> linePositions)
    {
        //set line renderer position
        _lineRenderers[linePositions.Keys].SetPosition(linePositions.Values);
    }
    public void SetLineColor(Dictionary<string, List<Color>> lineColors)
    {
        _lineRenderers[lineColors.Keys].SetColors(lineColors.Values);
    }
}
