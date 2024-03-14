using System.Collections.Generic;
using UnityEngine;

// Grabbing relies on finger colliders from OVRCustomSkeleton to detect finger collisions with the object via BoneCollisionController attached to relevant bone colliders specified in BaseGrabber, proximal and distal finger bones. All collisions registered in BaseGrabber are analyzed to check if all grabbing conditions are satisfied. Once an object is grabbed, it is set to kinematic and is parented to the hand to follow hand position and rotation in world space, similarly to ParentHeldObject in OVRGRabber (for controllers only).

// Since setting the object to kinematic disables its physics properties collisions with hands are no longer detected and another method for release had to be implemented. At grab-begin, the distance between the grabbing thumb and closest opposite finger is saved as the grab offset. While the hand is grabbing, the distance is calculated in every frame and the release action is triggered if the distance becomes greater than the initial distance plus specified threshold. 

public class KinematicGrabber : BaseGrabber
{
    [SerializeField]
    [Tooltip("GameObject childed to Right/Left HandAnchor. Position will be updated in FixedUpdate() to represent the midpoint between thumb and closest grabbing opposite finger to detect when grip release threshold is exceeded.")]
    private GameObject grabMidpoint = null;

    [SerializeField]
    [Tooltip("If distance between grabbing thumb/opposite finger and the midpoint exceeds the distance at grab begin by the threshold value, release will be triggered.")]
    private float releaseThreshold = 0.005f;

    [Tooltip("Values updated at GrabBegin to keep track of the distance from thumb collider to the center of grabbed Rigidbody")]
    private float distanceFromMidpointToThumb = 0f;

    [Tooltip("Values updated at GrabBegin to keep track of the distance from closest opposite finger (opposite = not thumb) to the center of grabbed Rigidbody")]
    private float distanceFromMidpointToOpposite = 0f;

    [Tooltip("Object to which grabbable candidate was parented before grab begin. Saved to restore on grab end.")]
    private Transform grabbedParent = null;

    [Tooltip("Saves closest collision point to hand on grab begin.")]
    private Vector3 grabbedCollision = Vector3.zero;

    private void FixedUpdate()
    {
        if (HandsManager.Instance.kinematicGrabbing)
        {
            CheckGrabOrRelease();
            UpdateGrabMidpoint();
        }
    }

    // Keep track of finger colliders and if thumb and any of the opposite finger colliders are touching the same object triger "Grab" check what object the thumb is grabbing to check if any opposite finger is grabbing the same object.
    void CheckGrabOrRelease()
    {
        if (!base.hand) return;

        // CASE 1: CURRENTLY NOT GRABBING - check if should start grabbing. If there are no current collisions registered, it either means that there are no grabbable objects available
        // CASE 2: CURRENTLY GRABBING - check if should release
        if (BeginGrabAllowed() && currentCollisions.Count > 1)
        {
            Collider grabbableCollider = ShouldGrab();
            if (grabbableCollider != null)
            {
                grabbedCollision = grabbableCollider.ClosestPoint(grabMidpoint.transform.position);
                GrabBegin(grabbableCollider.gameObject.GetComponent<Grabbable>());
            }
        }
        else if (ShouldRelease())
        {
                GrabEnd();
        }
    }

    private bool BeginGrabAllowed()
    {
        // don't allow grabbing with closed hand unless hand was already grabbing or grabbing while system gesture is in progress
        bool handOpen = handController.Pose.Equals(HandPose.Open);
        bool wasGrabbing = handController.State.Equals(HandState.GRABBING);
        bool handClenched = handController.Pose.Equals(HandPose.ThumbsUp) || handController.Pose.Equals(HandPose.Fist);

        return !(wasGrabbing || handClenched || handOpen || hand.IsSystemGestureInProgress);
    }

    // If thumb and any of the opposite fingers is holding the same object return object's collider, otherwise null
    private Collider ShouldGrab()
    {
        Collider grabbedCollider = null;

        // CHECK FOR GRABBING COLLISIONS WITH THUMB (pincer/lateral grasp)
        grabbedCollider = GetFirstCommonCollision(base.ThumbCollisionBones, base.OppositeCollisionBones);

        #region PALMAR GRASP, INCOMPLETE
        // CHECK FOR GRABBING WITHOUT THUMB (palmer grasp) - this doesn't perform well without gesture detection which would exclude grabbing of object with completely open hand as it attaches object to hand whenever touched. 
        //if (grabbedCollider == null)
        //{
        //grabbedCollider = GetCommonCollision(base.proximalCollisionBones, base.distalCollisionBones);
        //if(grabbedCollider)
        //{
        //    Debug.LogWarning("Performing palmar grasp");
        //}
        //}
        #endregion

        return grabbedCollider;
    }

