using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class StreamBehaviour : MonoBehaviour
{
    public LiquidBehaviour liquid;

    [Tooltip("basic line renderer with 2 access points (index 0 = start, index 1 = end). Point 0 at pouring origin and point 1 at pouring destination.")]
    private LineRenderer lineRenderer = null;

    [Tooltip("Splash particles displayed at the end of line renderer.")]
    private ParticleSystem splashParticles = null;

    private Coroutine pourRoutine = null;

    [Tooltip("Position at which stream should hit the ground.")]
    private Vector3 targetPosition = Vector3.zero;

    [Tooltip("Used to determine the stream end position at current liquid level. Null if currently not colliding with container.")]
    private Container fillableContainer = null;

    [Tooltip("Layers to ignore by new line renderers")]
    private LayerMask ignoreLayers;

    private readonly float streamVelocity = 1.0f;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        splashParticles = GetComponentInChildren<ParticleSystem>();

        ignoreLayers = ~((1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Grabbable")));
    }

    private void Start()
    {
        // initiate stream begin at origin (position of component stream is attached to)
        MoveToPosition(0, transform.position);

        // initiate the end of the stream to the same point as the beginning at start
        MoveToPosition(1, transform.position);

        StartCoroutine(UpdateParticles());
        pourRoutine = StartCoroutine(PourRoutine());
    }

    private void Update()
    {
        // If raycast hits anything, check if line renderer collides with a container and try to pour in if so.
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 2.0f))
        {
            Container container = hit.collider.gameObject.GetComponent<Container>();

            if (container != null)
            {
                container.PourIn();
                fillableContainer = container;
            }
            else
            {
                fillableContainer = null;
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void End()
    {
        if(pourRoutine != null) 
            StopCoroutine(pourRoutine);

        pourRoutine = StartCoroutine(EndPourRoutine());
    }

    private Vector3 FindEndPoint()
    {
        // create a ray from current position directly downwards
        Ray ray = new Ray(transform.position, Vector3.down);

        // visualize the ray with default length if not stopped by any object. 'Hit' will detect the nearest colliding object
        Physics.Raycast(ray, out RaycastHit hit, 2.0f, ignoreLayers);

        // if raycasthit hits a container set stream end at liquid level, otherwise if any collider set the end of the stream to the collision point else use default stream length

        if (fillableContainer && hit.collider)
        {
            return new Vector3(hit.point.x, fillableContainer.Liquid.liquidHeight, hit.point.z);
        }
        else if (fillableContainer)
        {
            // sets stream direction in the middle of the container if no collider found
            return new Vector3(fillableContainer.transform.position.x, fillableContainer.Liquid.liquidHeight, fillableContainer.transform.position.z);
        }
        else
        {
            return hit.collider ? hit.point : ray.GetPoint(2.0f);
        }
    }

    // move directly to target position
    private void MoveToPosition(int index, Vector3 targetPosition)
    {
        lineRenderer.SetPosition(index, targetPosition);
    }

    // animate movement towards target position over time
    private void AnimateToPosition(int index, Vector3 targetPosition)
    {
        Vector3 currentPosition = lineRenderer.GetPosition(index);
        Vector3 newPosition = Vector3.MoveTowards(currentPosition, targetPosition, Time.deltaTime * streamVelocity);

        lineRenderer.SetPosition(index, newPosition);
    }

    // manages start and end points of the stream while pouring
    private IEnumerator PourRoutine()
    {
        // continue routine while the object is active
        while (gameObject.activeSelf)
        {
            // find where stream should hit the ground
            targetPosition = FindEndPoint();

            // let stream beginning stay at the origin
            MoveToPosition(0, transform.position);

            // gradually move the stream ending towards target position on the ground
            AnimateToPosition(1, targetPosition);

            yield return null;
        }
    }

    // animate the stream start and end points until it hits the target before destroying it
    private IEnumerator EndPourRoutine()
    {
        // continue animating the stream until the origin meets destination
        while (lineRenderer.GetPosition(0) != targetPosition)
        {
            AnimateToPosition(0, targetPosition);
            AnimateToPosition(1, targetPosition);
            yield return null;
        }

        // destroy the strem instance once whole stream reached target
        Destroy(gameObject);
    }

    // manage position of splash particles
    private IEnumerator UpdateParticles()
    {
        while (gameObject.activeSelf)
        {
            // position the splash at target stream end
            splashParticles.gameObject.transform.position = targetPosition;

            // show splash particles only once the first stream point reaches the position
            bool hasReachedTarget = lineRenderer.GetPosition(1) == targetPosition;

            splashParticles.gameObject.SetActive(hasReachedTarget);

            yield return null;
        }
    }
}