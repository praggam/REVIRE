using Assets.Scripts.Gameplay;
using System;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using System.Threading;
using TMPro;
using System.Collections;
using UnityEngine.XR;

// Responsible for all game events related to loading tasks, starting, ending, pausing and resuming session. Other classes can register to events to trigger behaviour on given event. Public methods can be accessed directly via UI to change session states.
public class GameManager : Singleton<GameManager>
{
    [Tooltip("List of all tasks in session in order of execution.")]
    [SerializeField] List<Task> tasks = new List<Task>();

    // TODO temp. This is temporarily attached to GameManager due to issue with CanvasManager event OnTaskEnd. See EndTask() method.
    [SerializeField] SummaryPanel summaryPanel = null;
    [SerializeField] GameObject messagePanel = null;
    [SerializeField] TextMeshPro countDownText = null;
    [SerializeField] GameObject restPointRight = null;
    [SerializeField] GameObject restPointLeft = null;
    [SerializeField] GameObject table = null;

    public static event Action<Task> OnTaskStarted;
    public static event Action<Task> OnTaskTryStarted;
    public event Action<Task> OnTaskEnded;
    public static event Action OnSessionStarted;
    public static event Action OnSessionEnded;

    [Tooltip("Assigned to currently playing task. Null if session inactive.")]
    private Task currentTask = null;

    #region PROPERTIES
    public List<Task> Tasks { get => tasks; private set => tasks = value; }
    public Task CurrentTask { get => currentTask; private set => currentTask = value; }

    bool RandomTask
    {
        get
        {
            return SettingsManager.Instance.Settings.randomTasks.ToLower() == "true";
        }
    }

    #endregion

    public bool IsLastTask()
    {

        if (RandomTask)
        {
            bool alldone = true;
            foreach (bool b in finishedTasks)
                if (!b)
                    alldone = false;
            return alldone;
        }
        else
        {
            return tasks.IndexOf(currentTask) == tasks.Count - 1;
        }
    }
    public bool IsSessionActive() => currentTask != null;
    public void PauseSession() => Time.timeScale = 0;
    public void ResumeSession() => Time.timeScale = 1;

    private void Awake()
    {
        InitializeSingleton(this);
        TimeManager.Instance.TaskTimeEnded += EndTask;
        ScoreManager.Instance.OnScored += ScoreManager_OnScored;
        UIEventsManager.OnTaskMenuOpen += PauseSession;
        UIEventsManager.OnTaskMenuClose += ResumeSession;
        HandsManager.Instance.OnBothHandsInRestLocation += HandsManager_OnBothHandsInRestLocation;

        //RandomTask = false;// SettingsManager.Instance.Settings.randomTasks.ToUpper() == "TRUE";

        
    }

    private void OnDisable()
    {
        TimeManager.Instance.TaskTimeEnded -= EndTask;
        ScoreManager.Instance.OnScored -= ScoreManager_OnScored;
        UIEventsManager.OnTaskMenuOpen -= PauseSession;
        UIEventsManager.OnTaskMenuClose -= ResumeSession;
        HandsManager.Instance.OnBothHandsInRestLocation -= HandsManager_OnBothHandsInRestLocation;
    }

    public void StartSession()
    {
        SendLslForTask(TaskType.None);

        //XRDevice.SetTrackingSpaceType(TrackingSpaceType.Stationary);
        //InputTracking.Recenter();
        OnSessionStarted?.Invoke();
        finishedTasks = new bool[3];

        //resetstatistic
        ScoreManager.Instance.ResetSessionScore();
        summaryPanel.ResetSummary();
        
        
        LoadNextTask();
    }

    public void EndSession()
    {
        EndTask();
        currentTask = null;
        OnSessionEnded?.Invoke();
    }

    private void StartTask(Task task)
    {
        currentTask = task;

        OnTaskStarted?.Invoke(currentTask);

        //Debug.Log(currentTask.Type.ToString() + "- Started!");
        SendLslForTask(currentTask.Type);

        HandsManager.Instance.StartHandTracking();

        ResumeSession();

        ScoreManager.Instance.IncreaseScore(task, 0);
    }

    private void StartTask_Load()
    {
        
        
        currentTask.Load();

        taskLoaded = true;
    }

    public void EndTask()
    {
        if (currentTask != null)
        {

            // TODO temp override as summary panel is updating values too late
            if (summaryPanel)
            {
                summaryPanel.OnTaskEnded(currentTask);
            }

            OnTaskEnded?.Invoke(currentTask);
            currentTask.Unload();
            TimeManager.Instance.ResetTimerText();
            StopTaskTimer();
        }
        else
        {
            Debug.LogWarning("Game Manager: Task Ended event called while no task is active.");
        }

        taskLoaded = false;
        waiting = false;
        if(currentTask != null)
            SendLslForTask(currentTask.Type, 3);


        PauseSession();
    }

