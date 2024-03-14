using Assets.Scripts.Gameplay;
using System;
using UnityEngine;

// Responsible for detecting and triggering events when a grabbable object is placed on the level. Grabbable must be positioned exactly within the boundary of the level tile. Enables showing/hiding and changing color of podest level. 
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Renderer))]
public class PodestLevel : MonoBehaviour
{
    [Tooltip("Material the level should display as next in queue.")]
    [SerializeField] Material materialOnNext = null;

    [Tooltip("Material the level should display when grabbable overlapping with level but not positioned correctly inside.")]
    [SerializeField] Material materialOnIntersect = null;

    [Tooltip("Material the level should display when grabbable positioned inside correctly.")]
    [SerializeField] Material materialOnCurrent = null;

    [Tooltip("Podest level in hierarchy, where level 0 is the lowest/initial level.")]
    public int level = 0;

    //Events triggered when a grabbable object is detected within boundary of the level tile or respectively when it leaves the boundaries.")]
    public event Action<int> OnGrabbableEnterArea;
    public event Action<int> OnGrabbableExitArea;

    [Tooltip("Level tile renderer.")]
    private Renderer rend = null;

    [Tooltip("Boundaries of the collider are used to detect if grabbale object entered area.")]
    private Collider levelCollider = null;

    [Tooltip("Assigned when a grabbable object enters trigger area to check when object leaves area.")]
    private Collider otherCollider = null;

    public bool ChangeLevelActive = true;

    private void Awake()
    {
        levelCollider = GetComponent<Collider>();
        rend = GetComponent<Renderer>();
        GameManager.Instance.OnTaskEnded += ResetLevel;
    }

    private void ResetLevel(Task task)
    {
        // not needed for now
    }

    private void OnTriggerStay(Collider collider)
    {
        // renderer will be enabled if current level is not "next level"
        if (!rend.enabled)
            return;

        if (!ChangeLevelActive)
            return;

        // check if object entering the area is a grabbable
        Grabbable grabbable = collider.gameObject.GetComponent<Grabbable>();

        if (grabbable != null)
        {
            //otherCollider = collider; 

            if (CheckIfGrabbableInBounds(collider))
            {
                if (materialOnCurrent.color != rend.material.color)
                    LSLSender.SendLsl("Put Down Successfully", new float[] { 330 });
                
                OnGrabbableEnterArea?.Invoke(level);
                ChangeMaterial(materialOnCurrent);
                
            }
            else
            {
                //ChangeMaterial(materialOnIntersect);
            }  
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if(collider == otherCollider)
        {
            //otherCollider = null;
            ChangeMaterial(materialOnNext);
        }
    }

    private bool CheckIfGrabbableInBounds(Collider collider)
    {
        // checks if all object boundaries on x and z axes are within the boundary of level
        return
            collider.bounds.min.x > levelCollider.bounds.min.x &&
            collider.bounds.max.x < levelCollider.bounds.max.x &&
            collider.bounds.min.z > levelCollider.bounds.min.z &&
            collider.bounds.max.z < levelCollider.bounds.max.z;
            
    }

    private void ChangeMaterial(Material material)
    {
        if(rend.enabled)
            rend.material = material;
    }

    public void HideDisplay()
    {
        gameObject.SetActive(false);
    }

    public void Highlight()
    {
        if (!rend.enabled)
            rend.enabled = true;

        ChangeMaterial(materialOnNext);
    }
}
