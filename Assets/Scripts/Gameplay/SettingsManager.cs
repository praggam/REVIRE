using Assets.Scripts.Gameplay;
using UnityEngine;
using static Assets.Scripts.Gameplay.Task;
using System;
using System.Threading;
using LSL;
using TMPro;

// Responsible for loading/saving settings and progress between sessions. Contains a nested serializable class UserSettings which stores all user related non task-specific data. Task specific data is stored in Task class. If any custom user settings exist, they are loaded at application start. Otherwise default values are used. Currently implemented using JSON with PlayerPrefs. 
public class SettingsManager : Singleton<SettingsManager>
{
    [System.Serializable]
    public class UserSettings
    {
        public float maxReachUpward = 0;
        public float maxReachForward = 0;
        public string randomTasks = "false";
        public string leftHanded = "false";
        public float OffsetX = 0.0f;
        public float OffsetY = 0.0f;
    }

    [SerializeField] TMP_Text TextLeftHanded = null;
    [SerializeField] TMP_Text TextRandomTasks = null;


    
    public event Action OnSettingChanged;

    #region JSON STRINGS
    private readonly string settings_JSON = "settings";
    #endregion

    #region PROPERTIES
    private UserSettings settings = null;
    public UserSettings Settings { get => settings; set => settings = value; }
    #endregion

    private void Awake()
    {
        InitializeSingleton(this);
        LSLSender.init();
        LSLSender.SendLsl("Game Started", new float[2]);
    }

    private void OnEnable()
    {
        LoadSettings();
        TextLeftHanded.text = settings.leftHanded.ToUpper();
        settings.OffsetX = 0f;
        settings.OffsetY = 0f;
    }

    private void OnDisable()
    {
        SaveSettings();
    }

    public void ResetSettings()
    {
        // reset task-specific settings
        foreach (Task t in GameManager.Instance.Tasks)
        {
            if (PlayerPrefs.HasKey(t.Type.ToString()))
            {
                PlayerPrefs.DeleteKey(t.Type.ToString());
            }
        }

        // reset user settings
        if (PlayerPrefs.HasKey(settings_JSON))
        {
            PlayerPrefs.DeleteKey(settings_JSON);
        }
    }

    public void LoadSettings()
    {
        // load task settings
        foreach (Task t in GameManager.Instance.Tasks)
        {
            t.Settings = LoadTaskSettings(t);
        }

        // load user settings
        if (PlayerPrefs.HasKey(settings_JSON))
        {
            Settings = JsonUtility.FromJson<UserSettings>(PlayerPrefs.GetString(settings_JSON));
            if(settings == null)
            {
                settings = new UserSettings();
                var json2 = JsonUtility.ToJson(settings);
                PlayerPrefs.SetString(settings_JSON, json2);
            }
        }
        else
        {
            // load default settings
            Settings = new UserSettings();
        }
        TextLeftHanded.text = settings.leftHanded.ToUpper();
        TextRandomTasks.text = settings.randomTasks.ToUpper();
    }

    public void SaveSettings()
    {
        foreach (Task t in GameManager.Instance.Tasks)
        {
            if (t.Settings != null)
            {
                var json = JsonUtility.ToJson(t.Settings);
                PlayerPrefs.SetString(t.Type.ToString(), json);
            }
        }

        // save user settings
        var json2 = JsonUtility.ToJson(Settings);
        PlayerPrefs.SetString(settings_JSON, json2);


        //SettingChanged();
    }

    public void ToggleLeftHanded()
    {
        settings.leftHanded = (!(bool.Parse(settings.leftHanded))).ToString();
        TextLeftHanded.text = settings.leftHanded.ToUpper();

        SaveSettings();
        OnSettingChanged?.Invoke();
    }

    public void ToggleRandomTasks()
    {
        settings.randomTasks = (!(bool.Parse(settings.randomTasks))).ToString();
        TextRandomTasks.text = settings.randomTasks.ToUpper();

        SaveSettings();
    }

    public void SetOffsetX(float val)
    {
        settings.OffsetX = val;
        SaveSettings();
    }

    public void SetOffsetY(float val)
    {
        settings.OffsetY = val;
        SaveSettings();
    }

    public void SetTaskDuration(TaskType type, int duration)
    {
        //GetTask(type).Settings.taskDuration = duration;
        if(type == TaskType.Task1)
        {
            foreach (Task task in GameManager.Instance.Tasks)
            {
                task.Settings.taskDuration = duration;
            }
        }

        SaveSettings();
    }

    public void SetTaskDifficulty(TaskType type, Difficulty difficulty)
    {
        GetTask(type).Settings.difficulty = difficulty;
        SaveSettings();
    }

    public void SetMaxArmsReach(float upward, float forward)
    {
        Settings.maxReachForward = forward;
        Settings.maxReachUpward = upward;
        SaveSettings();
    }

    public int GetTaskDuration(TaskType type)
    {
        return GetTask(type).Settings.taskDuration;
    }

    internal Difficulty GetTaskDifficulty(TaskType type)
    {
        return GetTask(type).Settings.difficulty;
    }

    private Task GetTask(TaskType type)
    {
        foreach (Task task in GameManager.Instance.Tasks)
        {
            if (task.Type == type)
                return task;
        }

        Debug.LogError("Task not found in the scene: " + type);
        return null;
    }

    private TaskSettings LoadTaskSettings(Task task)
    {
        if (PlayerPrefs.HasKey(task.Type.ToString()))
        {
            return JsonUtility.FromJson<TaskSettings>(PlayerPrefs.GetString(task.Type.ToString()));
        }
        else
        {
            // load default settings
            return new TaskSettings();
        }
    }

    public void SendLSLData()
    {
        System.Random rnd = new System.Random();



        // create stream info and outlet
        StreamInfo info = new StreamInfo("TestCSharp", "EEG", 8, 100, LSL.channel_format_t.cf_float32, "sddsfsdf");
        StreamOutlet outlet = new StreamOutlet(info);
        float[] data = new float[8];
        int i = 100;
        while (i-- > 99)
        {
            // generate random data and send it
            for (int k = 0; k < data.Length; k++)
                data[k] = rnd.Next(-100, 100);
            outlet.push_sample(data);
            Thread.Sleep(10);
        }

    }
}