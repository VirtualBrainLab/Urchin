using UnityEngine;

public class AnnotationDataset
{
    private int[] baseSize = { 528, 320, 456 };
    private int[,,] annotations;
    private bool[,,] areaBorders;

    public AnnotationDataset(string name, ushort[] data, uint[] map, byte[] ccfIndexMap)
    {
        annotations = new int[baseSize[0], baseSize[1], baseSize[2]];

        int ccfi = 0;
        int i = 0;
        // Datasets are stored in column order, so go through in reverse
        for (int lr = 0; lr < baseSize[2]; lr++)
        {
            for (int dv = 0; dv < baseSize[1]; dv++)
            {
                for (int ap = 0; ap < baseSize[0]; ap++)
                {
                    if (ccfIndexMap[ccfi]==1)
                    {
                        annotations[ap, dv, lr] = (int)map[data[i]-1];
                        i++;
                    }
                    ccfi++;
                } 
            }
        }
    }

    public void ComputeBorders()
    {
        areaBorders = new bool[baseSize[0], baseSize[1], baseSize[2]];

        for (int ap = 0; ap < baseSize[0]; ap++)
        {
            // We go through coronal slices, going down each DV depth, anytime the *next* annotation point changes, we mark this as a border
            for (int lr = 0; lr < (baseSize[2]-1); lr++)
            {
                for (int dv = 0; dv < (baseSize[1]-1); dv ++)
                {
                    if ((annotations[ap, dv, lr] != annotations[ap, dv + 1, lr]) || annotations[ap,dv,lr] != annotations[ap, dv, lr+1])
                        areaBorders[ap, dv, lr] = true;
                }
            }
        }

    }

    public bool BorderAtIndex(int ap, int dv, int lr)
    {
        if ((ap >= 0 && ap < baseSize[0]) && (dv >= 0 && dv < baseSize[1]) && (lr >= 0 && lr < baseSize[2]))
            return areaBorders[ap, dv, lr];
        else
            return false;
    }

    public bool BorderAtIndex(Vector3 apdvlr)
    {
        return BorderAtIndex(Mathf.RoundToInt(apdvlr.x), Mathf.RoundToInt(apdvlr.y), Mathf.RoundToInt(apdvlr.z));
    }

    public int ValueAtIndex(int ap, int dv, int lr)
    {
        if ((ap >= 0 && ap < baseSize[0]) && (dv >= 0 && dv < baseSize[1]) && (lr >= 0 && lr < baseSize[2]))
            return annotations[ap, dv, lr];
        else
            return int.MinValue;
    }

    public int ValueAtIndex(Vector3 apdvlr)
    {
        return ValueAtIndex(Mathf.RoundToInt(apdvlr.x), Mathf.RoundToInt(apdvlr.y), Mathf.RoundToInt(apdvlr.z));
    }
}
