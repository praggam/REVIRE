using System.Collections.Generic;
using UnityEngine;

// Scriptable object used to store gesture data. There should be at least 2 objects saved for the same gesture, one per hand. Implementation allows to store multiple gestures with the same hand pose to detect more variations of the gesture if needed.
[CreateAssetMenu(fileName = "Gesture", menuName = "Custom/Gesture")]
public class Gesture: ScriptableObject
{
    public new string name = "New Gesture";

    [Tooltip("Maximum difference/tolerance between stored bone positions distance for gesture and actual detected positions needed to classify as given gesture. Value is summed over total of all bones. ")]
    public float detectionThreshold = 0.05f;

    [Tooltip("Differentiate gesture per hand.")]
    public OVRSkeleton.SkeletonType skeletonType = OVRSkeleton.SkeletonType.None;
    
    [Tooltip("Hand pose that will be triggered when this gesture detected. Enables storing of multiple variations of the same gesture.")]
    public HandPose handPose = HandPose.Default;
    public List<Vector3> bonePositions = new List<Vector3>();
    
    public Gesture(string name, OVRSkeleton.SkeletonType skeleton, List<Vector3> bonePositions)
    {
        this.name = name;
        this.bonePositions = bonePositions;
        this.skeletonType = skeleton;
    }
}
