using Assets.Scripts.Gameplay;
using UnityEngine;

// All functionality related to grabbable objects. Grabber will only detect grabbing event with Grabbable objects active in the scene.  
[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    private Vector3 initialPosition = Vector3.zero;
    private Quaternion initialRotation = Quaternion.identity;
    private Rigidbody grabbableRB = null;
    private bool isHeld = false;

    public bool AutoRegrabbing = false;

    #region PROPERTIES
    public Vector3 InitialPosition { get => initialPosition; set => initialPosition = value; }
    public bool IsHeld { get => isHeld; set => isHeld = value; }
    public Rigidbody GrabbableRB { get => grabbableRB; }
    #endregion

    private void Awake()
    {
        grabbableRB = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;

    }

    private void Start()
    {
        GameManager.Instance.OnTaskEnded += DisableGrabbable;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.OnTaskEnded -= DisableGrabbable;
        }
    }

    private void DisableGrabbable(Task task)
    {
        ResetPosition();
        gameObject.SetActive(false);
    }

    public void ResetPosition()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }

    public void ResetPositionY()
    {
        Vector3 pos = new Vector3(transform.position.x, initialPosition.y, transform.position.z);
        transform.position = pos;
        //transform.rotation = initialRotation;
    }
    public void ResetPositionY(float y)
    {
        AutoRegrabbing = true;
        Vector3 pos = new Vector3(transform.position.x, y, transform.position.z);
        transform.position = pos;
        //transform.rotation = initialRotation;
        //Debug.LogWarning("Reset pos%%%%%%%%%%%%%%%%%%%%%%%");
        AutoRegrabbing = false;
    }

    public void SetOffsetY(float offset)
    {
        Vector3 pos = new Vector3(transform.position.x + offset, transform.position.y, transform.position.z);
        transform.position = pos;
    }
}