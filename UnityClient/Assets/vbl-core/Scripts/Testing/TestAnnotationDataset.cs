using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestAnnotationDataset : MonoBehaviour
{
    [SerializeField] private Utils util;
    [SerializeField] private GameObject slider;

    // this file stores the indexes that we actually have data for
    private string datasetIndexFile = "data_indexes";
    private int[] baseSize = { 528, 320, 456 };

    // annotation files
    private string annotationIndexFile = "ann/indexes";
    private AnnotationDataset annotationDataset;

    // color data
    private Dictionary<int, Color> colors;

    // Quads
    Texture2D quadImage;
    [SerializeField] private GameObject quad;
    int depthIdx;

    // Start is called before the first frame update
    void Start()
    {
        // First load the indexing file
        Debug.Log("Loading the CCF index file");
        byte[] ccfIndexMap = util.LoadBinaryByteHelper(datasetIndexFile);

        // Load the annotation file
        Debug.Log("Loading the CCF annotation index and map files");
        ushort[] annData = util.LoadBinaryUShortHelper(annotationIndexFile);
        uint[] annMap = util.LoadBinaryUInt32Helper(annotationIndexFile + "_map");
        Debug.Log("Creating the CCF AnnotationDataset object");
        annotationDataset = new AnnotationDataset("annotation", annData, annMap, ccfIndexMap);

        colors = new Dictionary<int, Color>();

        List<Dictionary<string, object>> data = CSVReader.Read("AllenCCF/ontology_structure_minimal");

        for (var i = 0; i < data.Count; i++)
        {
            // get the values in the CSV file and add to the tree
            int id = (int)data[i]["atlas_id"];
            //string name = (string)data[i]["name"];
            //string shortName = (string)data[i]["acronym"];
            string hexColorString = data[i]["color_hex_code"].ToString();
            Color color = util.ParseHexColor(hexColorString);
            Debug.Log(id);
            Debug.Log(color);
            if (!colors.ContainsKey(id))
                colors.Add(id, color);
        }

        quadImage = new Texture2D(baseSize[0], baseSize[2]);
        quadImage.filterMode = FilterMode.Point;

        depthIdx = 0;
        quad.GetComponent<Renderer>().material.mainTexture = quadImage;

        RenderAnnotationLayer();
    }

    private void RenderAnnotationLayer()
    {
        for (int x = 0; x < baseSize[0]; x++)
        {
            for (int y = 0; y < baseSize[2]; y++)
            {
                int key = annotationDataset.ValueAtIndex(x, depthIdx, y);
                if (colors.ContainsKey(key))
                    quadImage.SetPixel(x, y, colors[key]);
                else
                    quadImage.SetPixel(x, y, Color.black);
            }
        }
        quadImage.Apply();
    }

    public void UpdateAnnotationDisplay()
    {
        depthIdx = (int)slider.GetComponent<Slider>().value;
        Debug.Log("Depth: " + depthIdx);
        RenderAnnotationLayer();
    }
}
