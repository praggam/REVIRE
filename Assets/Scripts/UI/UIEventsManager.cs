using System;
using UnityEngine;

// Responsible for sending commands to all other classes (mostly Managers) on UI events. Designed to be a single point of entry for all UI events. Not all UI elements use this currently but eventually a transition should be made as it is easier to debug and control what events are triggered on which action in UI. Assigning methods in UI events (such as OnClick event for button) is unreliable as it resets every time method name changes or class is moved which is difficult to debug and control.
public class UIEventsManager : MonoBehaviour
{
    public static event Action OnTaskMenuOpen;
    public static event Action OnTaskMenuClose;

    public void PauseButtonClicked()
    {
        //OnTaskMenuOpen?.Invoke();
        CanvasManager.Instance.ShowCanvas(CanvasType.TaskMenu);
        GameManager.Instance.PauseSession();
    }

    public void ResumeButtonClicked()
    {
        //OnTaskMenuClose?.Invoke();
        CanvasManager.Instance.ShowCanvas(CanvasType.TaskOverlay);
        GameManager.Instance.ResumeSession();
    }

    public void OptionsButtonClicked()
    {
        CanvasManager.Instance.ShowCanvas(CanvasType.OptionsMenu);
    }

    public void BackButtonClicked()
    {
        CanvasManager.Instance.ShowPreviousCanvas();
    }

    public void SessionCloseButtonClicked()
    {
        GameManager.Instance.EndSession();
        ScoreManager.Instance.ResetSessionScore();
        CanvasManager.Instance.ShowCanvas(CanvasType.MainMenu);
    }
}
