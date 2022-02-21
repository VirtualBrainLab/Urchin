using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UM_Launch : MonoBehaviour
{
    [SerializeField] private CCFModelControl modelControl;
    [SerializeField] private float maxExplosion = 10f;
    [SerializeField] private List<Shader> shaderOptions;
    [SerializeField] private List<string> shaderNames;

    [SerializeField] private GameObject consolePanel;
    [SerializeField] private TextMeshProUGUI consoleText;

    [Range(0,1), SerializeField] private float percentageExploded = 0f;

    private float prevPerc = 0f;

    private Dictionary<int, float> accuracy;
    private List<CCFTreeNode> nodes;

    private Vector3 center = new Vector3(5.7f, 4f, -6.6f);

    private Vector3 teal = new Vector3(0f, 1f, 1f);
    private Vector3 magenta = new Vector3(1f, 0f, 1f);

    // Start is called before the first frame update
    void Start()
    {

        accuracy = new Dictionary<int, float>();
        nodes = new List<CCFTreeNode>();

        modelControl.SetBeryl(true);
        modelControl.LateStart(false);

        //List<Dictionary<string, object>> data = CSVReader.Read("Datasets/lda_acc/lda");
        //Dictionary<int, List<float>> accData = new Dictionary<int, List<float>>();

        //for (int i = 0 ; i < data.Count; i++)
        //{
        //    Dictionary<string, object> row = data[i];

        //    string acronym = (string)row["region"];
        //    int nClu = (int)row["n_clus"];
        //    float acc = (float)row["ac"];
        //    int ID = modelControl.Acronym2ID(acronym);
        //    if (!accData.ContainsKey(ID))
        //        accData[ID] = new List<float>();
        //    accData[ID].Add(acc);
        //}

        //foreach (KeyValuePair<int, List<float>> pair in accData)
        //{
        //    accuracy.Add(pair.Key, pair.Value.Average());
        //}

        //StartCoroutine(DelayedColorChange(1f));
    }

    //IEnumerator DelayedColorChange(float delayTime)
    //{
    //    yield return new WaitForSeconds(delayTime);
    //    // now change the color of the nodes

    //    foreach (KeyValuePair<int, float> pair in accuracy)
    //    {
    //        CCFTreeNode node = modelControl.tree.findNode(pair.Key);
    //        if (node != null)
    //        {
    //            float intensity = (pair.Value - 0.5f) * 4f;
    //            node.loadNodeModel(true);
    //            node.SetColor(Cool(intensity));

    //            nodes.Add(node);
    //        }
    //    }
    //}

    // Update is called once per frame
    void Update()
    {
        // Check if we need to make an update
        if (prevPerc != percentageExploded)
        {
            prevPerc = percentageExploded;

            // for each tree node, move it's model away from the 0,0,0 point
            foreach (CCFTreeNode node in nodes)
                node.ExplodeModel(Vector3.zero, maxExplosion * percentageExploded);
        }

        // Check for key down events
        if (Input.GetKeyDown(KeyCode.C))
        {
            consolePanel.SetActive(!consolePanel.activeSelf);
        }
    }

    // [TODO] Refactor colormaps into their own class
    public Color Cool(float perc)
    {
        Vector3 colorVector = Vector3.Lerp(teal, magenta, perc);
        return new Color(colorVector.x, colorVector.y, colorVector.z, 1f);
    }

    public void Log(string text)
    {
        // Todo: deal with log running off the screen
        consoleText.text += "\n" + text;
    }
}
