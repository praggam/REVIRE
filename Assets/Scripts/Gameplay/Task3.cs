using Assets.Scripts.Gameplay;
using UnityEngine;

// Responsible for all logic related to Task 3 in which user must use both hands to grab a box from the table and place it on next podest level according to shown sequence. Sequence starts at the bottom podest level (tile), goes to the top level and switches direction, repeating until task is ended. Next tile in sequence is indicated by blue color, which changes to green on correctly positioned object is detected. 

/* Difficuly affects:
 *  - podest height (if calibration step is completed, maximum height data is adjusted to max arms reach calibration, otherwise default values are used)
 *  - (not implemented yet) box rotation - box must lay on the podest on specified side enforcing rotation movement
 */
public class Task3 : MonoBehaviour
{
    [Tooltip("Game object containing the Task class with loadable resources for current task.")]
    [SerializeField] Task task = null;

    [SerializeField]
    [Tooltip("Transform of this game object is used to adjust podest height.")]
    private GameObject podest = null;

    [SerializeField]
    [Tooltip("TODO. The box will select on which edge the box must lay to enforce object rotation.")]
    private GameObject box = null;

    [SerializeField]
    private Collider tableCollider = null;

    [Tooltip("Hard-coded podest dimensions are used if dynamic assignment not possible. This happens when no data is available from arms reach calibration.")]
    private readonly Vector3[] podestDimensions = {
        new Vector3(0.7f, 0.4f, 0.9f),
        new Vector3(0.7f, 0.5f, 0.9f),
        new Vector3(0.7f, 0.9f, 0.9f)};

    [Tooltip("Scalars for maximum podest dimension depending on difficulty setting. 1 = max podest height equal max arms reach upward")]
    private readonly float[] podestScalarsDynamic = { 0.4f, 0.6f, 0.8f };

    [SerializeField]
    private PodestLevel[] podestLevels = null;

    private int currentLevel = 0;

    private bool movingUp = true;

    private static Grabbable boxGrabbable = null;

    private void Awake()
    {
        Debug.Assert(podestLevels.Length > 0, "No podest levels assigned");

        if (box)
        {
            Debug.LogWarning("BOX ASSIGNED");
            boxGrabbable = box.GetComponent<Grabbable>();
        }

        PositionGrabbables();
        //ResizeGrabbables();
        
    }

    private void Start()
    {
        foreach (PodestLevel level in podestLevels)
        {
            level.OnGrabbableEnterArea += GrabbableEnteredArea;
            level.HideDisplay();
        }

        currentLevel = podestLevels[0].level;
        DisplayNextStep();

        GameManager.OnTaskTryStarted += GameManager_OnTaskTryStarted;
        GameManager.OnTaskStarted += GameManager_OnTaskStarted;
    }

    private void GameManager_OnTaskStarted(Task obj)
    {
        if (obj.Type == TaskType.Task3)
        {
            boxGrabbable.InitialPosition = podestLevels[0].transform.position;
            boxGrabbable.transform.position = podestLevels[0].transform.position;
            HandsManager.Instance.ResetGrabbables();

            movingUp = true;

            currentLevel = podestLevels[0].level;
            podestLevels[2].HideDisplay();

            DisplayNextStep();
            
            podest.SetActive(true);
            box.SetActive(true);

            task.ResetSetting();
        }
    }

    private void GameManager_OnTaskTryStarted(Task obj)
    {
        if (obj.Type == TaskType.Task3)
        {
            HandsManager.Instance.ResetGrabbables();

            box.SetActive(true);
            podest.SetActive(true);
            ActivatePodestLevels(true);
        }
    }

    bool scored = false;
    // TODO optimization
    private void GrabbableEnteredArea(int level)
    {
        //Debug.LogWarning(string.Format("Grabbable entered podest level {0}", level));

        if (movingUp && level == currentLevel + 1)
        {
            ScoreManager.Instance.IncreaseScore(task, 1);
            podestLevels[currentLevel].gameObject.SetActive(false);
            currentLevel = level;

            // check if reached top of podest and switch directions if so
            bool reachedTop = currentLevel == 2;//podestLevels[podestLevels.Length - 1].level;
            if (reachedTop)
            {
                movingUp = false;
            }

            DisplayNextStep();
            ActivatePodestLevels(false);
        }
        else if (!movingUp && level == currentLevel - 1)
        {
            ScoreManager.Instance.IncreaseScore(task, 1);
            podestLevels[currentLevel].gameObject.SetActive(false);
            currentLevel = level;

            bool reachedBottom = currentLevel == 0;
            if (reachedBottom)
            {
                movingUp = true;
            }

            DisplayNextStep();
            ActivatePodestLevels(false);
        }
    }

    void ActivatePodestLevels(bool active)
    {
        foreach (PodestLevel pl in podestLevels)
            pl.ChangeLevelActive = active;
    }

