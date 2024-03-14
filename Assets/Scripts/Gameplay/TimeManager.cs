using System;
using System.Collections;
using TMPro;
using UnityEngine;

// Responsible for all time related events such as starting and ending task timer and updating timer display in session. Timer must be initialized by external class to start counting for current task. Triggers event when task time is completed unless stopped earlier which can happen if user ends task prematurely.
public class TimeManager : Singleton<TimeManager>
{
    [SerializeField] TMP_Text timerDisplay = null;

    [Tooltip("Time measured for current task from initialization.")]
    public int CurrentTaskTime = 0;

    private Coroutine taskTimer = null;
    public event Action<int> TaskTimeEnded;

    private void Awake()
    {
        InitializeSingleton(this);
    }

    public void StartTaskTimer(int taskTime)
    {
        CurrentTaskTime = 0;
        //timerDisplay.text = ConverterUtil.MinutesSecondsToString(taskTime);
        ResetTimerText();// timerDisplay.text = ConverterUtil.MinutesSecondsToString(0);
        taskTimer = StartCoroutine(TaskTimer(taskTime));
    }

    // returns the task duration
    public int StopTaskTimer()
    {
        if (taskTimer != null)
        {
            StopCoroutine(taskTimer);
        }

        return CurrentTaskTime;
    }

    private IEnumerator TaskTimer(int taskTime)
    {
        //while (currentTaskTime < taskTime)
        while (true)
        {
            yield return new WaitForSeconds(1);
            CurrentTaskTime += 1;
            //timerDisplay.text = ConverterUtil.MinutesSecondsToString(taskTime - currentTaskTime);
            timerDisplay.text = ConverterUtil.MinutesSecondsToString(CurrentTaskTime);
        }

        // trigger time ended event to stop the task
        //TaskTimeEnded?.Invoke(currentTaskTime);
    }

    public void ResetTimerText()
    {
        timerDisplay.text = ConverterUtil.MinutesSecondsToString(0);
    }
}