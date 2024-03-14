using System;
using System.Collections.Generic;
using UnityEngine;

// Responsible for laoding and unloading task resources based on session events and other functionalities common to all tasks. 
namespace Assets.Scripts.Gameplay
{
    [System.Serializable]
    public enum TaskType
    {
        None,
        Task1,
        Task2,
        Task3
    }

    [System.Serializable]
    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

    public class Task : MonoBehaviour
    {
        [System.Serializable]
        public class TaskSettings
        {
            public int taskDuration = 5;
            public int currentScore = 0;
            public int currentDuration = 0;
            public Difficulty difficulty = 0;
        }

        [SerializeField] TaskType type = TaskType.None;

        [Tooltip("Task name as displayed in task overlay title.")]
        public new string name = "Default";

        [Tooltip("Set to true to enable both handed grabbing for the task.Note, it is not recommended to enable both grabbing methods in one task.")]
        [SerializeField] bool bothHandedGrabbing = false;

        [Tooltip("Set to true to enable both handed grabbing for the task. Note, it is not recommended to enable both grabbing methods in one task.")]
        [SerializeField] bool kinematicGrabbing = false;

        [Tooltip("All resources that should be loaded when task starts and unloaded when it ends. Will deactivate all resources that are activated at start.")]
        [SerializeField] List<GameObject> taskResources = new List<GameObject>();

        private TaskSettings settings = new TaskSettings();

        #region PROPERTIES
        public TaskSettings Settings { get => settings; set => settings = value; }
        public TaskType Type { get => type; set => type = value; }
        #endregion


        private void Awake()
        {
            LoadResources(false);
            Settings.currentScore = 0;
        }

        private void UpdateTaskTime(int time)
        {
            Settings.currentDuration = time;
        }

        private void OnDestroy()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.StopTaskTimer();
        }

        public void Load()
        {
            HandsManager.Instance.bothHandedGrabbing = bothHandedGrabbing;
            HandsManager.Instance.kinematicGrabbing = kinematicGrabbing;

            LoadResources(true);
            TimeManager.Instance.StartTaskTimer(Settings.taskDuration);
            TimeManager.Instance.TaskTimeEnded += UpdateTaskTime;

            HandsManager.Instance.StartHandTracking();
        }

        public void Restart()
        {
            HandsManager.Instance.ResetGrabbables();
            Unload();
            Load();
            Settings.currentScore = 0;
        }

        public void Unload()
        {
            LoadResources(false);

            TimeManager.Instance.TaskTimeEnded -= UpdateTaskTime;
            Settings.currentDuration = TimeManager.Instance.StopTaskTimer();
        }

        private void LoadResources(bool value)
        {
            foreach (GameObject go in taskResources)
            {
                go.SetActive(value);

                foreach (Transform child in go.transform)
                {
                    child.gameObject.SetActive(value);
                }
            }
        }

        public void WaitForRestPoint()
        {
            foreach (GameObject go in taskResources)
            {
                go.SetActive(false);
                foreach (Transform child in go.transform)
                {
                    if (child.GetComponent<Grabbable>() != null)
                        child.GetComponent<Grabbable>().IsHeld = false;
                    child.gameObject.SetActive(false);
                }
            }
        }

        public void RestartNewAttempt()
        {
            foreach (GameObject go in taskResources)
            {
                go.SetActive(true);
                foreach (Transform child in go.transform)
                {

                    child.gameObject.SetActive(true);
                }
            }

        }

        public void ResetSetting()
        {
            Settings.currentDuration = 0;
            Settings.currentScore = 0;
        }
    }
}