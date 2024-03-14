using Assets.Scripts.Gameplay;
using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LiquidBehaviour))]
public class Container : MonoBehaviour
{
    [Range(0, 1)]
    [Tooltip("Value between 0 and 1. The volume is relative to object's renderer bounds which does not account for special shapes such as thinner mouth of the bottle so the volumes may appear unnatural. ")]
    public float minCapacity = 0.05f;

    [Range(0, 1)]
    [Tooltip("Value between 0 and 1. The volume is relative to object's renderer bounds which does not account for special shapes such as thinner mouth of the bottle so the volumes may appear unnatural, e.g. when bottle stands upward it appear to have more liquid than when tilted. Lowering the max capacity also helps reduce overflowing i.e. when container is 100% full and it stands upwards, the liquid animation will show upward-flow. ")]
    public float maxCapacity = 0.7f;

    [Range(0, 1)]
    [Tooltip("Defines the amount of liquid in the container. 0 if empty, 1 if full.")]
    public float filled = 0.5f;

    [SerializeField]
    [Tooltip("If true, object will automatically refill when minimum capacity is reached.")]
    private bool refillOnEmpty = false;

    [SerializeField]
    [Tooltip("If true, object will automatically empty when maximum capacity is reached.")]
    private bool emptyOnFilled = false;

    [SerializeField]
    [Tooltip("If true, object will automatically tilt to pour liquid, for demonstration purposes. This will temporarily set object to kinematic.")]
    private bool tiltOnStart = false;

    // TODO decouple container and liquid behaviour or put them in one class
    private LiquidBehaviour liquid;
    private float initialFilled = 0f;

    public LiquidBehaviour Liquid { get => liquid; private set => liquid = value; }

    public event Action ContainerFull;
    public event Action ContainerEmpty;

    private void Awake()
    {
        // translate filled value from percentage to amount to match min and max capacity
        filled = filled * (maxCapacity - minCapacity) + minCapacity;
        initialFilled = filled;
    }

    private void Start()
    {
        liquid = GetComponent<LiquidBehaviour>();

        // check if working in play mode - shouldn't execute tilt in edit mode
        if (Application.isPlaying)
        {
            StartCoroutine(Tilt());
        }

        if (filled < minCapacity)
            filled = minCapacity;

        if (filled > maxCapacity)
            filled = maxCapacity;
    }

    private void OnEnable()
    {
        GameManager.OnTaskStarted += OnTaskStart;
    }

    private void OnDisable()
    {
        GameManager.OnTaskStarted -= OnTaskStart;
    }

    private void OnTaskStart(Task task)
    {
        filled = initialFilled;
    }

    public void TryPourOut()
    {
        if (filled > minCapacity)
        {
            liquid.PourOut();

            filled -= liquid.flowVelocity * Time.deltaTime;
            //Debug.Log("pourout = " + liquid.flowVelocity* Time.deltaTime);
            
            // make sure capacity never goes below min.
            if (filled <= minCapacity)
            {
                filled = refillOnEmpty ? maxCapacity : minCapacity;
                ContainerEmpty?.Invoke();
            }
        }
    }

    public void PourIn()
    {
        if (filled < maxCapacity)
        {
            filled += liquid.flowVelocity * Time.deltaTime;

            // make sure capacity never goes above max.
            if (filled >= maxCapacity)
            {
                filled = emptyOnFilled ? minCapacity : maxCapacity;
                ContainerFull?.Invoke();
            }
        }
    }

    #region DEBUGGING

    // Plays bottle tilt animation for debugging purposes. Will animate tilting the container only if 'tiltAnimationOnStart' set to true in the inspector.
    private IEnumerator Tilt()
    {
        // reset bottle position
        transform.localRotation = Quaternion.Euler(270f, 0f, 0f);

        // set object to kinematic so the animation is not affected by gravity
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        bool kinematicOnAnimationExit = rb.isKinematic;
        rb.isKinematic = true;

        while (tiltOnStart)
        {
            float rotationSpeed = 30f;
            float maxRotation = 110f;

            // tilt right
            yield return StartCoroutine(Rotate(maxRotation, rotationSpeed, 1));

            // keep in position
            yield return new WaitForSeconds(5f);

            // tilt left
            yield return StartCoroutine(Rotate(maxRotation, rotationSpeed, -1));

            // reset bottle to be full
            if (filled <= minCapacity)
                filled = maxCapacity;
        }

        rb.isKinematic = kinematicOnAnimationExit;
        yield return null;
    }

    // direction 1 for right tilt, -1 for left tilt
    private IEnumerator Rotate(float maxRotation, float rotationSpeed, int direction)
    {
        float currentRotation = 0f;

        while (gameObject.activeSelf && currentRotation <= maxRotation)
        {
            float rotation = rotationSpeed * Time.deltaTime;
            transform.Rotate(direction * rotation, 0, 0);
            currentRotation += rotation;
            yield return null;
        }
    }

    #endregion DEBUGGING
}