using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCameraLookAt : MonoBehaviour
{
    [SerializeField] private Transform mouse;

    // Start is called before the first frame update
    void Start()
    {
        transform.LookAt(mouse);
        transform.RotateAround(transform.position, transform.right, 90);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
