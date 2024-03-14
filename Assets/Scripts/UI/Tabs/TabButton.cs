using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TabButton : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Content of tab which will be activated when tab button clicked.")]
    public GameObject tabContent = null;
    public event Action<TabButton> OnTabClicked;

    private Button button = null;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnTabClicked.Invoke(this);
    }

    public void SetActiveTab(bool value)
    {
        if(tabContent != null)
        {
            tabContent.SetActive(value);
        }
        else
        {
            Debug.LogWarning("Tab content not assigned.");
        }
            

        if (button != null)
        {
            button.interactable = !value;
        }
    }

}
