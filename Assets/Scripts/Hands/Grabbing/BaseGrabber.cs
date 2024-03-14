using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRSkeleton;

// Base grabber responsible for initializing hand colliders, collecting collision events etc. Should be extended by a grabber class which handles when to grab and release etc. Currently 2 subclasses are implemented: KinematicGrabber and PhysicsGrabber.

// Base grabber keeps a list of all active collisions with fingers which can be used by extending classes such as for kinematic grabbing. Provides methods common for all grabber classes such as finding if any object is colliding with specific hand bones, useful to check e.g. if a thumb is colliding with the same object as index finger for pinch grab or to check if colliders of both hands are grabbing the same object for both handed grabbing. 
[RequireComponent(typeof(HandController))]
[RequireComponent(typeof(OVRHand))]
[RequireComponent(typeof(OVRSkeleton))]
public class BaseGrabber : MonoBehaviour
{   
    [SerializeField]
    [Tooltip("If Object Parenting is used (KinematicGrabber), grabbed object will be childed to this Transform. If Object Parenting is disabled, object will follow the movement of this Transform which will in turn be recalculated every time a collider is added to collision list for grabbed object.")]
    protected GameObject handReference = null;
    
    public HandController handController = null;

    [HideInInspector] public OVRCustomSkeleton skeleton = null;
    [HideInInspector] public Grabbable grabbedObject = null;
    [HideInInspector] public bool grabbedByBothHands = false;

    [HideInInspector] public OVRHand hand = null;
    protected SkinnedMeshRenderer handRenderer = null;
    protected SkeletonType skeletonType = SkeletonType.None;

    #region BONE COLLISION COLLECTIONS
    public List<short> ThumbCollisionBones { get => thumbCollisionBones; private set => thumbCollisionBones = value; }
    private List<short> thumbCollisionBones = new List<short>
    {
        (short) BoneId.Hand_Thumb2,          // thumb proximal phalange bone
		(short) BoneId.Hand_Thumb3           // thumb distal phalange bone
    };

    public List<short> OppositeCollisionBones { get => oppositeCollisionBones; protected set => oppositeCollisionBones = value; }
    private List<short> oppositeCollisionBones = new List<short>
    {
        (short) BoneId.Hand_Index2,           // index intermediate phalange bone
		(short) BoneId.Hand_Index3,           // index distal phalange bone
		(short) BoneId.Hand_Middle2,          // middle intermediate phalange bone
		(short) BoneId.Hand_Middle3,          // middle distal phalange bone
		(short) BoneId.Hand_Ring2,            // ring intermediate phalange bone
		(short) BoneId.Hand_Ring3,            // ring distal phalange bone
		(short) BoneId.Hand_Pinky2,           // pinky intermediate phalange bone
		(short) BoneId.Hand_Pinky3              // pinky distal phalange bone
    };

    protected Dictionary<short, Collider> currentCollisions = new Dictionary<short, Collider>();

    #endregion

    #region HAND EVENTS
    public static event Action<Grabbable, BaseGrabber> OnGrabEnter;
    public static event Action<Grabbable, BaseGrabber> OnGrabExit;

    public virtual void GrabEnter(Grabbable go) => OnGrabEnter(go, this);
    public virtual void GrabExit(Grabbable go) => OnGrabExit(go, this);
    #endregion

    protected void Start()
    {
        hand = GetComponent<OVRHand>();
        skeleton = GetComponent<OVRCustomSkeleton>();
        skeletonType = skeleton.GetSkeletonType();
        handRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        handController = GetComponent<HandController>();

        RegisterCollisionEvents();
        StartCoroutine(AttachColliders());
    }

    public void Update()
    {
        // don't render hands if tracking confidence low
        handRenderer.enabled = hand.IsDataHighConfidence;
    }

    public void GrabObject(Grabbable go, bool bothHandedGrab)
    {
        grabbedByBothHands = bothHandedGrab;

        handController.State = HandState.GRABBING;
        grabbedObject = go;
        go.IsHeld = true;
        GrabEnter(go);
    }

    private void AddBoneCollision(short boneId, Collider collider)
    {
        if(!currentCollisions.ContainsKey(boneId))
            currentCollisions.Add(boneId, collider);
    }

    private void RemoveBoneCollision(short boneId, Collider collider)
    {
        if (currentCollisions.ContainsKey(boneId))
            currentCollisions.Remove(boneId);
    }

