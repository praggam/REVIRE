using Assets.Scripts.Gameplay;
using UnityEngine;

// Responsible for all logic related to Task 1 in which user must perform combination of movement with both hands to reach out to grab a bottle and pour liquid into a glass. Current implementation allows user to grab bottle and glass with preffered hand. A point is added for every fully filled glass. Pouring while holding both bottle and glass increases the amount of points per full glass x2.

/* Difficuly affects:
 *  - glass size
 *  - glass and bottle position on the table
 */
public class Task1 : MonoBehaviour
{
    #region DIFFICULTY SETUP
    private readonly Vector3[] glassSizes = {
        new Vector3(4f, 4f, 6f),
        new Vector3(3f, 3f, 5f),
        new Vector3(2.5f, 2.5f, 4.5f)};
    #endregion

    [Tooltip("Game Object containing the Task class with loadable resources for current task.")]
    [SerializeField] Task task = null;

    [SerializeField] Container bottle = null;
    [SerializeField] Container glass = null;

    [Tooltip("Transform of this game object is used to adjust podest height.")]
    [SerializeField] GameObject putDownGlass1 = null;
    [SerializeField] GameObject putDownBottle = null;


    [Tooltip("Table boundaries used to determine correct position of grabbables on the table in respect to arms reach calibration and difficulty settings.")]
    [SerializeField] Collider tableCollider = null;


    [Tooltip("Position used to determine if glass pouring origin is above the mouth/eye level for tilt and drink motion. ")]

    private float minYLevelofGlass = 0;
    private bool glassOnTable = false;
    private bool dropGlass = false;
    private bool dropBottle = false;

    private Grabbable glassGrabbable = null;
    private Grabbable bottleGrabbable = null;

    private bool leftHanded
    {
        get
        {
            return SettingsManager.Instance.Settings.leftHanded.ToLower() == "true";
        }
    }

    const float TableHeight = 0.75f;
    const float GlassHeight = 0.04f;
    const float BottleHeight = 0.11f;

    private float GlassToFloor
    {
        get
        {
            return TableHeight + GlassHeight + SettingsManager.Instance.Settings.OffsetY;
        }
    }

    private float BottleToFloor
    {
        get
        {
            return TableHeight + BottleHeight + SettingsManager.Instance.Settings.OffsetY;
        }
    }

    private void Awake()
    {
        //ResizeGrabbables();

        if (glass)
        {
            glassGrabbable = glass.GetComponent<Grabbable>();
        }

        if (bottle)
        {
            bottleGrabbable = bottle.GetComponent<Grabbable>();
        }

        minYLevelofGlass = glass.transform.position.y;
        PositionGrabbables();

    }

    private void Start()
    {
        glass.ContainerFull += OnGlassFull;
        SettingsManager.Instance.OnSettingChanged += SettingManager_SettingChanged;
        GameManager.OnTaskStarted += GameManager_OnTaskStarted;
    }

    private void GameManager_OnTaskStarted(Task obj)
    {
        if (obj.Type == TaskType.Task1)
        {
            scored = false;
            bottle.gameObject.SetActive(true);
            glass.filled = 0;

            task.ResetSetting();
        }
    }

