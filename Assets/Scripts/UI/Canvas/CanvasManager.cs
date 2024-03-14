using Assets.Scripts.Gameplay;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// Responsible for switching between different panels (canvas controllers). Makes sure only one canvas is active at a time.
public class CanvasManager : Singleton<CanvasManager>
{
    [Tooltip("Define which hand/s will be able to open the menu with menu gesture")]
    [SerializeField] private List<HandController> handControllers = new List<HandController>();

    [SerializeField] private SummaryPanel summaryPanel = null;
    [SerializeField] private GestureSequence menuOpeningSequence = null;
    [SerializeField] private TMP_Text taskTitle = null;

    private List<CanvasController> canvasControllers;
    private CanvasController activeCanvas = null;
    private CanvasController previousCanvas = null;

    private CanvasController GetCanvasController(CanvasType type) =>
        canvasControllers.Find(controller => controller.canvasType.Equals(type));

    private void Awake()
    {
        InitializeSingleton(this);

        // note that all controllers must be active in the scene on load
        canvasControllers = GetComponentsInChildren<CanvasController>().ToList();
        canvasControllers.ForEach(controller => controller.gameObject.SetActive(false));

        // open main menu canvas on app start
        ShowCanvas(CanvasType.MainMenu);

        OVRManager.display.RecenterPose();
    }

    private void Start()
    {
        foreach (HandController hc in handControllers)
        {
            hc.OnSequenceDetected += TryOpenMenu;
        }

        GameManager.OnTaskStarted += OnTaskStarted;
        GameManager.Instance.OnTaskEnded += OnTaskEnded;
        GameManager.OnSessionEnded += OnSessionEnded;

        UIEventsManager.OnTaskMenuOpen += OnTaskMenuOpen;
        UIEventsManager.OnTaskMenuClose += OnTaskMenuClose;
    }

    #region EVENTS

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        foreach (HandController hc in handControllers)
        {
            hc.OnSequenceDetected -= TryOpenMenu;
        }

        GameManager.OnTaskStarted -= OnTaskStarted;
        GameManager.Instance.OnTaskEnded -= OnTaskEnded;
        GameManager.OnSessionEnded -= OnSessionEnded;

        UIEventsManager.OnTaskMenuOpen -= OnTaskMenuOpen;
        UIEventsManager.OnTaskMenuClose -= OnTaskMenuClose;
    }

    private void OnTaskMenuOpen()
    {
        if (activeCanvas.canvasType.Equals(CanvasType.TaskOverlay))
        {
            Debug.LogWarning("CanvasManager - trying to open task menu while it is already open.");
        }

        if (GameManager.Instance.IsSessionActive())
        {
            ShowCanvas(CanvasType.TaskMenu);
        }
        else
        {
            Debug.LogWarning("CanvasManager - trying to open task menu while session is inactive.");
        }
    }

    private void OnTaskMenuClose()
    {
        if (!activeCanvas.canvasType.Equals(CanvasType.TaskOverlay))
        {
            Debug.LogWarning("CanvasManager - trying to close task menu while it is already closed.");
        }

        if (GameManager.Instance.IsSessionActive())
        {
            ShowCanvas(CanvasType.TaskOverlay);
        }
        else
        {
            Debug.LogWarning("CanvasManager - trying to open task menu while session is inactive.");
        }
    }

    private void OnTaskStarted(Task task)
    {
        taskTitle.text = task.name;
        ShowCanvas(CanvasType.TaskOverlay);
    }

    private void OnTaskEnded(Task task)
    {
        summaryPanel.OnTaskEnded(task);
        ShowCanvas(CanvasType.SessionSummary);
    }

    private void OnSessionEnded()
    {
        ShowCanvas(CanvasType.SessionSummary);
    }

    #endregion EVENTS

    #region MENUS

    public void ShowCanvas(CanvasType canvasType)
    {
        CanvasController canvas = GetCanvasController(canvasType);

        if (canvas == null || canvasType.Equals(CanvasType.Default))
        {
            Debug.LogWarning(string.Format("CanvasManager - error displaying canvas. Canvas not found: {0}.", canvasType));
            return;
        }

        HideCanvas(activeCanvas);

        canvas.gameObject.SetActive(true);
        activeCanvas = canvas;
    }

    public void ShowCanvas(CanvasController canvas)
    {

        if (canvas == null || canvas.canvasType.Equals(CanvasType.Default))
        {
            Debug.LogWarning(string.Format("CanvasManager - error displaying canvas. Canvas not found: {0}.", canvas));
            return;
        }

        HideCanvas(activeCanvas);

        canvas.gameObject.SetActive(true);
        activeCanvas = canvas;
    }

    public void HideCanvas(CanvasController canvas)
    {  
        if (canvas != null && canvas.isActiveAndEnabled)
        {
            canvas.gameObject.SetActive(false);

            if (canvas.Equals(activeCanvas))
            {
                previousCanvas = activeCanvas;
                activeCanvas = null;
            } 
        }
    }

    public void ShowPreviousCanvas()
    {
        if(previousCanvas != null)
        {
            ShowCanvas(previousCanvas);
        }
        else
        {
            Debug.LogWarning("CanvasManager - trying to display previous canvas: Canvas null.");
        }
    }

    // TODO this should also pause/resume game so it should be put in UIEventsManager
    private void TryOpenMenu(GestureSequence gestureSequence)
    {
        if (gestureSequence.Equals(menuOpeningSequence))
        {
            if (activeCanvas != null && activeCanvas.canvasType.Equals(CanvasType.TaskMenu))
            {
                ShowCanvas(CanvasType.TaskMenu);
            }
            else
            {
                HideCanvas(activeCanvas);
            }
        }
    }

    #endregion MENUS

    #region NOTIFICATIONS, INCOMPLETE

    //public NotificationController ShowNotification(NotificationType notificationType)
    //{
    //    NotificationController notification = GetNotificationController(notificationType);

    //    if (notification == null)
    //    {
    //        Debug.LogWarning("Notification not found: " + notificationType);
    //        return null;
    //    }
    //    // if there is an active notification, queue current to display once it's closed
    //    // maybe check this before setting notification values and store queue as text, canvas type and button text
    //    if (activeNotification != null)
    //    {
    //        QueueNotification(notification);
    //    }
    //    else
    //    {
    //        activeNotification = notification;
    //        activeNotification.gameObject.SetActive(true);
    //    }

    //    return notification;
    //}

    //public void HideNotification(NotificationController notification)
    //{
    //    // if notification is currently open, hide it and activate next notification and remove it from the queue
    //    if (activeNotification.Equals(notification))
    //    {
    //        activeNotification.gameObject.SetActive(false);

    //        if (queuedNotificationControllers.Any())
    //        {
    //            NotificationController nextNotification = queuedNotificationControllers[0];
    //            queuedNotificationControllers.RemoveAt(0);
    //            activeNotification = nextNotification;
    //            nextNotification.gameObject.SetActive(true);
    //        }
    //    }
    //    // if notification is not currently displayed, check if it's in the queue and remove it
    //    else
    //    {
    //        // TODO
    //    }
    //}

    //private void QueueNotification(NotificationController notification)
    //{
    //    if (!queuedNotificationControllers.Contains(notification))
    //        queuedNotificationControllers.Add(notification);
    //}

    #endregion NOTIFICATIONS
}