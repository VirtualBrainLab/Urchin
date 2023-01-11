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

    }

    public void DeleteLine(List<string> lines) { 
    
        //calls destroy (the one specific line)
    }

    public void SetLinePosition(Dictionary<string, List<Vector3>> linePositions)
    {
        //set line renderer position
    }
    public void SetLineColor(Dictionary<string, List<Color>> lineColors)
    {

    }
}
