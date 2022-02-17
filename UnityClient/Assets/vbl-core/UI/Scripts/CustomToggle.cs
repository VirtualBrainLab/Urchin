

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class CustomToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private bool initialStateSelected;

    private PanelController pcontroller;


    [SerializeField] private Animator animator;

    [SerializeField] private GameObject mainPanel;

    //[HideInInspector] public ToggleGroup toggleGroup;

    private bool isSelected = false;
    public bool IsSelected => isSelected;

    private bool isDisabled = false;
    public bool IsDisabled => isDisabled;

    private void Start()
    {
        pcontroller = GameObject.Find("main").GetComponent<PanelController>();
        if (initialStateSelected)
            SetSelected(true);
        // When the CustomToggle starts up the onSelect and onDeselect functions are unset
    }

    public void SetPanel(GameObject panel)
    {
        mainPanel = panel;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDisabled)
            return;

        if (isSelected)
        {
            OnDeselect();
        }
        else
        {
            mainPanel.SetActive(true);
            SetSelected(true);
        }
    }

    public void OnDeselect()
    {
        if (isDisabled)
            return;

        mainPanel.SetActive(false);
        SetSelected(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDisabled)
            return;

        if (!isSelected)
        {
            animator.SetTrigger("Highlighted");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDisabled)
            return;

        if (!isSelected)
        {
            animator.SetTrigger("Normal");
        }
    }

    public void SetSelected(bool value)
    {
        if (isDisabled)
            return;

        isSelected = value;
        if (isSelected == true)
        {
            animator.SetTrigger("Selected");
        }
    }

    public void SetDisabled(bool value)
    {
        isDisabled = value;
        if (isDisabled == true)
        {
            isSelected = false;
            animator.SetTrigger("Disabled");
        }
        else
        {
            animator.SetTrigger("Normal");
        }
    }
}