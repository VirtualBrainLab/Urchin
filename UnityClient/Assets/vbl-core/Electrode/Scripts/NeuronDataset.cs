using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuronDataset
{
    private byte[,,] indexes;
    private float[] map;

    public NeuronDataset(byte[] data, float[] map, int[] ccfSize, byte[] ccfIndexMap)
    {
        indexes = new byte[ccfSize[0], ccfSize[1], ccfSize[2]];
        int ccfi = 0;
        int i = 0;
        // Datasets are stored in column order, so go through in reverse
        for (int lr = 0; lr < ccfSize[2]; lr++)
        {
            for (int dv = 0; dv < ccfSize[1]; dv++)
            {
                for (int ap = 0; ap < ccfSize[0]; ap++)
                {
                    if (ccfIndexMap[ccfi] == 1)
                    {
                        indexes[ap, dv, lr] = data[i];
                        i++;
                    }
                    ccfi++;
                }
            }
        }
        this.map = map;
    }

    public float ValueAtIndex(int ap, int dv, int lr)
    {
        return map[indexes[ap, dv, lr]];
    }
}
