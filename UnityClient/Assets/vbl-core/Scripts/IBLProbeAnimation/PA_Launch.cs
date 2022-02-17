using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PA_Launch : MonoBehaviour
{
    [SerializeField] private CCFModelControl modelControl;
    [Range(-1,9), SerializeField] private int currentClosestLab;

    private List<int> previousLabs;

    private int previous;

    // Start is called before the first frame update
    void Start()
    {
        modelControl.LateStart(true);
        previousLabs = new List<int>();

        previous = currentClosestLab;
    }

    // Update is called once per frame
    void Update()
    {
        if (previous != currentClosestLab)
        {
            previousLabs.Add(previous);
            previous = currentClosestLab;
        }
    }
    public int GetCurrentLab()
    {
        return currentClosestLab;
    }

    public List<int> GetPreviousLabs()
    {
        return previousLabs;
    }
}
