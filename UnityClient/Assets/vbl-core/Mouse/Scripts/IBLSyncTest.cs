using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBLSyncTest : MonoBehaviour
{
    public Transform leftPawTransform;
    public Transform rightPawTransform;

    private int time;
    private int timePoints;

    private float[,] pawL;
    private float[,] pawR;

    private Vector3 pawLOffset;
    private Vector3 pawROffset;

    int wait = 120;
    int frame60hz;
    float last60hzframe;

    float scale = 0.002f; // the mouse model + rig are scaled up by 1000x and 10x 

    bool active = false;

    // Start is called before the first frame update
    void Start()
    {
        frame60hz = 1;

        List<Dictionary<string, object>> data = CSVReader.Read("IBLSyncTest/points_3d");

        timePoints = data.Count;
        pawL = new float[data.Count, 3];
        pawR = new float[data.Count, 3];

        for (var i=0;i<data.Count;i++)
        {
            pawL[i, 0] = (float)data[i]["paw_l_x"] * scale;
            pawL[i, 1] = (float)data[i]["paw_l_y"] * scale;
            pawL[i, 2] = (float)data[i]["paw_l_z"] * scale;
            pawR[i, 0] = (float)data[i]["paw_r_x"] * scale;
            pawR[i, 1] = (float)data[i]["paw_r_y"] * scale;
            pawR[i, 2] = (float)data[i]["paw_r_z"] * scale;
        }

        pawLOffset = leftPawTransform.localPosition - new Vector3(pawL[0, 0], pawL[0, 1], pawL[0, 2]);
        pawROffset = rightPawTransform.localPosition - new Vector3(pawR[0, 0], pawR[0, 1], pawR[0, 2]);
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            if (Time.realtimeSinceStartup >= (frame60hz / 60))
            {
                frame60hz++;
                Update60hz();
            }
        }
    }

    void Update60hz()
    {
        if (frame60hz > wait && frame60hz < (wait + timePoints))
        {
            int frame = frame60hz - wait;

            leftPawTransform.localPosition = new Vector3(pawL[frame, 0], pawL[frame, 1], pawL[frame, 2]) + pawLOffset;
            rightPawTransform.localPosition = new Vector3(pawR[frame, 0], pawR[frame, 1], pawR[frame, 2]) + pawROffset;
        }
    }
}
