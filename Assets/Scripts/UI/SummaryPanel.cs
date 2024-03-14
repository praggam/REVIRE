using Assets.Scripts.Gameplay;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Repsonsible for updating the task and total summary UI when task or respectively session end event is triggered. Resets values when session is completed.
public class SummaryPanel : MonoBehaviour
{
    [System.Serializable]
    protected class TaskSummary
    {
        public Task task = null;

        public TMP_Text name;
        public TMP_Text score;
        public TMP_Text time;

        public void SetValues()
        {
            if (task == null) return;

            name.text = task.name;
            score.text = ConverterUtil.PointsToString(task.Settings.currentScore);
            time.text = ConverterUtil.MinutesSecondsToStringDescriptive(TimeManager.Instance.CurrentTaskTime);
        }

        public void Reset()
        {
            score.text = "-";
            time.text = "-";
        }
    }

    [SerializeField]
    private List<TaskSummary> taskSummaries = new List<TaskSummary>();

    [SerializeField]
    private TaskSummary totals = null;

    private void Start()
    {
        GameManager.Instance.OnTaskEnded += OnTaskEnded;
        GameManager.OnSessionEnded += OnSessionEnded;
    }

    private void OnDisable()
    {
        if(GameManager.Instance)
        {
            GameManager.Instance.OnTaskEnded -= OnTaskEnded;
            GameManager.OnSessionEnded -= OnSessionEnded;
        }

    }

    public void OnTaskEnded(Task task)
    {
        UpdateTaskSummary(task);

        if (GameManager.Instance.IsLastTask())
        {
            UpdateTotals();
        }
    }

    private void OnSessionEnded()
    {
        ResetSummary();
    }

    public void UpdateTaskSummary(Task task)
    {
        foreach (TaskSummary summary in taskSummaries)
        {
            if (summary.task.Equals(task))
            {
                summary.SetValues();
                return;
            }
        }
    }

    public void UpdateTotals()
    {
        int totalScore = 0;
        int totalTime = 0;

        foreach (Task task in GameManager.Instance.Tasks)
        {
            totalScore += task.Settings.currentScore;
            totalTime += task.Settings.currentDuration;
        }

        totals.score.text = ConverterUtil.PointsToString(totalScore);
        totals.time.text = ConverterUtil.MinutesSecondsToStringDescriptive(totalTime);
    }

    public void ResetSummary()
    {
        foreach (TaskSummary summary in taskSummaries)
        {
            summary.Reset();
        }

        totals.Reset();
    }
}