using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Globalization;
using TMPro;

public class VisualStimulusTooltip : MonoBehaviour
{
    private Camera uiCam;

    // access to VisualStimulus
    private VisualStimulus visStim;

    // tooltip fields
    public GameObject tooltipPrefab;    // the prefab to use for tooltips around this stimulus
    private GameObject curTooltip;      // the current instantiated tool tip prefab   
    private GameObject header;          // the tooltip header
    private GameObject content;         // the tooltip content field (includes sliders)
    public Slider contrastSlider;      // the tooltip slider to adjust stim contrast
    public Slider rotationSlider;      // the tooltip slider to adjust stim rotation
    public TMP_InputField contrastInput;
    public TMP_InputField rotationInput;

    // Start is called before the first frame update
    void Start()
    {
        // Setup camera / visStim
        uiCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        visStim = gameObject.GetComponent<VisualStimulus>();

    }

    public void SetTooltipObject(GameObject tooltip)
    {
        curTooltip = tooltip;
        LateStart();
    }

    private void LateStart()
    {
        header = curTooltip.transform.Find("Header").gameObject;
        content = curTooltip.transform.Find("Content").gameObject;
        contrastSlider = content.transform.Find("ContrastSlider").GetComponent<Slider>();
        rotationSlider = content.transform.Find("RotationSlider").GetComponent<Slider>();
        contrastInput = content.transform.Find("ContrastInput").GetComponent<TMP_InputField>();
        rotationInput = content.transform.Find("RotationInput").GetComponent<TMP_InputField>();
        contrastSlider.onValueChanged.AddListener(delegate { SetContrast(contrastSlider.value); });
        rotationSlider.onValueChanged.AddListener(delegate { SetRotation(rotationSlider.value); });
        contrastInput.onValueChanged.AddListener(delegate { SetContrast(contrastInput.text); });
        rotationInput.onValueChanged.AddListener(delegate { SetRotation(rotationInput.text); });
    }

    private void UpdateTooltipPos()
    {
        Vector2 position = uiCam.WorldToScreenPoint(transform.position);
        // Position tooltip based on screen position
        // see: https://www.youtube.com/watch?v=HXFoUGw7eKk&ab_channel=GameDevGuideGameDevGuide
        float pivotX = position.x / Screen.width;
        float pivotY = position.y / Screen.height;
        curTooltip.GetComponent<RectTransform>().pivot = new Vector2(-0.75f + pivotX, pivotY);
        curTooltip.transform.position = position;
    }

    private void UpdateTooltipProperties(List<string> properties)
    {

    }

    private void SetTooltipText(string contText, string headText = "")
    {
        // Don't display the header when there's no header text
        if (string.IsNullOrEmpty(headText))
        {
            header.gameObject.SetActive(false);
        }
        else
        {
            header.gameObject.SetActive(true);
            header.GetComponent<TextMeshProUGUI>().text = headText;
        }
        content.GetComponent<TextMeshProUGUI>().text = contText;
    }

    private void SetContrast(string newContrast)
    {
        if (float.TryParse(newContrast, NumberStyles.Float, CultureInfo.InvariantCulture, out float output))
        {
            visStim.SetContrast(output);
        }
    }

    private void SetContrast(float newContrast)
    {
        visStim.SetContrast(newContrast);
    }

    private void SetRotation(string newRotation)
    {
        if (float.TryParse(newRotation, NumberStyles.Float, CultureInfo.InvariantCulture, out float output))
        {
            visStim.SetRotation(output);
        }
    }
    private void SetRotation(float newRotation)
    {
        visStim.SetRotation(newRotation);
    }

    public void ShowTooltip(string content, List<string> properties, string header = "") {
        if (curTooltip) {
            SetTooltipText(content, header);
            UpdateTooltipPos();
            UpdateTooltipProperties(properties);
            curTooltip.SetActive(true);
        }
    }

    public void HideTooltip() {
        if (curTooltip) {
            curTooltip.SetActive(false);
        }
    }
    public bool TooltipActive()
    {
        if (!(curTooltip == null))
        {
            return curTooltip.activeSelf;
        }
        return false;
    }
}
