using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    #region Serialized
    [SerializeField] private GameObject _textParent;
    [SerializeField] private GameObject _textPrefab;
    [SerializeField] private Canvas uiCanvas;
    #endregion

    #region Private variables
    private Dictionary<string, GameObject> _textGOs;
    #endregion

    #region Unity
    private void Awake()
    {
        _textGOs = new Dictionary<string, GameObject>();
    }
    #endregion

    #region Public functions

    public void Create(List<string> data)
    {
        Debug.Log("Creating text");
        foreach (string textName in data)
        {
            if (!_textGOs.ContainsKey(textName))
            {
                GameObject textGO = Instantiate(_textPrefab, _textParent.transform);
                _textGOs.Add(textName, textGO);
            }
        }
    }

    public void Delete(List<string> data)
    {
        Debug.Log("Deleting text");
        foreach (string textName in data)
        {
            if (_textGOs.ContainsKey(textName))
            {
                Destroy(_textGOs[textName]);
                _textGOs.Remove(textName);
            }
        }
    }

    public void SetText(Dictionary<string, string> data)
    {
        foreach (KeyValuePair<string, string> kvp in data)
        {
            if (_textGOs.ContainsKey(kvp.Key))
                _textGOs[kvp.Key].GetComponent<TMP_Text>().text = kvp.Value;
        }
    }

    public void SetColor(Dictionary<string, string> data)
    {
        Debug.Log("Setting text colors");
        foreach (KeyValuePair<string, string> kvp in data)
        {
            Color newColor;
            if (_textGOs.ContainsKey(kvp.Key) && ColorUtility.TryParseHtmlString(kvp.Value, out newColor))
            {
                _textGOs[kvp.Key].GetComponent<TMP_Text>().color = newColor;
            }
            else
                Client.LogError("Failed to set text color to: " + kvp.Value);
        }
    }

    public void SetSize(Dictionary<string, int> data)
    {
        Debug.Log("Setting text sizes");
        foreach (KeyValuePair<string, int> kvp in data)
        {
            if (_textGOs.ContainsKey(kvp.Key))
                _textGOs[kvp.Key].GetComponent<TMP_Text>().fontSize = kvp.Value;
        }
    }

    public void SetPosition(Dictionary<string, List<float>> data)
    {
#if UNITY_EDITOR
        Debug.Log("Setting text positions");
#endif
        Vector2 canvasWH = new Vector2(uiCanvas.GetComponent<RectTransform>().rect.width, uiCanvas.GetComponent<RectTransform>().rect.height);
        foreach (KeyValuePair<string, List<float>> kvp in data)
        {
            if (_textGOs.ContainsKey(kvp.Key))
            {
                _textGOs[kvp.Key].transform.localPosition = new Vector2(canvasWH.x * kvp.Value[0] / 2, canvasWH.y * kvp.Value[1] / 2);
            }
            else
            {
                Debug.Log("Couldn't set position for " + kvp.Key);
                Client.LogError(string.Format("Couldn't set position of {0}", kvp.Key));
            }
        }
    }

    public void Clear()
    {
        Debug.Log("(Client) Clearing text");
        foreach (GameObject text in _textGOs.Values)
            Destroy(text);
        _textGOs = new Dictionary<string, GameObject>();
    }
    #endregion
}