    // returns the first collider currently grabbed by any bone from both collision list, e.g. if checking for palmar grasp we would be checking for same collider between thumb tip and index finger tip.
    public Collider GetFirstCommonCollision(List<short> collisionBones1, List<short> collisionBones2)
    {
        List<Collider> colliders1 = GetCurrentCollisions(collisionBones1);
        List<Collider> colliders2 = GetCurrentCollisions(collisionBones2);

        foreach (Collider collider in colliders1)
        {
            if (colliders2.Contains(collider))
                return collider;
        }

        return null;
    }

    public List<Collider> GetCommonCollisions(List<short> collisionBones1, List<short> collisionBones2)
    {
        List<Collider> colliders1 = GetCurrentCollisions(collisionBones1);
        List<Collider> colliders2 = GetCurrentCollisions(collisionBones2);

        List<Collider> commonCollisions = new List<Collider>();

        foreach (Collider collider in colliders1)
        {
            if (colliders2.Contains(collider))
                commonCollisions.Add(collider);
        }

        return commonCollisions;
    }

    // returns all collisions with provided list of bones
    protected List<Collider> GetCurrentCollisions(List<short> collisionBones)
    {
        List<Collider> colliders = new List<Collider>();

        foreach (short bone in collisionBones)
        {
            currentCollisions.TryGetValue(bone, out Collider collider);
            if (collider != null && !colliders.Contains(collider))
                colliders.Add(collider);
        }

        return colliders;
    }

    public void ForceRelease(Grabbable grabbable)
    {
        // TODO force release on 'Reset positions'
        Debug.LogWarning(string.Format("Force release: {0}", grabbable));
    }


    IEnumerator AttachColliders()
    {
        // wait until the capsule colliders are created in OVRSkeleton. Bone colliders are set to active when hand tracking confidence is high.
        yield return new WaitWhile(() => skeleton == null || skeleton.Capsules.Count == 0);

        foreach (OVRBoneCapsule capsule in skeleton.Capsules)
        {
            if (ThumbCollisionBones.Contains(capsule.BoneIndex) || 
                OppositeCollisionBones.Contains(capsule.BoneIndex))
            {
                SetupGrabCollider(capsule);
            }

            SetupGrabLayer(capsule);
        }
    }

    void SetupGrabCollider(OVRBoneCapsule capsule)
    {
        GameObject capsuleColliderGO = capsule.CapsuleCollider.gameObject;
        GameObject capsuleRBGO = capsule.CapsuleRigidbody.gameObject;

        // TODO check which one is needed if needed at all
        capsuleRBGO.tag = "Hand";
        capsuleColliderGO.tag = "Hand";

        BoneCollisionController collisionGO = capsuleRBGO.AddComponent<BoneCollisionController>();
        collisionGO.boneid = capsule.BoneIndex;
        collisionGO.hand = skeletonType;
    }

    void SetupGrabLayer(OVRBoneCapsule capsule)
    {
        GameObject capsuleColliderGO = capsule.CapsuleCollider.gameObject;
        GameObject capsuleRBGO = capsule.CapsuleRigidbody.gameObject;

        capsuleColliderGO.layer = LayerMask.NameToLayer("Grabber");
        capsuleRBGO.layer = LayerMask.NameToLayer("Grabber");
    }

    void RegisterCollisionEvents()
    {
        if (skeletonType == SkeletonType.HandRight)
        {
            BoneCollisionController.OnCollisionEnter_R += OnCollisionAttach;
            BoneCollisionController.OnCollisionExit_R += OnCollisionDetach;
        }
        else if (skeletonType == SkeletonType.HandLeft)
        {
            BoneCollisionController.OnCollisionEnter_L += OnCollisionAttach;
            BoneCollisionController.OnCollisionExit_L += OnCollisionDetach;
        }
    }
    void DeregistrCollisionEvents()
    {
        if (skeletonType == SkeletonType.HandRight)
        {
            BoneCollisionController.OnCollisionEnter_R -= OnCollisionAttach;
            BoneCollisionController.OnCollisionExit_R -= OnCollisionDetach;
        }
        else if (skeletonType == SkeletonType.HandLeft)
        {
            BoneCollisionController.OnCollisionEnter_L -= OnCollisionAttach;
            BoneCollisionController.OnCollisionExit_L -= OnCollisionDetach;
        }
    }

    private void OnCollisionAttach(short boneId, Collider collision)
        => AddBoneCollision(boneId, collision);

    private void OnCollisionDetach(short boneId, Collider collision)
        => RemoveBoneCollision(boneId, collision);

    private void OnDestroy()
    {
        DeregistrCollisionEvents();
    }
}

