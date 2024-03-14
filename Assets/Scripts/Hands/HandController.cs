using System;
using UnityEngine;

[RequireComponent(typeof(OVRHand))]
public class HandController : MonoBehaviour
{
    private HandPose pose = HandPose.Default;
    private HandState state = HandState.DEFAULT;

    public HandPose Pose { get => pose; set => pose = value; }
    public HandState State { get => state; set => state = value; }

    public event Action<HandPose> OnHandPoseChanged;
    public event Action<GestureSequence> OnSequenceDetected;

    public void PoseChanged(HandPose newPose)
    {
        // if hand gesture changed but hand pose remains the same return. This can happen for multiple variants of the same hand pose.
        if (newPose.Equals(pose))
            return;

        pose = newPose;
        OnHandPoseChanged?.Invoke(newPose);
    }

    public void SequenceDetected(GestureSequence sequence)
    {
        Debug.LogWarning("Sequence detected: " + sequence.name);
        OnSequenceDetected?.Invoke(sequence);
    }
}
