using Assets.Scripts.Gameplay;
using System.Collections;
using TMPro;
using UnityEngine;

// Responsible for all logic related to Task 2 in which user simulates drinking from glass or bottle. Drinking from glass is activated when glass is placed within a defined range of user's mouth as tracked by the HMD (through eye level tracking). Drinking from bottle adds an additional difficulty aspect as bottle must be tilted above user's eye level. The tilt angle depends on the leevl of liquid in the bottle. 

/* Difficuly affects:
 *  - glass size
 *  - glass and bottle position on the table
 *  - bottle minimum capacity - bottle gets filled up more often to adjust to smaller/larger tilting angle requirement
 */
public class Task2 : MonoBehaviour
{
    #region DIFFICULTY SETUP
    private readonly Vector3[] glassSizes = {
        new Vector3(4f, 4f, 6f),
        new Vector3(3f, 3f, 5f),
        new Vector3(2.5f, 2.5f, 4.5f)};

    private readonly float[] BottleMinCapacities = {
        0.6f, 0.4f, 0.1f};
    #endregion

    [Tooltip("Game Object containing the Task class with loadable resources for current task.")]
    [SerializeField] Task task = null;

    [SerializeField] Container bottle = null;
    [SerializeField] Container glass = null;

    [Tooltip("Table boundaries used to determine correct position of grabbables on the table in respect to arms reach calibration and difficulty settings.")]
    [SerializeField] Collider tableCollider = null;

    [Tooltip("Attached to collider, triggers event when object enters the trigger collider area. Note that to work efficiently, this object should be set up under correct layer mask which interacts with grabbable objects and ignores the rest (Layer Collision Matrix in Physics Settings).")]
    [SerializeField] OnTriggerEvent drinkableAreaTrigger = null;

    [Tooltip("Position used to determine if glass pouring origin is above the mouth/eye level for tilt and drink motion. ")]
    private Transform eyeLevel = null;
    private bool bottleAboveEyeLevel = false;

    [Tooltip("Set to true if glass is held within drinkable area boundary.")]
    private bool glassInRange = false;

    [Tooltip("Set to true if bottle is held within drinkable area boundary and above user's eye level (HMD center)")]
    private bool bottleInRange = false;

    [Tooltip("Gain 1 points for every x seconds of holding bottle in drinking position.")]
    private readonly float secondsPerPoint = 5f;

    [Tooltip("Transform of this game object is used to adjust podest height.")]
    [SerializeField] GameObject putDownGlass2 = null;

    private Grabbable glassGrabbable = null;
    private float drinkingForSeconds = 0f;

    private float tableLevelY = 0;
    private bool glassOnTable = false;
    private bool dropGlass = false;

    [Header("Debugging")]
    [SerializeField] bool debug = false;
    [SerializeField] TMP_Text isDrinkingLabel = null;
    [SerializeField] TMP_Text drinkingForSecondsLabel = null;
    [SerializeField] TMP_Text eyeLevelLabel = null;
    [SerializeField] TMP_Text bottleMouthLevelLabel = null;

    const float TableHeight = 0.75f;
    const float GlassHeight = 0.04f;
    private float GlassToFloor
    {
        get
        {
            return TableHeight + GlassHeight + SettingsManager.Instance.Settings.OffsetY;
        }
    }

    private void Awake()
    {
        //ResizeGrabbables();
        //StartCoroutine(PositionGrabbables());
        //ChangeBottleCapacity();
        
        if(glass)
            glassGrabbable = glass.GetComponent<Grabbable>();

        if(drinkableAreaTrigger)
            eyeLevel = drinkableAreaTrigger.gameObject.transform;

        PositionGrabbables();

        tableLevelY = glass.transform.position.y;

    }

    public void Start()
    {
        GameManager.OnTaskStarted += GameManager_OnTaskStarted;
    }

