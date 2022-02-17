using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionStimulus : MonoBehaviour
{
    // we'll spawn points as our "moving dots" stimulus
    public GameObject pointPrefab;

    float xBound = 10f;
    float yBound = 10f;

    GameObject[] points;
    float[] lifetimes;
    float[] coherenceLifetimes;
    bool[] coherent;

    bool active;

    float motionSpeed;
    float motionCoherence;
    float motionCoherenceLifetime;
    float motionLifetime;

    float density; // dots per deg^2
    int dotDensityCount;
    
    // we'll use a circular aperture

    // Start is called before the first frame update
    void Start()
    {
        motionSpeed = 10f;
        motionCoherence = 1f;
        motionLifetime = 2f;
        motionCoherenceLifetime = 0.5f; // how long to keep coherent dots coherent

        density = 0.1f;

        dotDensityCount = Mathf.CeilToInt(xBound * yBound * density);

        points = new GameObject[dotDensityCount];
        lifetimes = new float[dotDensityCount];
        coherenceLifetimes = new float[dotDensityCount];
        coherent = new bool[dotDensityCount];

        for (int i=0;i<dotDensityCount;i++)
        {
            InstantiateNewPoint(i, true);
        }

        active = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            Vector3 coherentMovementUpdate = Vector3.forward * motionSpeed * Time.deltaTime;

            for (int i=0;i<dotDensityCount;i++)
            {
                if (lifetimes[i] >= motionLifetime)
                {
                    Destroy(points[i]);
                    InstantiateNewPoint(i, false);
                }
                else
                {
                    lifetimes[i] += Time.deltaTime;
                    coherenceLifetimes[i] += Time.deltaTime;

                    if (coherent[i])
                    {
                        points[i].transform.position += coherentMovementUpdate;
                    }
                    else
                    {
                        float angle = Mathf.PI * 2 * Random.value;
                        points[i].transform.position += new Vector3(0f, Mathf.Cos(angle), Mathf.Sin(angle)) * motionSpeed * Time.deltaTime;
                    }

                    if (coherenceLifetimes[i] >= motionCoherenceLifetime)
                    {
                        coherent[i] = Random.value < motionCoherence;
                        coherenceLifetimes[i] = 0;
                    }
                }
            }
        }
    }

    void InstantiateNewPoint(int idx, bool useRandomLifetime)
    {
        GameObject newPoint = Instantiate(pointPrefab, transform);
        newPoint.transform.position += new Vector3(0, Random.value * yBound * 2 - yBound, Random.value * xBound * 2 - xBound);
        points[idx] = newPoint;
        coherent[idx] = Random.value < motionCoherence;
        if (useRandomLifetime)
        {
            lifetimes[idx] = Random.value * motionLifetime;
        }
        else
        {
            lifetimes[idx] = 0;
        }
    }
}
