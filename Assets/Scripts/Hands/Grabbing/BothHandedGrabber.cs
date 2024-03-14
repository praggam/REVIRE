using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Responsible for detecting if both hands are grabbing the same object and when to release it. Keeps track of the middle of the grip which is estimated in the middle of both hands and recalculates the position as hands move.

// TODO optimize methods, maybe find a way to merge with kinematic grabber. How to merge if Kinematic is per hand and BH is per both?
public class BothHandedGrabber : MonoBehaviour
{
    [SerializeField]
    [Tooltip("GameObject childed to Right/Left HandAnchor. In case of both handed grabber, doesn't matter which one picked, the other one will be disabled during grab. Position will be updated in FixedUpdate() to represent the midpoint between thumb and closest grabbing opposite finger to detect when grip release threshold is exceeded.")]
    private GameObject grabMidpoint = null;

    [SerializeField]
    [Tooltip("If distance between grabbing thumb/opposite finger and the midpoint exceeds the distance at grab begin by the threshold value, release will be triggered.")]
    private float releaseThreshold = 0.05f;

    [SerializeField] BaseGrabber leftGrabber = null;
    [SerializeField] BaseGrabber rightGrabber = null;

    // Values updated at GrabBegin
    private float distanceFromMidpoint_L = 0f;
    private float distanceFromMidpoint_R = 0f;

    Grabbable grabbedObject = null;
    private Transform leftTransform;
    private Transform rightTransform;

    private void Start()
    {
        StartCoroutine(SetHandTransforms());
        Debug.Assert(leftGrabber && rightGrabber, "Both Handed Grabber: Grabbers not assigned.");
    }

    private void FixedUpdate()
    {
        if (HandsManager.Instance.bothHandedGrabbing)
        {
            CheckGrabOrRelease();
            UpdateGrabMidpoint();
        }
    }

    void CheckGrabOrRelease()
    {
        if (!(leftGrabber.hand && rightGrabber.hand)) return;

        // CASE 1: CURRENTLY NOT GRABBING - check if should start grabbing.
        if (HandsManager.Instance.BothHandsTracked && BeginGrabAllowed(leftGrabber) && BeginGrabAllowed(rightGrabber))
        {
            Collider grabbableCollider = GetGrabbableColliderIfAny();
            if (grabbableCollider != null)
            {
                grabbedObject = grabbableCollider.gameObject.GetComponent<Grabbable>();
                GrabBegin(grabbedObject);
            }
        }

        // CASE 2: CURRENTLY GRABBING - check if should release
        else if (ShouldRelease())
        {
            if (grabbedObject && grabbedObject.GrabbableRB)
            {
                grabbedObject.transform.parent = null;
                grabbedObject.GrabbableRB.isKinematic = false;
                grabbedObject.IsHeld = false;
            }

            grabbedObject = null;


            // TODO rewrite this to be a method of grabber
            GrabEnd(leftGrabber);
            GrabEnd(rightGrabber);

            distanceFromMidpoint_L = 0f;
            distanceFromMidpoint_R = 0f;

        }
    }

    private bool BeginGrabAllowed(BaseGrabber grabber)
    {
        bool wasGrabbing = grabber.handController.State.Equals(HandState.GRABBING);
        bool handClenched = grabber.handController.Pose.Equals(HandPose.ThumbsUp) || grabber.handController.Pose.Equals(HandPose.Fist);

        return !handClenched && !wasGrabbing;
    }



    private bool ShouldRelease()
    {
        if (!HandsManager.Instance.BothHandsTracked)
            return true;

        if (!leftGrabber.grabbedByBothHands || !rightGrabber.grabbedByBothHands)
            return true;

        //return if not currently grabbing
        //if (!leftGrabber.handController.State.Equals(HandState.GRABBING) || !rightGrabber.handController.State.Equals(HandState.GRABBING))
        if(!grabbedObject)
            return false;

        if(!HandsManager.Instance.BothHandsTracked)
            return true;

        // calculate distance between thumb and opposite fingers to chec if exceeded the release threshold
        float leftDistance = DistanceFromHand(leftGrabber) - distanceFromMidpoint_L;
        float rightDistance = DistanceFromHand(rightGrabber) - distanceFromMidpoint_R;

        // recalculate the position difference and release if total distance larger than threshold
        return (leftDistance > releaseThreshold || rightDistance > releaseThreshold);
    }