    private void GameManager_OnTaskStarted(Task obj)
    {
        if (obj.Type == TaskType.Task2)
        {
            scored = false;
            glass.filled = glass.maxCapacity;

            task.ResetSetting();
        }
    }

    private void OnEnable()
    {
        drinkableAreaTrigger.TriggerEntered += OnContainerEnterDrinkableArea;
        drinkableAreaTrigger.TriggerExited += OnContainerExitDrinkableArea;
        glass.ContainerEmpty += OnGlassEmpty;
    }

    private void OnDisable()
    {
        drinkableAreaTrigger.TriggerEntered -= OnContainerEnterDrinkableArea;
        drinkableAreaTrigger.TriggerExited -= OnContainerExitDrinkableArea;
        glass.ContainerEmpty -= OnGlassEmpty;
    }

    bool isObjectOnTableArea(Vector3 pos)
    {
        float tableMinX = 3.25f + SettingsManager.Instance.Settings.OffsetX;
        float tableMaxX = 4.25f + SettingsManager.Instance.Settings.OffsetX;
        float tableMinZ = -4.55f;
        float tableMaxZ = -2.55f;
        bool ObjectOnTableArea = (pos.x < tableMaxX && pos.x > tableMinX) && (pos.z < tableMaxZ && pos.z > tableMinZ);

        return ObjectOnTableArea;
    }
    public void FillGlass()
    {
        glass.filled = glass.maxCapacity;
    }
    float getMinYOfGlass_old(Quaternion rot)
    {
        float minY = GlassToFloor;//0.79f
        float rotX = Mathf.Abs(rot.x);

        if (rotX < 0.5)
            minY -= 0.02f;
        if (rotX < 0.3)
            minY -= 0.04f;
        if (rotX < 0.05)
            minY -= 0.055f;

        return minY;
        /*
        //stand Position
        float minY = 0.79f;
        float rotX = Mathf.Abs(rot.x);
        if (rotX < 0.5)
            minY = 0.77f;
        if (rotX < 0.3)
            minY = 0.75f;
        if (rotX < 0.05)
            minY = 0.735f;

        return minY;*/
    }
    float getMinYOfGlass(Quaternion rot)
    {
        //stand Position
        float gTf = 0.79f + SettingsManager.Instance.Settings.OffsetY;

        float minY = GlassToFloor;//0.79f
        float rotX = Mathf.Abs(rot.eulerAngles.x);

        if (isRange(rotX, 45, 70) || isRange(rotX, 290, 315))
            minY -= 0.005f;

        if (isRange(rotX, 20, 45) || isRange(rotX, 315, 340))
            minY -= 0.005f;

        if (isRange(rotX, 0, 20) || isRange(rotX, 340, 360))
            minY -= 0.02f;

        return minY;
    }

    bool isRange(float val, float low, float high)
    {
        return val >= low && val < high;

    }

