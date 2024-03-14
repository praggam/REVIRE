using Assets.Scripts.Gameplay;
using System;
using UnityEngine;

// Implemented to ease management of UI elements with multiple tabs/pages. Allows to store changes in the meantime and save or cancel them when tab group closed.
public abstract class Saveable : MonoBehaviour
{
    private TabsController tabsController = null;
    protected TaskType taskType = TaskType.None;

    public abstract void SaveChanges();

    public abstract void CancelChanges();

    protected virtual void Awake()
    {
        tabsController = GetComponentInParent<TabsController>();
        TaskTab tab = GetComponentInParent<TaskTab>();
        if (tab != null)
        {
            taskType = tab.task;
        }
        else
        {
            Debug.LogWarning("Task settings: Task Tab not found in parent.");
        }
    }

    protected virtual void OnEnable()
    {
        if (tabsController)
        {
            tabsController.OnSave += SaveChanges;
            tabsController.OnCancel += CancelChanges;
        }
    }

    protected virtual void OnDisable()
    {
        //if (tabsController)
        //    tabsController.OnSave -= SaveChanges;
    }

    protected virtual void OnDestroy()
    {
        if (tabsController)
        {
            tabsController.OnSave -= SaveChanges;
            tabsController.OnCancel -= CancelChanges;
        }
    }
}