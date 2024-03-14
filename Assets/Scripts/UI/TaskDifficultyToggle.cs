using Assets.Scripts.Gameplay;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Controls UI elements of the task difficulty toggle. Toggle is designed as button which changes value when clicked. Fetches current difficulty from task settings and saves or cancels changes on UI event.
public class TaskDifficultyToggle : Saveable
{
    [SerializeField] TMP_Text valueDisplay = null;
    [SerializeField] Button button = null;

    private Difficulty difficulty = Difficulty.Easy;
    private Difficulty[] difficulties = null;

    private bool changedValue = false;

    private void Start()
    {

        difficulties = (Difficulty[]) Enum.GetValues(typeof(Difficulty));
        difficulty = SettingsManager.Instance.GetTaskDifficulty(taskType);

        UpdateValue();
    }

    private void UpdateValue()
    {
        valueDisplay.text = difficulty.ToString();
    }

    public override void CancelChanges()
    {
        changedValue = false;
    }

    public override void SaveChanges()
    {
        SettingsManager.Instance.SetTaskDifficulty(taskType, difficulty);
        changedValue = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        //if (SettingsManager.Instance != null & !changedValue)
        //{
        //    difficulty = SettingsManager.Instance.GetTaskDifficulty(taskType);
        //    UpdateValue();
        //}

        if (button != null)
        {
            button.onClick.AddListener(ChangeValue);
        }
        
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (button != null)
        {
            button.onClick.RemoveListener(ChangeValue);
        }
    }

    private void ChangeValue()
    {
        changedValue = true;

        if ((int)difficulty < difficulties.Length - 1)
        {
            difficulty = difficulties[(int)difficulty + 1];
        }
        else
        {
            difficulty = difficulties[0];
        }

        UpdateValue();
    }
}
