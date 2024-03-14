using Assets.Scripts.Gameplay;
using System;
using TMPro;
using UnityEngine;

// Responsible for keeping track of session score in permanent storage and updating score display in session. 
public class ScoreManager : Singleton<ScoreManager>
{
    [SerializeField] TMP_Text scoreDisplay = null;
    //[SerializeField] TMP_Text timeDisplay = null;

    private int RequiredAttempts = 5;

    public event Action<Task, int> OnScored; 
    private void Awake()
    {
        InitializeSingleton(this);
        UpdateScoreDisplay(0);

        GameManager.OnTaskStarted += OnScoreUpdate;
    }


    private void OnScoreUpdate(Task task)
    {
        UpdateScoreDisplay(task.Settings.currentScore);
    }

    public void ResetScore(Task task)
    {
        task.Settings.currentScore = 0;
        task.Settings.currentDuration = 0;
    }

    public void ResetSessionScore()
    {
        foreach (Task t in GameManager.Instance.Tasks)
        {
            ResetScore(t);
        }
    }

    public void IncreaseScore(Task task, int score)
    {
        task.Settings.currentScore += score;

        RequiredAttempts = task.Settings.taskDuration;
        
        if (task.Equals(GameManager.Instance.CurrentTask))
        {
            UpdateScoreDisplay(task.Settings.currentScore);
            //OnScored?.Invoke(task);
            
            GameManager.Instance.ScoreManager_OnScored(task, score);

            if (task.Settings.currentScore >= RequiredAttempts)
            {
                GameManager.Instance.EndTask(TimeManager.Instance.CurrentTaskTime);
            }
            else
            {
                if(score == 0)
                {


                }
                    
            }

        }
    }

    public void UpdateScoreDisplay(int score)
    {
        scoreDisplay.text = ConverterUtil.PointsToString(score);
    }
}