    private bool ShouldRelease()
    {
        // return if not currently grabbing
        if (!handController.State.Equals(HandState.GRABBING))
            return false;

        // force grab release if open hand gesture detected
        if (handController.Pose.Equals(HandPose.Open) && !grabbedByBothHands)
            return true;

        // calculate distance between thumb and opposite fingers to chec if exceeded the release threshold
        float thumbDistance =
            ComputeDistanceBetweenFingerAndObject((short)OVRSkeleton.BoneId.Hand_Thumb2, grabbedObject.GrabbableRB) - distanceFromMidpointToThumb;
        float oppositeDistance = GetClosestHoldingOpposite() - distanceFromMidpointToOpposite;

        // recalculate the position difference and release if total distance larger than threshold
        return (thumbDistance > releaseThreshold || oppositeDistance > releaseThreshold);
    }

    private void GrabBegin(Grabbable go)
    {
        base.GrabObject(go, false);
       
        // save positions of fingers around the object at moment of grabbing. Get distance between thumb and the nearest opposite finger to see if it is larger than distance at the beginning of the grab, regardless which finger is currently the closest in case fingers were moved during grab
        distanceFromMidpointToThumb = ComputeDistanceBetweenFingerAndObject((short)OVRSkeleton.BoneId.Hand_Thumb2, grabbedObject.GrabbableRB);
        distanceFromMidpointToOpposite = GetClosestHoldingOpposite();

        // Child object to hand and set it to kinematic to disable physics properties
        grabbedParent = grabbedObject.transform.parent;
        grabbedObject.transform.parent = handReference.transform;
        grabbedObject.GrabbableRB.isKinematic = true;
    }
    private void GrabEnd()
    {
        // TODO write GrabEnd in BaseGrabber for common commands with bothhandedgrabber
        //base.GrabEnd();

        handController.State = HandState.EMPTY;

        distanceFromMidpointToThumb = 0f;
        distanceFromMidpointToOpposite = 0f;

        if (grabbedObject && grabbedObject.GrabbableRB)
        {
            base.GrabExit(grabbedObject);

            grabbedObject.transform.parent = grabbedParent;
            grabbedObject.GrabbableRB.isKinematic = false;
        }

        grabbedObject.IsHeld = false;
        grabbedObject = null;
    }

    private void UpdateGrabMidpoint()
    {
        if (!grabbedObject || grabbedByBothHands) return;

        // TODO if needed, change reference point to the nearest collision point as now it's using grabbedRB center of mass which can be far away if grab started e.g. at the corner of the object
        //Vector3 referencePoint = grabbedObject.GrabbableRB.centerOfMass;
        Vector3 referencePoint = grabbedCollision.normalized;

        Vector3 thumbPos = GetFingerPosition((short)OVRSkeleton.BoneId.Hand_Thumb3);
        Vector3 closestOpposite = GetClosestOppositeFingerPos(referencePoint).Key;

        // place midpoint exactly between the thumb and opposite grabbing finger
        Vector3 newPos = (thumbPos + closestOpposite) / 2;
        grabMidpoint.transform.position = newPos;
    }

    private Vector3 GetFingerPosition(short boneId)
    {
        if (boneId > skeleton.Bones.Count)
            throw new System.Exception("BoneId does not exist");

        return skeleton.Bones[boneId].Transform.position;
    }


    private KeyValuePair<Vector3, float> GetClosestOppositeFingerPos(Vector3 referencePoint)
    {
        float closestDistance = Mathf.Infinity;
        short finger = 0;

        List<short> fingers = new List<short>()
        {
            (short) OVRSkeleton.BoneId.Hand_Index3,
            (short) OVRSkeleton.BoneId.Hand_Middle3,
            (short) OVRSkeleton.BoneId.Hand_Pinky3,
            (short) OVRSkeleton.BoneId.Hand_Ring3
        };

        foreach (short s in fingers)
        {
            float newDist = ComputeDistanceBetweenFingerAndObject(s, grabbedObject.GrabbableRB);
            if (newDist < closestDistance)
            {
                closestDistance = newDist;
                finger = s;
            }       
        }

        return new KeyValuePair<Vector3, float>(GetFingerPosition(finger), closestDistance);
    }

    private float GetClosestHoldingOpposite()
    {
        float closestDistance = Mathf.Infinity;

        List<short> fingers = new List<short>()
        {
            (short) OVRSkeleton.BoneId.Hand_Index3,
            (short) OVRSkeleton.BoneId.Hand_Middle3,
            (short) OVRSkeleton.BoneId.Hand_Pinky3,
            (short) OVRSkeleton.BoneId.Hand_Ring3
        };

        foreach (short s in fingers)
        {
            float newDist = ComputeDistanceBetweenFingerAndObject(s, grabbedObject.GrabbableRB);
            if (newDist < closestDistance)
                closestDistance = newDist;
        }

        return closestDistance;
    }

    private float ComputeDistanceBetweenFingerAndObject(short fingerId, Rigidbody rb)
    {
        if (!rb || fingerId >= skeleton.GetCurrentNumBones())
        {
            Debug.LogWarning("Error  trying to compute distance. Rigidbody was null or incorrect finger bone id.");
            return Mathf.Infinity;
        }

        Vector3 handPos = skeleton.Bones[fingerId].Transform.position;
        Vector3 objectCentre = rb.worldCenterOfMass;
        float distance = Vector3.Distance(handPos, objectCentre);

        return distance;
    }
}
