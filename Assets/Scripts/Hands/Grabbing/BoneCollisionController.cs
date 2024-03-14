using UnityEngine;
using System;

// Attached to hand finger bones will trigger collision events which hand controller is listening for, if collided with Grabbale object.
[RequireComponent(typeof(Rigidbody))]
public class BoneCollisionController : MonoBehaviour
{
    public short boneid = -1;
    public OVRSkeleton.SkeletonType hand = OVRSkeleton.SkeletonType.None;

    // collision events from bone colliders. Hand controller should listen to collision events to trigger grab action. Right hand controller will listen to '_R' events and left to '_L'.
    public static event Action<short, Collider> OnCollisionEnter_L;
    public static event Action<short, Collider> OnCollisionEnter_R;
    public static event Action<short, Collider> OnCollisionExit_L;
    public static event Action<short, Collider> OnCollisionExit_R;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.gameObject.GetComponent<Grabbable>())
            return;

        if (collision.collider.gameObject.CompareTag("Liquid"))
            return;

        if(hand == OVRSkeleton.SkeletonType.HandLeft)
        {
            OnCollisionEnter_L(boneid, collision.collider);
        }
        else if(hand == OVRSkeleton.SkeletonType.HandRight)
        {
            OnCollisionEnter_R(boneid, collision.collider);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!collision.collider.gameObject.GetComponent<Grabbable>())
            return;

        if (collision.collider.gameObject.CompareTag("Liquid"))
            return;

        if (hand == OVRSkeleton.SkeletonType.HandLeft)
        {
            OnCollisionExit_L(boneid, collision.collider);
        }
        else if (hand == OVRSkeleton.SkeletonType.HandRight)
        {
            OnCollisionExit_R(boneid, collision.collider);
        }
    }
}
