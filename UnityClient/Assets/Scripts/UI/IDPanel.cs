using TMPro;
using UnityEngine;

public class IDPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField _inputField;
    [SerializeField] GameObject _idPanelGO;

    public void UpdateText(string text)
    {
        _inputField.SetTextWithoutNotify(text);
    }

    public void SetVisibility(bool visible)
    {
        _idPanelGO.SetActive(visible);
    }
}
