using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// experimental implementation. Use Unity physics engine to simulate forces while grabbing. As opposed to KinematicGrabber, object retains its physics properties also when grabbed. Downside: grabbing is not stable as objects slip out of hand if too much force is applied. To slightly decrease object movement stuttering, set Interpolation in rigidbody.
public class PhysicsGrabber : BaseGrabber
{
    protected Vector3 grabbedObjectPosOffset = Vector3.zero;
    protected Vector3 lastHandPosition = Vector3.zero;
    protected Quaternion grabbedObjectRotOffset = Quaternion.identity;
    protected Quaternion lastHandRotation = Quaternion.identity;


    private void FixedUpdate()
    {
        CheckGrabOrRelease();
        MovegrabbedObjectIfAny();
    }


    void CheckGrabOrRelease()
    {
        bool handClenched = hand.GetFingerIsPinching(OVRHand.HandFinger.Thumb);
        bool wasGrabbing = (handController.State == HandState.GRABBING);

        // don't allow grabbing with closed hand unless hand was already grabbing
        bool canGrab = !handClenched || wasGrabbing;

        bool isGrabbing;
        Collider grabbableCollider = null;

        if (currentCollisions.Count > 1 && canGrab)
        {
            grabbableCollider = GetThumbCollisions();
            isGrabbing = IsOppositeGrabbing(grabbableCollider);
        }
        else
        {
            isGrabbing = false;
        }

        // check if hand pose has changed and update accordingly
        if (isGrabbing && !wasGrabbing)
        {
            GrabBegin(grabbableCollider.gameObject.GetComponent<Grabbable>());
        }
        // temporarily disable for object parenting as it disables all colliders
        else if (!isGrabbing && wasGrabbing)
        {
            GrabEnd();
        }
    }


    private void GrabBegin(Grabbable go)
    {
        handController.State = HandState.GRABBING;
        grabbedObject = go;

        base.GrabEnter(grabbedObject);


        // initialize new invisible object parented to hnd at the position where object was grabbed
        // as reference where to hold the grabbed object
        handReference.transform.position = grabbedObject.transform.position;
        handReference.transform.rotation = grabbedObject.transform.rotation;
    }

    private void GrabEnd()
    {
        handController.State = HandState.EMPTY;

        if (grabbedObject)
        {
            base.GrabExit(grabbedObject);

            grabbedObject = null;
        }

        grabbedObject = null;
    }

    private void MovegrabbedObjectIfAny()
    {
        if (!grabbedObject)
            return;

        MovegrabbedObjectWithForce();

        lastHandPosition = handReference.transform.position;
        lastHandRotation = handReference.transform.rotation;
    }

    private void MovegrabbedObjectWithForce()
    {
        if (grabbedObject == null)
            return;

        Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();

        // adjust velocity to move to hand. Forces new position
        rb.velocity = (handReference.transform.position - rb.transform.position) / Time.deltaTime;
        //grabbedObject.transform.position += (handTransform.transform.position - lastHandPosition) * Time.deltaTime;


        // use standard quaternion rotation to calculate the rotation from-to
        // ([FROM] hand rotation) * ([TO] current object rotation, inversed)
        Quaternion deltaRot = handReference.transform.rotation * Quaternion.Inverse(grabbedObject.transform.rotation);
        //Quaternion deltaRot = handTransform.transform.rotation * Quaternion.Inverse(rb.rotation);

        // translate movement from quaternion to euler angles which rigidbodies use
        Vector3 eulerRot = new Vector3(
            Mathf.DeltaAngle(0, deltaRot.eulerAngles.x),
            Mathf.DeltaAngle(0, deltaRot.eulerAngles.y),
            Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));

        // smooth out the movement.
        // eulerRot *= .95f;

        // convert to radians
        eulerRot *= Mathf.Deg2Rad;

        rb.angularVelocity = eulerRot / Time.deltaTime;
    }

    private void MovegrabbedObjectWithForceOVR()
    {
        // Set up offsets for grabbed object desired position relative to hand.
        //Vector3 relPos = m_grabbedObj.transform.position - transform.position;
        //relPos = Quaternion.Inverse(transform.rotation) * relPos;
        //m_grabbedObjectPosOff = relPos;

        //Quaternion relOri = Quaternion.Inverse(transform.rotation) * m_grabbedObj.transform.rotation;
        //m_grabbedObjectRotOff = relOri;

        //MovegrabbedObject(m_lastPos, m_lastRot, true);

        //SetPlayerIgnoreCollision(m_grabbedObj.gameObject, true);
    }

    // OUTDATED:
    // look for thumb collision in the list of all collisions, if any exists, return the other collider.
    // Currently it returns the first thumb collision it finds. If grabbing of multiple objects needed,
    // this should return a list of all thumb collisions.
    protected Collider GetThumbCollisions()
    {
        foreach (short thumbBone in ThumbCollisionBones)
        {
            currentCollisions.TryGetValue(thumbBone, out Collider collisionWithThumb);
            if (collisionWithThumb != null)
                return collisionWithThumb;
        }

        return null;
    }

    // OUTDATED: Check if any collider other than the thumb is grabbing the same object.
    protected bool IsOppositeGrabbing(Collider grabbableCollider)
    {
        if (grabbableCollider)
        {
            foreach (short oppositeBone in OppositeCollisionBones)
            {
                currentCollisions.TryGetValue(oppositeBone, out Collider colliderWithCurrent);

                if (colliderWithCurrent == grabbableCollider)
                    return true;
            }
        }

        return false;
    }
}