    // will visualize where box must be put next
    private void DisplayNextStep()
    {
        Vector3 boxPos = boxGrabbable.transform.position;
        boxGrabbable.InitialPosition = new Vector3(boxPos.x, boxPos.y, boxPos.z);

        PodestLevel level = movingUp ? podestLevels[currentLevel + 1] : podestLevels[currentLevel - 1];

        if (level == null)
        {
            Debug.LogError("Task 3: Level not found.");
        }

        level.gameObject.SetActive(true);
        level.Highlight();
    }

    const float TableHeight = 0.75f;
    const float BoxHeight = 0.08f;
    private float BoxToFloor
    {
        get
        {
            return TableHeight + BoxHeight + SettingsManager.Instance.Settings.OffsetY;
        }
    }

    int resetNum = 0;
    float getMinY(Vector3 pos)
    {
        float min = 0.83f;
        min = BoxToFloor;
        float farX = SettingsManager.Instance.Settings.OffsetX + 4.17f;
        float nearX = SettingsManager.Instance.Settings.OffsetX + 3.67f;
        if (pos.x > nearX && pos.x < farX)
        {
            if (pos.z < -3.0f && pos.z > -3.4f) min += 0.164f; //Level 2
            if (pos.z < -3.4f && pos.z > -3.8f) min += 0.103f; //Level 1
            if (pos.z < -3.8f && pos.z > -4.2f) min += 0.044f; //Level 0
        }
        /*
        if (!isObjectOnTableArea(boxGrabbable.transform.position))
            min = -0.01f;
        */
        Rect lvl2 = new Rect(3.9f, -3.4f, 0.4f, 0.4f);
        Rect lvl1 = new Rect(3.9f, -3.4f, 0.4f, 0.4f);


        return min;
    }

    bool isObjectOnTableArea(Vector3 pos)
    {
        float baseX = SettingsManager.Instance.Settings.OffsetX;

        float tableMinX = 3.25f + baseX;
        float tableMaxX = 4.25f + baseX;
        float tableMinZ = -4.55f;
        float tableMaxZ = -2.55f;
        bool ObjectOnTableArea = (pos.x < tableMaxX && pos.x > tableMinX) && (pos.z < tableMaxZ && pos.z > tableMinZ);

        return ObjectOnTableArea;
    }

    private void Update()
    {
        Vector3 posBox = box.transform.position;
        float min = getMinY(posBox);
        if (posBox.y < min & isObjectOnTableArea(posBox))
        {
            boxGrabbable.ResetPositionY(min);
            resetNum++;
        }
        
        /*
        string rep = HandsManager.Instance.HandLeft.transform.position.normalized.y.ToString() + "   " + HandsManager.Instance.HandRight.transform.position.normalized.y.ToString(); ; ;
        if (posBox.x > 4.25f) rep = "OUT X";
        Debug.LogAssertion("Box: x=" + posBox.x + " y=" + posBox.y + " z=" + posBox.z
            + "\n  minY=" + min
            + "\n RESET = " + resetNum
            //+ "\n  rot: x=" + rotBottle.x + " y=" + rotBottle.y + " z=" + rotBottle.z
            + "\n" + rep); ;
        */
    }




    // set podest height according to difficulty level chosen. If user completed calibration step, use upward arms reach to define maximum podest height. Otherwise use hard-coded dimensions
    private void ResizeGrabbables()
    {
        Vector3 podestScale = podest.transform.localScale;

        if (SettingsManager.Instance.Settings.maxReachUpward != 0)
        {
            // max arms reach is defined in the world space.
            // TODO in calibration check if upward reach is taller than table height

            float up = podestScalarsDynamic[(int)task.Settings.difficulty] * (SettingsManager.Instance.Settings.maxReachUpward - tableCollider.bounds.max.y);

            podestScale = new Vector3(podestScale.x, up, podestScale.z);
        }
        else
        {
            //if (task.settings.difficulty < 0 || (int)task.settings.difficulty > podestDimensions.Length)
            //    task.settings.difficulty = 0;

            podestScale = podestDimensions[(int)task.Settings.difficulty];
        }

        podest.transform.localScale = podestScale;
    }

    private void PositionGrabbables()
    {
        float baseX = SettingsManager.Instance.Settings.OffsetX;
        float baseY = SettingsManager.Instance.Settings.OffsetY;

        var boxPos = boxGrabbable.transform.position;
        var podestPos = podest.transform.position;

        boxGrabbable.transform.position = new Vector3(boxPos.x + baseX, boxPos.y + baseY, boxPos.z);
        boxGrabbable.InitialPosition = new Vector3(boxPos.x + baseX, boxPos.y + baseY, boxPos.z);
        podest.transform.position = new Vector3(podestPos.x + baseX, podestPos.y + baseY, podestPos.z);

        return;
    }
}