    float r = 0; 
    private void Update()
    {
        UpdateEyeAndBottleLevel();
        UpdateEyeAndGlassLevel();
        // Debug.LogWarning(string.Format("Liquid above pour origin: {0}", bottle.Liquid.LiquidAbovePourOrigin));

        Vector3 posGlass = glass.transform.position;
        Quaternion rotGlass = glass.transform.rotation;

        var w = glass.transform.rotation.z;
        if (Mathf.Pow(r - w, 2) > 0.0001)
        {
            Debug.Log("Rotation: w=" + w);
            r = w;
        }

        float[] z_ax = new float[] { 0.1f, 0.20f, 0.25f, 0.30f, 0.35f, 0.37f, 0.39f, 0.41f, 0.45f, 0.50f }; //rotation
        float[] fill = new float[] { 0.8f, 0.60f, 0.50f, 0.40f, 0.30f, 0.20f, 0.12f, 0.05f, 0.00f, 0.00f }; //remain water %
        bool overflow = false;
        for (int i = 0; i < z_ax.Length; i++)
            overflow = overflow || (w > z_ax[i] && glass.filled > fill[i]);

        // minimum height of the glass (glass not throw the Table)
        float minYLevelofGlass = getMinYOfGlass(rotGlass);
        bool isOnTableArea = isObjectOnTableArea(posGlass);
        if (posGlass.y < minYLevelofGlass && glassGrabbable.IsHeld && isOnTableArea)
        {
            glassGrabbable.ResetPositionY(minYLevelofGlass);
        }

        if (glassInRange && overflow)
        {
            if(glass.filled == glass.maxCapacity)
                LSLSender.SendLsl("Drinking Started", new float[] { 204 });
            
            glass.TryPourOut();

            if(glass.filled == 0)
                LSLSender.SendLsl("Drinking Finished", new float[] { 205 });
        }
        
        if(bottleInRange && bottleAboveEyeLevel && bottle.Liquid.LiquidAbovePourOrigin)
        {
            drinkingForSeconds += Time.deltaTime;

            if (debug)
            {
                drinkingForSecondsLabel.text = drinkingForSeconds.ToString();
                if(debug) Debug.LogWarning(string.Format("Drinking for seconds: {0}", drinkingForSeconds));
            }
                

            if (drinkingForSeconds >= secondsPerPoint)
            {
                //ScoreManager.Instance.IncreaseScore(task, 1);
                drinkingForSeconds = 0f;
            }
        }

        glassOnTable = (Mathf.Abs(posGlass.y - minYLevelofGlass) < 0.01);

        if (glassOnTable && !glassGrabbable.IsHeld)
        {
            bool glassPutDownOnArea = (distanceXZ(posGlass, putDownGlass2.transform.position) < 0.07);
            if (glassPutDownOnArea && glass.filled < 0.05 && !scored)
            {
                LSLSender.SendLsl("Glass on the Area", new float[] { 230 });
                ScoreManager.Instance.IncreaseScore(task, 1);
                //glass.filled = glass.maxCapacity;
                scored = true;
            }            
        }

        if (glass.filled > 0.05)
            scored = false;

        //drop the glass
        if (!glassGrabbable.IsHeld && !glassOnTable)
        {
            if (!dropGlass)
                LSLSender.SendLsl("Task 2 - Drop the Glass", new float[] { 243 });

            dropGlass = true;
        }
        if (glassGrabbable.IsHeld && !glassOnTable) dropGlass = false;
    }
    bool scored = false;
    private float distanceXZ(Vector3 point1, Vector3 point2)
    {
        float dist = Mathf.Pow((point1.x - point2.x), 2) + Mathf.Pow((point1.z - point2.z), 2);
        dist = Mathf.Sqrt(dist);
        return dist;
    }

    // updates current eye and bottle mouth level y-coordinates and displayes the value in debug mode. 
    void UpdateEyeAndBottleLevel()
    {
        float eyeY = eyeLevel.position.y;
        float bottleY = bottle.Liquid.pourOrigin.transform.position.y;

        //bottleAboveEyeLevel = eyeY <= bottleY;
        

        if (debug)
        {
            isDrinkingLabel.text = bottleAboveEyeLevel.ToString();
            eyeLevelLabel.text = eyeY.ToString();
            bottleMouthLevelLabel.text = bottleY.ToString();
        }
    }
    void UpdateEyeAndGlassLevel()
    {
        float eyeY = eyeLevel.position.y;
        float bottleY = bottle.Liquid.pourOrigin.transform.position.y;
        float glassY = glass.Liquid.pourOrigin.transform.position.y;// bottle.Liquid.pourOrigin.transform.position.y;

        bool laststate = bottleAboveEyeLevel;
        bottleAboveEyeLevel = eyeY <= glassY;

        if (bottleAboveEyeLevel & !laststate)
            Debug.Log("VR Above Eye Level");

        if (debug)
        {
            isDrinkingLabel.text = bottleAboveEyeLevel.ToString();
            eyeLevelLabel.text = eyeY.ToString();
            bottleMouthLevelLabel.text = bottleY.ToString();
        }
    }

