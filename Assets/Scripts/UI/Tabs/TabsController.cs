using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TabsController : MonoBehaviour
{
    [SerializeField] private List<TabButton> tabs = new List<TabButton>();

    public event Action OnSave;
    public event Action OnCancel;

    private void Awake()
    {
        tabs.ForEach(tab => tab.OnTabClicked += ActivateTab);
    }

    private void OnEnable()
    {
        ActivateTab(tabs.First());
    }

    public void SaveSettings()
    {
        // reactivate all tabs to let them listen to save event
        tabs.ForEach(t => t.SetActiveTab(true));

        // TODO check if this is invoking anything
        OnSave?.Invoke(); 
    }
    public void CancelSettings()
    {
        // reactivate all tabs to let them listen to cancel event
        tabs.ForEach(t => t.SetActiveTab(true));
        OnCancel.Invoke();
    }

    private void ActivateTab(TabButton tab)
    {
        if(tab != null) {
            tabs.ForEach(t => t.SetActiveTab(false));
            tab.SetActiveTab(true);
        }   
    }

#if UNITY_EDITOR
    [ContextMenu("Autofill Tabs From Children")]
    public void AddAllTabs()
    {
        tabs = gameObject.GetComponentsInChildren<TabButton>().ToList();
    }
#endif
}
