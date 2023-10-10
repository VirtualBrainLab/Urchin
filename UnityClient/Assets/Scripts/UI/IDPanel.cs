using TMPro;
using UnityEngine;

public class IDPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField _inputField;

    public void UpdateText(string text)
    {
        _inputField.SetTextWithoutNotify(text);
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