    public void EndTask(int time)
    {
        if (currentTask != null)
        {
            currentTask.Settings.currentDuration = time;
            EndTask();
        }
        /*
        if (currentTask != null)
        {
            currentTask.Unload();
            OnTaskEnded?.Invoke(currentTask);
        }

        PauseSession();
        */
    }

    bool[] finishedTasks = new bool[3];
    public void LoadNextTask()
    {
        if (RandomTask)
        {

            System.Random rnd = new System.Random(DateTime.Now.Millisecond);
            int rndTask = rnd.Next(0, 3);

            int i = 0;
            while (finishedTasks[rndTask])
            {


                if (IsLastTask())
                {
                    EndSession();
                    return;
                }

                rndTask = rnd.Next(0, 3);
            }

            finishedTasks[rndTask] = true;

            EndTask();
            StartTask(tasks[rndTask]);
            //F: not completed
        }
        else
        {
            // load first task if no task active
            if (currentTask == null)
            {
                StartTask(tasks[0]);
            }

            // end session after last task
            else if (IsLastTask())
            {
                EndSession();
            }

            // else load next task in order
            else
            {
                EndTask();
                StartTask(tasks[tasks.IndexOf(currentTask) + 1]);
            }
        }

    }

    public void RestartCurrentTask()
    {
        if (currentTask != null)
        {
            currentTask.Restart();
            OnTaskStarted?.Invoke(currentTask);

            ResumeSession();

            Debug.Log(currentTask.Type.ToString() + "- Restarted!");
            SendLslForTask(currentTask.Type);
        }
        else
        {
            Debug.LogError("Error trying to restart a task: session inactive ");
        }
    }

    public void Quit()
    {
        Application.Quit();
    }



    public void SendLslForTask(TaskType taskType, int status = 0)
    {
        /// status : 
        /// 0 : Task Started
        /// 1 : Task Trial Strated
        /// 2 : Task Trial Finished
        /// 3 : Task Finished
        
        float[] data = new float[2];
        int i = -1;

        switch (taskType)
        {
            case (TaskType.Task1): i = 1; break;
            case (TaskType.Task2): i = 2; break;
            case (TaskType.Task3): i = 3; break;
            default: i = 0; break;
        }

        if (i == 0)
            return;

        data[0] = (i * 100) + status;
        data[1] = status;

        string msg = "";
        switch (status)
        {
            case 0:
                msg = string.Format("Task {0} Started", i);
                break;
            case 1:
                msg = string.Format("[{0}|{1}] Task {0} Trial Started", i, status);
                break;
            case 2:
                msg = string.Format("[{0}|{1}] Task {0} Trial Finished", i, status);
                break;
            case 3:
                msg = string.Format("Task {0} Finished", i, status);
                break;
        }

        LSLSender.SendLsl(msg, data);

        //LSLSender.Data = data;
        ////Thread FirstThread = new Thread(new ThreadStart(LSLSender.SendLslForTask));
        //Thread SecondThread = new Thread(() => LSLSender.SendLsl(taskType.ToString() + " Started", data));
        //SecondThread.Start();
    }

    bool waiting = false;
    public void ScoreManager_OnScored(Task obj, int score)
    {
        if (score > 0)
            SendLslForTask(currentTask.Type, 2);
        countdown = 5;
        ShowRestMessage(true);
        waiting = true;
    }
    public void HandsManager_OnBothHandsInRestLocation()
    {
        if (waiting)
        {
            SendLslForTask(currentTask.Type, 1);
            ShowRestMessage(true);
            StartTaskTimer();
            waiting = false;
        }

    }

    public void ShowRestMessage(bool show)
    {
        messagePanel.gameObject.SetActive(show);
        restPointLeft.SetActive(show);
        restPointRight.SetActive(show);

        countDownText.text = "";
    }

    int countdown = 5;

    private Coroutine taskTimer = null;
    public void StartTaskTimer()
    {
        countDownText.text = countdown.ToString();
        taskTimer = StartCoroutine(TaskTimer());
    }

    private IEnumerator TaskTimer()
    {
        //while (currentTaskTime < taskTime)
        while (countdown > 0)
        {
            yield return new WaitForSeconds(1);
            if (countdown > 0)
                countdown -= 1;
            countDownText.text = countdown.ToString();

        }

        StopTaskTimer();

        if (!taskLoaded)
            StartTask_Load();

        OnTaskTryStarted?.Invoke(currentTask);
    }


    private void StopTaskTimer()
    {
        StopCoroutine(taskTimer);
        HandsManager.Instance.ResetObjects();
        ShowRestMessage(false);
    }

    bool taskLoaded = false;

}