    private void SettingManager_SettingChanged()
    {
        UpdateLeftHandPosition();
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

    float getMinYOfGlass_old(Quaternion rot)
    {
        //stand Position
        float gTf = 0.79f + SettingsManager.Instance.Settings.OffsetY;

        float minY = GlassToFloor;//0.79f
        float rotX = Mathf.Abs(rot.x);

        if (rotX < 0.5)
            minY -= 0.02f;
        if (rotX < 0.3)
            minY -= 0.04f;
        if (rotX < 0.05)
            minY -= 0.055f;

        return minY;
    }

    float getMinYOfBottle(Quaternion rot)
    {
        //stand Position
        float bTf = 0.86f + SettingsManager.Instance.Settings.OffsetY;

        float minY = BottleToFloor;//0.86f
        float rotX = Mathf.Abs(rot.x);
        if (rotX < 0.5)
            minY -= 0.03f;
        if (rotX < 0.3)
            minY -= 0.06f;
        if (rotX < 0.05)
            minY -= 0.09f;

        return minY;
    }
    float lastGlassFilled = 0;
    public void Update()
    {
        
        Vector3 posGlass = glass.transform.position;
        Quaternion rotGlass = glass.transform.rotation;
        Vector3 posBottle = bottle.transform.position;
        Quaternion rotBottle = bottle.transform.rotation;

        // minimum height of the glass (glass not throw the Table)
        minYLevelofGlass = getMinYOfGlass(rotGlass);
        bool isOnTableArea = isObjectOnTableArea(posGlass);
        if (posGlass.y < minYLevelofGlass &&
            posGlass.y > 0.5f &&
            glassGrabbable.IsHeld && 
            isOnTableArea)
        {
            glassGrabbable.ResetPositionY(minYLevelofGlass);
        }

        float minYLevelOfBottle = getMinYOfBottle(rotBottle);
        // minimum height of the bottle
        if (posBottle.y < minYLevelOfBottle && 
            posBottle.y > 0.5f &&
            bottleGrabbable.IsHeld && 
            isObjectOnTableArea(posBottle))
        {
            bottleGrabbable.ResetPositionY(minYLevelOfBottle);
        }

        glassOnTable = (Mathf.Abs(posGlass.y - minYLevelofGlass) < 0.01);
        bool bottleOnTable = (Mathf.Abs(posBottle.y - minYLevelOfBottle) < 0.1);

        if(glassOnTable && !scored && glass.filled > lastGlassFilled)
        {
            glass.filled = lastGlassFilled;
        }
        lastGlassFilled = glass.filled;

        bool objectsOnTheTable = glassOnTable && !glassGrabbable.IsHeld && bottleOnTable & !bottleGrabbable.IsHeld;

        if (glass.filled == 0)
            scored = false;

        //task Done Checking
        if (objectsOnTheTable)
        {
            if (glass.filled >= glass.maxCapacity * 0.9)
            {
                bool glassPutDownOnArea = (distanceXZ(posGlass, putDownGlass1.transform.position) < 0.07);
                bool bottlePutDownOnArea = (distanceXZ(posBottle, putDownBottle.transform.position) < 0.07);

                bool isGlassStanding = true || (rotGlass.x > 0.68 && rotGlass.x < 0.72) || (rotGlass.x < -0.68 && rotGlass.x > -0.72);
                bool isBottleStanding = true || (rotBottle.x > 0.68 && rotBottle.x < 0.72) || (rotBottle.x < -0.68 && rotBottle.x > -0.72);
                //isGlassStanding = glass.transform.rotation.eulerAngles.z == 0;
                //isBottleStanding = bottle.transform.rotation.eulerAngles.z == 0;

                bool successfulGlassOnArea = glassPutDownOnArea && !glassGrabbable.IsHeld && isGlassStanding;
                bool successfulBottleOnArea = bottlePutDownOnArea && !bottleGrabbable.IsHeld && isBottleStanding;

                bool done = successfulGlassOnArea && successfulBottleOnArea;
                if (done && !scored)
                {
                    scored = true;
                    //glass.filled = 0;
                    ScoreManager.Instance.IncreaseScore(task, 1);
                    LSLSender.SendLsl("Task 1 - Put on Area", new float[] { 130 });

                }
            }
        }

        //string rep = HandsManager.Instance.HandLeft.transform.position.normalized.y.ToString();
        //rep += " " + HandsManager.Instance.HandRight.transform.position.normalized.y.ToString();
        //if (posBottle.x > 4.25f) rep = "OUT X";
        //Debug.LogAssertion("Bottle: x=" + posBottle.x + " y=" + posBottle.y + " z=" + posBottle.z
        //    + "\n  minY=" + minYLevelOfBottle
        //    + "\n  rot: x=" + rotBottle.x + " y=" + rotBottle.y + " z=" + rotBottle.z
        //    + "\n  rotEulerAngles: Glass x=" + rotGlass.eulerAngles.x + " Bottle x=" + rotBottle.eulerAngles.x
        //    + "\n  rotEulerAngles: Glass y=" + rotGlass.eulerAngles.y + " Bottle y=" + rotBottle.eulerAngles.y
        //    + "\n  rotEulerAngles: Glass z=" + rotGlass.eulerAngles.z + " Bottle z=" + rotBottle.eulerAngles.z
        //    + "\n" + rep);

        //drop the glass
        if (!glassGrabbable.IsHeld && !glassOnTable)
        {
            if (!dropGlass)
                LSLSender.SendLsl("Task 1 - Drop the Glass", new float[] { 141 });

            dropGlass = true;
        }
        if (glassGrabbable.IsHeld && !glassOnTable) dropGlass = false;

        //drop the bottle
        if (!bottleGrabbable.IsHeld && !bottleOnTable)
        {
            if (!dropBottle)
                LSLSender.SendLsl("Task 1 - Drop the Bottle", new float[] { 142 });

            dropBottle = true;
        }
        if (bottleGrabbable.IsHeld && !bottleOnTable) dropBottle = false;


    }
    bool scored = false;
    private float distanceXZ(Vector3 point1, Vector3 point2)
    {
        float dist = Mathf.Pow((point1.x - point2.x), 2) + Mathf.Pow((point1.z - point2.z), 2);
        dist = Mathf.Sqrt(dist);
        return dist;
    }

    private void OnGlassFull()
    {
        // check if user holding glass while pouring. If yes, increase points x2
        int points = 1;
        if (glassGrabbable.IsHeld) { points *= 2; }

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
        var bottlePos = bottleGrabbable.transform.position;
        var gpdPos = putDownGlass1.transform.position;
        var bpdPos = putDownBottle.transform.position;

        float glassZ = leftHanded ? -3.88f : -3.5f;
        float bottleZ = leftHanded ? -3.5f : -3.88f;

        glassGrabbable.transform.position = new Vector3(glassPos.x + baseX, glassPos.y + baseY, glassZ);
        bottleGrabbable.transform.position = new Vector3(bottlePos.x + baseX, bottlePos.y + baseY, bottleZ);
        glassGrabbable.InitialPosition = new Vector3(glassPos.x + baseX, glassPos.y + baseY, glassZ);
        bottleGrabbable.InitialPosition = new Vector3(bottlePos.x + baseX, bottlePos.y + baseY, bottleZ);

        putDownGlass1.transform.position = new Vector3(gpdPos.x + baseX, gpdPos.y + baseY, glassZ);
        putDownBottle.transform.position = new Vector3(bpdPos.x + baseX, bpdPos.y + baseY, bottleZ);


        return;
        //check konam ke che kar mikone va karbordesh chie
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
            ChangeInitialPosition(glass, forwardPos);

            // TODO fix bottle's pivot, must be remodeled in Blender. Temporarily fixed by adding offset.
            ChangeInitialPosition(bottle, forwardPos - 0.0f);
        }
        else
        {
            Debug.LogWarning("Task 1: Max reach forward outside of table boundary. Retreating to default values.");
        }
    }

    public void UpdateLeftHandPosition()
    {
        var glassPos = glassGrabbable.transform.position;
        var bottlePos = bottleGrabbable.transform.position;

        float glassZ = leftHanded ? -3.88f : -3.5f;
        float bottleZ = leftHanded ? -3.5f : -3.88f;

        glassGrabbable.transform.position = new Vector3(glassPos.x, glassPos.y, glassZ);
        bottleGrabbable.transform.position = new Vector3(bottlePos.x, bottlePos.y, bottleZ);
        glassGrabbable.InitialPosition = new Vector3(glassPos.x, glassPos.y, glassZ);
        bottleGrabbable.InitialPosition = new Vector3(bottlePos.x, bottlePos.y, bottleZ);
    }


    public void ChangeInitialPosition(Container container, float forwardPosition)
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