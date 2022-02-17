using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class Utils : MonoBehaviour
{
    private void Start()
    {
        //StartCoroutine(FlatironTest("https://ibl.flatironinstitute.org/cortexlab/Subjects/KS046/2020-12-02/001/alf/lick.times.npy"));
    }

    public Vector3 WorldSpace2apdvlr(Vector3 point)
    {
        point = (point) * 1000f / 25f;
        int ap25 = Mathf.RoundToInt(point.z);
        int dv25 = Mathf.RoundToInt(-point.y);
        int lr25 = Mathf.RoundToInt(-point.x);
        return new Vector3(ap25, dv25, lr25);
    }

    public Vector3 apdvlr2World(Vector3 apdvlr)
    {
        return new Vector3(-apdvlr.z / 40f, -apdvlr.y / 40f, apdvlr.x / 40f);
    }

    public float Hypot(Vector2 values)
    {
        return Mathf.Sqrt(values.x * values.x + values.y * values.y);
    }

    public Color ParseHexColor(string hexString)
    {
        Color color = new Color();
        ColorUtility.TryParseHtmlString(hexString, out color);
        return color;
    }

    // From Math3d: http://wiki.unity3d.com/index.php/3d_Math_functions
    //Two non-parallel lines which may or may not touch each other have a point on each line which are closest
    //to each other. This function finds those two points. If the lines are not parallel, the function 
    //outputs true, otherwise false.
    public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        closestPointLine1 = Vector3.zero;
        closestPointLine2 = Vector3.zero;

        float a = Vector3.Dot(lineVec1, lineVec1);
        float b = Vector3.Dot(lineVec1, lineVec2);
        float e = Vector3.Dot(lineVec2, lineVec2);

        float d = a * e - b * b;

        //lines are not parallel
        if (d != 0.0f)
        {

            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;

            return true;
        }

        else
        {
            return false;
        }
    }

    public float4 Color2float4(Color color)
    {
        return new float4(color.r, color.g, color.b, color.a);
    }

    public float GaussianNoise()
    {
        float u1 = 1.0f - Random.value; // uniform(0,1] random doubles
        float u2 = 1.0f - Random.value;
        return Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2); // random normal(mean,stdDev^2)
    }

    // Note that this might be possible to replace by:
    // Buffer.BlockCopy()
    // see https://stackoverflow.com/questions/6952923/conversion-double-array-to-byte-array
    public float[] LoadBinaryFloatHelper(string datasetName)
    {
        TextAsset textAsset = Resources.Load("Datasets/" + datasetName) as TextAsset;
        byte[] tempData = textAsset.bytes;
        Debug.Log("Loading " + datasetName + " with " + tempData.Length + " bytes");
        float[] data = new float[tempData.Length / 4];

        Buffer.BlockCopy(tempData, 0, data, 0, tempData.Length);
        Debug.LogFormat("Found {0} floats", data.Length);

        return data;
    }
    public uint[] LoadBinaryUInt32Helper(string datasetName)
    {
        TextAsset textAsset = Resources.Load("Datasets/" + datasetName) as TextAsset;
        byte[] tempData = textAsset.bytes;
        Debug.Log("Loading " + datasetName + " with " + tempData.Length + " bytes");
        uint[] data = new uint[tempData.Length / 4];

        Buffer.BlockCopy(tempData, 0, data, 0, tempData.Length);
        Debug.LogFormat("Found {0} UInt32", data.Length);

        return data;
    }
    public ushort[] LoadBinaryUShortHelper(string datasetName)
    {
        TextAsset textAsset = Resources.Load("Datasets/" + datasetName) as TextAsset;
        byte[] tempData = textAsset.bytes;
        Debug.Log("Loading " + datasetName + " with " + tempData.Length + " bytes");
        ushort[] data = new ushort [tempData.Length / 2];

        Buffer.BlockCopy(tempData, 0, data, 0, tempData.Length);
        Debug.LogFormat("Found {0} UShort", data.Length);

        return data;
    }
    public byte[] LoadBinaryByteHelper(string datasetName)
    {
        TextAsset textAsset = Resources.Load("Datasets/" + datasetName) as TextAsset;
        byte[] tempData = textAsset.bytes;
        Debug.Log("Loading " + datasetName + " with " + tempData.Length + " bytes");
        Debug.LogFormat("Found {0} bytes", tempData.Length);

        return tempData;
    }
    public double[] LoadBinaryDoubleHelper(string datasetName)
    {
        TextAsset textAsset = Resources.Load("Datasets/" + datasetName) as TextAsset;
        byte[] tempData = textAsset.bytes;
        Debug.Log("Loading " + datasetName + " with " + tempData.Length + " bytes");
        double[] data = new double[tempData.Length / 8];

        Buffer.BlockCopy(tempData, 0, data, 0, tempData.Length);
        Debug.LogFormat("Found {0} Doubles", data.Length);

        return data;
    }



    public void LoadFlatIronData(string eid, string dataType, string uri, Action<string, string, Array> callback)
    {
        StartCoroutine(FlationRequestNPY(eid, dataType, uri, callback));
    }

    public IEnumerator FlationRequestNPY(string eid, string dataType, string uri, Action<string, string, Array> callback)
    {
        Debug.Log("A Coroutine requested was started for: " + uri);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            var username = "iblmember";
            var password = "GrayMatter19";
            string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                                           .GetBytes(username + ":" + password));
            webRequest.SetRequestHeader("Authorization", "Basic " + encoded);
            webRequest.SetRequestHeader("Encoding", "gzip");
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    Debug.Log(webRequest.downloadedBytes);
                    byte[] data = webRequest.downloadHandler.data;
                    MemoryStream stream = new MemoryStream(data);
                    callback(eid, dataType, LoadNPY(stream));
                    break;
            }
        }

        Debug.Log("GetRequest complete on URL" + uri);
    }

    public IEnumerator FlatironTest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            var username = "iblmember";
            var password = "GrayMatter19";
            string encoded = System.Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                                           .GetBytes(username + ":" + password));
            webRequest.SetRequestHeader("Authorization", "Basic " + encoded);
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    Debug.Log(webRequest.downloadedBytes);
                    byte[] data = webRequest.downloadHandler.data;
                    MemoryStream stream = new MemoryStream(data);
                    Array output = LoadNPY(stream);

                    
                    for (int i =0; i<100; i++)
                    {
                        Debug.Log(output.GetValue(i));
                    }

                    break;
            }
        }
    }

    private static Array LoadNPY(Stream stream)
    {
        Debug.Log("Parsing NPY stream received from URL");
        using (var reader = new BinaryReader(stream, System.Text.Encoding.ASCII
            #if !NET35 && !NET40
                , leaveOpen: true
            #endif
            ))
        {
            int bytes;
            Type type;
            int[] shape;
            if (!parseReader(reader, out bytes, out type, out shape))
                throw new FormatException();

            Debug.Log(type);
            Debug.Log("Shape length: " + shape.Length);

            if (shape.Length==1)
            {
                Array array = Array.CreateInstance(type, shape[0]);
                array = readValueMatrix(reader, array, bytes, type, shape);

                // TODO: Double arrays are useless in Unity we should just convert them to float and save memory
                return array;
            }
            else
            {
                Debug.LogError("Warning: cannot handle this data shape");
            }
        }

        return null;
    }

    private static Array readValueMatrix(BinaryReader reader, Array matrix, int bytes, Type type, int[] shape)
    {
        int total = 1;
        for (int i = 0; i < shape.Length; i++)
            total *= shape[i];
        var buffer = new byte[bytes * total];

        reader.Read(buffer, 0, buffer.Length);
        Buffer.BlockCopy(buffer, 0, matrix, 0, buffer.Length);

        return matrix;
    }

    private static bool parseReader(BinaryReader reader, out int bytes, out Type t, out int[] shape)
    {
        bytes = 0;
        t = null;
        shape = null;

        // The first 6 bytes are a magic string: exactly "x93NUMPY"
        if (reader.ReadChar() != 63) return false;
        if (reader.ReadChar() != 'N') return false;
        if (reader.ReadChar() != 'U') return false;
        if (reader.ReadChar() != 'M') return false;
        if (reader.ReadChar() != 'P') return false;
        if (reader.ReadChar() != 'Y') return false;

        byte major = reader.ReadByte(); // 1
        byte minor = reader.ReadByte(); // 0

        if (major != 1 || minor != 0)
            throw new NotSupportedException();

        ushort len = reader.ReadUInt16();

        string header = new String(reader.ReadChars(len));
        string mark = "'descr': '";
        int s = header.IndexOf(mark) + mark.Length;
        int e = header.IndexOf("'", s + 1);
        string type = header.Substring(s, e - s);
        bool? isLittleEndian;
        t = GetType(type, out bytes, out isLittleEndian);

        if (isLittleEndian.HasValue && isLittleEndian.Value == false)
            throw new Exception();

        mark = "'fortran_order': ";
        s = header.IndexOf(mark) + mark.Length;
        e = header.IndexOf(",", s + 1);
        bool fortran = bool.Parse(header.Substring(s, e - s));

        if (fortran)
            throw new Exception();

        mark = "'shape': (";
        s = header.IndexOf(mark) + mark.Length;
        e = header.IndexOf(")", s + 1);
        shape = header.Substring(s, e - s).Split(',').Where(v => !String.IsNullOrEmpty(v)).Select(Int32.Parse).ToArray();

        return true;
    }


    private static Type GetType(string dtype, out int bytes, out bool? isLittleEndian)
    {
        isLittleEndian = IsLittleEndian(dtype);
        bytes = Int32.Parse(dtype.Substring(2));

        string typeCode = dtype.Substring(1);

        if (typeCode == "b1")
            return typeof(bool);
        if (typeCode == "i1")
            return typeof(Byte);
        if (typeCode == "i2")
            return typeof(Int16);
        if (typeCode == "i4")
            return typeof(Int32);
        if (typeCode == "i8")
            return typeof(Int64);
        if (typeCode == "u1")
            return typeof(Byte);
        if (typeCode == "u2")
            return typeof(UInt16);
        if (typeCode == "u4")
            return typeof(UInt32);
        if (typeCode == "u8")
            return typeof(UInt64);
        if (typeCode == "f4")
            return typeof(Single);
        if (typeCode == "f8")
            return typeof(Double);
        if (typeCode.StartsWith("S"))
            return typeof(String);

        throw new NotSupportedException();
    }

    private static bool? IsLittleEndian(string type)
    {
        bool? littleEndian = null;

        switch (type[0])
        {
            case '<':
                littleEndian = true;
                break;
            case '>':
                littleEndian = false;
                break;
            case '|':
                littleEndian = null;
                break;
            default:
                throw new Exception();
        }

        return littleEndian;
    }


    public Dictionary<string, float3> LoadIBLmlapdv()
    {
        // load the UUID and MLAPDV data
        List<Dictionary<string, object>> data_mlapdv = CSVReader.Read("Datasets/ibl/mlapdv_with_uuid");
        Dictionary<string, float3> mlapdvData = new Dictionary<string, float3>();
        float scale = 1000f;

        for (var i = 0; i < data_mlapdv.Count; i++)
        {
            string uuid = (string)data_mlapdv[i]["uuid"];
            float ml = (float)data_mlapdv[i]["ml"] / scale;
            float ap = (float)data_mlapdv[i]["ap"] / scale;
            float dv = (float)data_mlapdv[i]["dv"] / scale;
            mlapdvData.Add(uuid, new float3(ml, ap, dv));
        }

        return mlapdvData;
    }
}
