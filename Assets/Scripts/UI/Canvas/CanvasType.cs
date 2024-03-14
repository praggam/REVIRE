// Note - always add new items to the bottom of the list! Otherwise enum assignments in Unity inspector won´t shift correctly
public enum CanvasType
{
    Default, // used to switch to "none" to hide active canvas
    MainMenu,
    TaskMenu,
    TaskOverlay,
    OptionsMenu,
    CalibrationMenu,
    TutorialMenu,
    Notification,
    SessionSettingsMenu,
    SessionSummary
}