using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIColor : MonoBehaviour
{
    Vector4 green = new Vector4(63, 0, 27, 0);
    Vector4 orange = new Vector4(0, 22, 68, 0);
    Vector4 teal = new Vector4(58, 7, 0, 0);
    Vector4 red = new Vector4(0, 41, 48, 0);
    Vector4 blue = new Vector4(53, 34, 0, 0);

    float[] shading = { 0f, 10f, 25f, 50f, 75f };

    int mV = 1;

    Vector3 CMYK2RGB(Vector4 cmyk)
    {
        return new Vector3((mV - cmyk.x) * (mV - cmyk.w), (mV - cmyk.y) * (mV - cmyk.w), (mV - cmyk.z) * (mV - cmyk.w));
    }
    Vector3 CMYK2RGB(Vector4 cmyk, float shading)
    {
        return new Vector3((mV - cmyk.x) * (mV - shading), (mV - cmyk.y) * (mV - shading), (mV - cmyk.z) * (mV - shading));
    }

    public float DefaultShading(int level)
    {
        if (level > shading.Length)
        {
            Debug.LogError("(UIColor) Requested shading strength does not exist");
            return 100f;
        }
        else
        {
            return shading[level];
        }
    }

    public Vector3 Green()
    {
        return CMYK2RGB(green);
    }
    public Vector3 Green(float shading)
    {
        return CMYK2RGB(green, shading);
    }
    public Vector3 Orange()
    {
        return CMYK2RGB(orange);
    }
    public Vector3 Orange(float shading)
    {
        return CMYK2RGB(orange, shading);
    }
    public Vector3 Teal()
    {
        return CMYK2RGB(teal);
    }
    public Vector3 Teal(float shading)
    {
        return CMYK2RGB(teal, shading);
    }
    public Vector3 Red()
    {
        return CMYK2RGB(red);
    }
    public Vector3 Red(float shading)
    {
        return CMYK2RGB(red, shading);
    }
    public Vector3 Blue()
    {
        return CMYK2RGB(blue);
    }
    public Vector3 Blue(float shading)
    {
        return CMYK2RGB(blue, shading);
    }
}
