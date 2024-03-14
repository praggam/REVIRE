using Assets.Scripts.Gameplay;
using UnityEngine;

// Comment-out to see the liquid physics behaviour in edit mode. May misfunction if this script is dependant on other scripts that don't run in edit mode!
//[ExecuteInEditMode]
[RequireComponent(typeof(Container))]
public class LiquidBehaviour : MonoBehaviour
{
    [Tooltip("If set to true, the liquid can pour out of the container when tilted.")]
    public bool pourable = true;

    [Range(0, 1)]
    [Tooltip("Liquid flow rate per frame in fractions.")]
    public float flowVelocity = 0.02f;

    [SerializeField]
    [Tooltip("Game object with a Collider and MeshRenderer containing the liquid shader.")]
    private GameObject liquid = null;

    [Tooltip("Transform of pour origin is used to define if liquid should be poring out of the container. It should be a child of the container GameObject and cover the container opening e.g. bottle mouth.")]
    public GameObject pourOrigin = null;

    [HideInInspector]
    [Tooltip("Current level of liquid in worldspace (on y-axis).")]
    public float liquidHeight = 0.0f;

    [Tooltip("Container this behaviour is attached to.")]
    private Container container;

    [Tooltip("Collider attached to liquid game object which will determine mesh bounds for liquid height.")]
    private MeshCollider liquidCollider = null;

    [Tooltip("Material depending on _LiquidHeight shader property on y-axis in worldspace which will be dynamically set to visualize container fullness.")]
    private Material liquidMaterial = null;

    [SerializeField]
    [Tooltip("Stream prefab which will be instantiated when pouring starts. Must contain StreamBehaviour class component.")]
    private GameObject streamPrefab = null;

    [Tooltip("Stream behaviour contained in streamPrefab. When enabled, stream behaviour will render a line from pour origin to destination until end method called. Self-destroys once origin of line reached destination.")]
    private StreamBehaviour currentStream;

    [Tooltip("Find offset for lower edge. Position returns coords of object center)")]
    private float pourOriginOffset = 0.0f;

    // Calculate when liquid in tilted bottle reaches the bottle mouth to start pouring
    public bool LiquidAbovePourOrigin => pourOrigin != null &&
            liquidHeight > pourOrigin.transform.position.y - pourOriginOffset;

    private void Awake()
    {
        Debug.Assert(pourOrigin != null);

        container = GetComponent<Container>();

        flowVelocity = flowVelocity * (container.maxCapacity - container.minCapacity) + container.minCapacity;

        // TODO optimization - delay in pour start to prevent liquid from pouring when bottle is full and standing upward
        liquidCollider = liquid.GetComponent<MeshCollider>();
        liquidMaterial = liquid.GetComponent<Renderer>().material;

        // TODO this may need to be recalculated as container changes position so in Update
        pourOriginOffset = pourOrigin.GetComponent<MeshRenderer>().bounds.size.y / 2;
    }

    private void Update()
    {
        // Keep track of the top of the liquid surface in world space. Height is calculated from objects most-down position in world space plus the amount of liquid filled in proportion to objects bounds volume.
        Bounds bounds = liquidCollider.bounds;
        liquidHeight = bounds.min.y + (bounds.max.y - bounds.min.y) * container.filled;

        // Update liquid height in material component which will render the correct liquid volume.
        liquidMaterial.SetFloat("_LiquidHeight", liquidHeight);

        if (pourable && LiquidAbovePourOrigin)
        {
            container.TryPourOut();
        }
        else if (currentStream)
        {
            currentStream.End();
            currentStream = null;
        }
    }

    private void CreateStream()
    {
        // instantiate instance of a stream prefab at pour origin parented to current object
        GameObject streamObject = Instantiate(streamPrefab, pourOrigin.transform.position, Quaternion.identity, pourOrigin.transform);

        currentStream = streamObject.GetComponent<StreamBehaviour>();
    }

    public void PourOut()
    {
        if (currentStream == null)
            CreateStream();
    }
}