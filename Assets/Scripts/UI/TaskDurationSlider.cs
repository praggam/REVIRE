using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// Controls UI elements of the task duration slider, fetches current duration from task settings and saves or cancels changes on UI event.
public class TaskDurationSlider : Saveable
{
    [SerializeField] Button decreaseButton = null;
    [SerializeField] Button increaseButton = null;
    [SerializeField] TMP_Text valueDisplay = null;

    [Tooltip("slider values will increase according to timestep, e.g for timestep 15: duration 1 = 15 seconds, 2 = 30 seconds etc.")]
    private int taskDuration = 1;

    [Tooltip("Timestep in seconds.")]
    private readonly int timestep = 1;

    [Tooltip("Minumum task duration defined in number of timesteps. E.g. if timestep is 10, duration of 1 will equal to 10 seconds.")]
    private readonly int minDuration = 1;

    [Tooltip("Maximum task duration defined in number of timesteps. E.g. if timestep is 10, maxDuration 12 will equal to 120 seconds.")]
    private readonly int maxDuration = 20;


    protected override void Awake()
    {
        base.Awake();
        taskDuration = Math.Min(maxDuration, SettingsManager.Instance.GetTaskDuration(taskType));
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();

        //  Add listeners to Increase/Decrease events to change value on button click
        if (decreaseButton != null)
            decreaseButton.onClick.AddListener(Decrease);

        if (increaseButton != null)
            increaseButton.onClick.AddListener(Increase);

        UpdateUIValues();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (decreaseButton != null)
            decreaseButton.onClick.RemoveListener(Decrease);

        if (increaseButton != null)
            increaseButton.onClick.RemoveListener(Increase);
    }

    public override void SaveChanges()
    {
        SettingsManager.Instance.SetTaskDuration(taskType, taskDuration * timestep);
    }

    public override void CancelChanges()
    {
        // currently not doing anything
    }

    public void Increase()
    {
        ++taskDuration;

        UpdateUIValues();
    }

    public void Decrease()
    {
        --taskDuration;

        UpdateUIValues();
    }

    // set buttons interactable if current task duration is resp. larger than minimum and smaller than maximum. Update displayed text to match current duration.
    private void UpdateUIValues()
    {
        if (decreaseButton != null)
        {
            decreaseButton.interactable = (taskDuration > minDuration);
        }

        if (increaseButton != null)
        {
            increaseButton.interactable = (taskDuration < maxDuration);
        }

        valueDisplay.text = (taskDuration).ToString();
    }
}
