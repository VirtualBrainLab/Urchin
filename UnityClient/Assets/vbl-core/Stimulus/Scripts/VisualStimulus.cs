using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VisualStimulus : MonoBehaviour {
    // Visual stimulus fields
    private Vector3 mouseEyePosition;
    private Vector2 stimPosition;

    // Tooltip access
    private VisualStimulusTooltip tooltip;
    private bool tooltipActive;

    // Properties
    public List<string> properties;
    private Dictionary<string, int> propertyIndexes;
    private float[] propertyValues;
    // statics
    private static string[] availableProperties = { "contrast", "rotation", "scale", "speed", "coherence" };
    private static float[] defaultPropertyValues = { 1f, 0f, 1f, 1f ,1f};
    private static float[] minPropertyValues = { 0f, 0f, 0.1f, 0f , 0f};
    private static float[] maxPropertyValues = { 1f, 180f, 10f, 10f , 1f};
    private static List<Func<float, bool>> propertyFunctions;

    private void Awake()
    {
        propertyFunctions = new List<Func<float, bool>>();
        propertyFunctions.Add(SetContrast);
        propertyFunctions.Add(SetRotation);
        propertyFunctions.Add(SetScale);
        propertyFunctions.Add(SetSpeed);
        propertyFunctions.Add(SetCoherence);
    }

    // Start is called before the first frame update
    void Start() {
        // Ideally this would be coded in a neutral way, e.g. by calling something on the mouse like
        // MouseManager or whatever and having it return the eye coordinate...
        mouseEyePosition = GameObject.Find("StimCamera").transform.position;

        // try to get a tooltip
        tooltip = gameObject.GetComponent<VisualStimulusTooltip>();
        if (tooltip) { tooltipActive = true; }

        // Setup properties
        SetupProperties();

        UpdateProperties();
    }

    private void SetupProperties()
    {
        propertyIndexes = new Dictionary<string, int>();

        foreach (string property in properties)
        {
            bool available = false;
            int pIdx = 0;
            for (pIdx = 0; pIdx < availableProperties.Length; pIdx++)
            {
                if (property == availableProperties[pIdx])
                {
                    available = true; break;
                }
            }

            if (available)
            {
                propertyIndexes.Add(property, pIdx);
            }
            else
            {
                Debug.LogError("Requested property: " + property + " is not available in VisualStimulus class");
            }
        }

        propertyValues = new float[propertyIndexes.Keys.Count];

        // Initialize values
        foreach (var prop in propertyIndexes)
        {
            propertyValues[prop.Value] = defaultPropertyValues[prop.Value];
        }

    }

    private void UpdateProperties()
    {

    }

    public void Clicked()
    {
        if (tooltipActive)
        {
            if (tooltip.TooltipActive())
            {
                tooltip.HideTooltip();
            }
            else
            {
                // Unnecessarily fancy formatting to keep spacing even
                double spacing = 100.0 / gameObject.name.Length;
                string content = "<mspace=" + spacing.ToString("#.##") + ">" + gameObject.name + "</mspace>";
                tooltip.ShowTooltip(content, properties);
            }
        }
    }

    public void HideTooltip()
    {
        if (tooltipActive)
        {
            tooltip.HideTooltip();
        }
    }

    /**
     * Return the position of this stimulus object on the screen in real coordinates (degrees)
     */
    public void ComputeStimulusPosition()
    { 
        // X reversed (right = -X)
        // Y normal (up = +Y)

        // The +90f is because I'm confused about how this works, if we just use the x/z plane coordinates then why 
        // does it come out rotated?
        float xAngle = Vector3.SignedAngle(new Vector3(mouseEyePosition.x,0, transform.position.z),
                                                 new Vector3(transform.position.x,0, transform.position.z), Vector3.up);
        float yAngle = Vector3.SignedAngle(new Vector3(0, mouseEyePosition.y, transform.position.z),
                                                 new Vector3(0, transform.position.y, transform.position.z), Vector3.right);

        // Return the position of this object in real coordinates
        stimPosition = new Vector2(xAngle, yAngle);
    }

    public List<string> GetProperties()
    {
        return properties;
    }

    public Vector2 StimPosition()
    {
        ComputeStimulusPosition();
        return stimPosition;
    }


    public float GetProperty(string property)
    {
        return propertyValues[propertyIndexes[property]];
    }

    public void SetProperty(string property, float newValue)
    {
        int idx = propertyIndexes[property];
        propertyValues[idx] = Mathf.Clamp(newValue, minPropertyValues[idx], maxPropertyValues[idx]);
        propertyFunctions[idx](newValue);
    }

    public bool SetContrast(float newContrast)
    {
        Color newColor = GetComponent<Renderer>().material.color;
        newColor.a = newContrast;
        GetComponent<Renderer>().material.color = newColor;
        return true;
    }

    public bool SetRotation(float newRotation)
    {
        gameObject.transform.localEulerAngles = new Vector3(90, 0, newRotation);
        return true;
    }

    public bool SetScale(float newScale)
    {
        gameObject.transform.localScale = newScale * Vector2.one;
        return true;
    }

    public bool SetSpeed(float newSpeed)
    {

        return true;
    }

    public bool SetCoherence(float newCoherence)
    {

        return true;
    }
}