    private void OnContainerEnterDrinkableArea(Collider collider)
    {
        if(collider.CompareTag("Glass"))
        {
            
            // check if user is holding the glass
            if (glassGrabbable && glassGrabbable.IsHeld && bottleAboveEyeLevel)
            {
                if (!glassInRange)
                {
                    Debug.Log("VR Glass in Range!");
                }
                glassInRange = true;
                
            }
        }
        else if(collider.CompareTag("Bottle"))
        {

                bottleInRange = true;
        }
    }

    private void OnContainerExitDrinkableArea(Collider collider)
    {
        if (collider.CompareTag("Glass"))
        {
            glassInRange = false;
        }
        else if (collider.CompareTag("Bottle"))
        {
            // comment out if points should only be counted for holdling bottle in correct position for 'consecutive' x-seconds.
            //drinkingForSeconds = 0f;
            bottleInRange = false;
        }
    }

    // glass only empties when held close to mouth
    private void OnGlassEmpty()
    {
        int points = 1;
        //ScoreManager.Instance.IncreaseScore(task, points);
    }

    // set glass size according to difficulty level chosen
    private void ResizeGrabbables()
    {
        if (task.Settings.difficulty < 0 || (int)task.Settings.difficulty > glassSizes.Length)
            task.Settings.difficulty = 0;

        glass.transform.localScale = glassSizes[(int)task.Settings.difficulty];
    }

    // position objects at user's max reach. Depends on difficulty level set and calibration settings
    private void PositionGrabbables()
    {
        float baseX = SettingsManager.Instance.Settings.OffsetX;
        float baseY = SettingsManager.Instance.Settings.OffsetY;

        var glassPos = glassGrabbable.transform.position;
        var gpdPos = putDownGlass2.transform.position;

        glassGrabbable.transform.position = new Vector3(glassPos.x + baseX, glassPos.y + baseY, glassPos.z);
        putDownGlass2.transform.position = new Vector3(gpdPos.x + baseX, gpdPos.y + baseY, gpdPos.z);

        return;
        /*
        yield return new WaitWhile(() => SettingsManager.Instance == null);

        float forwardPos = SettingsManager.Instance.Settings.maxReachForward;
        float tableSpace = tableCollider.bounds.max.x - forwardPos;

        if (task.Settings.difficulty == Difficulty.Easy)
        {
            forwardPos += tableSpace * 0.4f;
        }
        else if (task.Settings.difficulty == Difficulty.Normal)
        {
            forwardPos += tableSpace * 0.2f;
        }

        //  make sure it is within table boundary and if not, apply default values
        if (forwardPos > tableCollider.bounds.min.x && forwardPos < tableCollider.bounds.max.x)
        {
            //ChangeInitialPosition(glass, forwardPos);

            // TODO fix bottle's pivot, must be remodeled in Blender. Temporarily fixed by adding offset.
            //ChangeInitialPosition(bottle, forwardPos - 0.3f);
        }
        else
        {
            Debug.LogWarning("Task 1: Max reach forward outside of table boundary. Retreating to default values.");
        }
        */
    }

    // change minimum liquid level in the bottle to enable easier pouring depending on task difficulty level. The lower the min. liquid level, the more difficult it ist to tilt bottle to pour out liquid.
    private void ChangeBottleCapacity()
    {
        bottle.minCapacity = BottleMinCapacities[(int)task.Settings.difficulty];
    }

    private void ChangeInitialPosition(Container container, float forwardPosition)
    {
        Vector3 newPosition = new Vector3(forwardPosition, container.transform.position.y, container.transform.position.z);

        container.transform.position = newPosition;

        Grabbable grabbable = container.gameObject.GetComponent<Grabbable>();

        if (grabbable)
        {
            grabbable.InitialPosition = newPosition;
        }
    }
}