    private void GrabBegin(Grabbable go)
    {
        leftGrabber.GrabObject(go, true);
        rightGrabber.GrabObject(go, true);

        //grabbedRB = go.GetComponent<Rigidbody>();

        // get distance between both hands at the beginning of grab to release when threshold exceeded
        distanceFromMidpoint_L = DistanceFromHand(leftGrabber);
        distanceFromMidpoint_R = DistanceFromHand(rightGrabber);

        UpdateGrabMidpoint();

        // Child object to hand and set it to kinematic to disable physics properties. This will be in the middle of both hnds
        go.GrabbableRB.isKinematic = true;
        go.transform.parent = grabMidpoint.transform;
    }

    private void GrabEnd(BaseGrabber grabber)
    {
        grabber.handController.State = HandState.EMPTY;
        grabber.grabbedByBothHands = false;

        if (grabber.grabbedObject != null)
        {
            grabber.GrabExit(grabber.grabbedObject);
            grabber.grabbedObject = null;
        }

        ResetGrabMidpoint();
    }

    private float DistanceFromHand(BaseGrabber grabber)
    {
        if (!(grabbedObject && grabbedObject.GrabbableRB && grabber))
        {
            return Mathf.Infinity;
        }

        Vector3 pos = grabber.skeleton.Bones[(short)OVRSkeleton.BoneId.Hand_Start].Transform.position;
        Vector3 objectCentre = grabbedObject.GrabbableRB.worldCenterOfMass;
        float distance = Vector3.Distance(pos, objectCentre);

        return distance;
    }


    private Collider GetGrabbableColliderIfAny()
    {
        // CHECK FOR GRABBING COLLISIONS WITH THUMB (pincer/palmar grasp)
        List<Collider> grabbedColliders_L = leftGrabber.GetCommonCollisions(leftGrabber.ThumbCollisionBones, leftGrabber.OppositeCollisionBones);

        if (grabbedColliders_L.Count == 0)
            return null;

        List<Collider> grabbedColliders_R = rightGrabber.GetCommonCollisions(rightGrabber.ThumbCollisionBones, rightGrabber.OppositeCollisionBones);

        if (grabbedColliders_R.Count == 0)
            return null;

        Collider grabbable = grabbedColliders_L.Intersect(grabbedColliders_R).FirstOrDefault();

        return grabbable;
    }

    private void UpdateGrabMidpoint()
    {
        // if either hand is not clearly visible, dont update
        if (!HandsManager.Instance.BothHandsTracked)
            return;

        // TODO grabbed by both hands should be handled differently
        if (!(grabbedObject && grabbedObject.GrabbableRB && leftGrabber.grabbedByBothHands)) 
            return;

        Vector3 leftHandPos = HandsManager.Instance.HandLeft.transform.position;
        Vector3 rightHandPos = HandsManager.Instance.HandRight.transform.position;

        //Vector3 leftHandPos = leftTransform.position;
        //Vector3 rightHandPos = rightTransform.position;

        Vector3 newPos = (leftHandPos + rightHandPos) / 2;
        grabMidpoint.transform.position = newPos;

        #region WORKING ON ROTATION; INCOMPLETE
        // TODO try to change rotation
        //Quaternion leftHandRot = HandsManager.Instance.HandLeft.transform.rotation;
        //Quaternion rightHandRot = HandsManager.Instance.HandLeft.transform.rotation;

        //Vector3 currentOffset = grabMidpoint.transform.InverseTransformPoint(leftHandPos);
        //Vector3 desiredOffset = grabMidpoint.transform.InverseTransformPoint(rightHandPos);

        //grabMidpoint.transform.localRotation *= Quaternion.FromToRotation(currentOffset, desiredOffset);
        #endregion

    }

    private void ResetGrabMidpoint()
    {
        grabMidpoint.transform.position = new Vector3(0, 0, 0);
    }

    private IEnumerator SetHandTransforms()
    {
        yield return new WaitWhile(() => !(leftGrabber.skeleton && rightGrabber.skeleton ) || leftGrabber.skeleton.Capsules.Count == 0 || rightGrabber.skeleton.Capsules.Count == 0);
 
        //leftTransform = leftGrabber.skeleton.Bones[(short)OVRSkeleton.BoneId.Hand_Middle1].Transform;
        //rightTransform = rightGrabber.skeleton.Bones[(short)OVRSkeleton.BoneId.Hand_Middle1].Transform;

    }
}
