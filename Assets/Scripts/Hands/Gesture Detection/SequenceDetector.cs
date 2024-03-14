using System.Collections.Generic;
using UnityEngine;

// Keeps track of all possible sequences. Sends event to all sequences if hand pose change was detected and checks if any of the sequences has matched (reached last pose in sequence). If there is a match, all sequences are reset to step 0 and sequence match event is triggered for detected sequence.
public class SequenceDetector : MonoBehaviour
{
    [SerializeField] List<GestureSequence> enabledSequences = new List<GestureSequence>();
    [SerializeField] HandController handController = null;
    [SerializeField] OVRHand hand = null;

    bool handWasTracked = false;
    private void ResetAll() => enabledSequences.ForEach(x => x.ResetSequence());

    private void Start()
    {
        // TODO check if each gesture has at least 1 step and if not, show a warnign and disable sequence
        handController.OnHandPoseChanged += OnHandPoseChanged;
        //OVRManager.TrackingLost += ResetAll;
    }

    private void OnDisable()
    {
        handController.OnHandPoseChanged -= OnHandPoseChanged;
       // OVRManager.TrackingLost -= ResetAll;
    }

    private void Update()
    {
        if (handWasTracked && !hand.IsTracked)
        {
            ResetAll();
        }

        handWasTracked = hand.IsTracked;
    }

    private void OnHandPoseChanged(HandPose pose)
    {
        // ignore default pose to avoid gesture sequence resetting in between recognized gestures
        if (pose.Equals(HandPose.Default))
            return;

        // iterate through all available sequences and try to match the current pose with any. 

        foreach (GestureSequence sequence in enabledSequences)
        {
            bool sequenceMatched = sequence.TryMatchSequenceStep(pose);
            
            if (sequenceMatched)
            {
                handController.SequenceDetected(sequence);
                ResetAll();
                break;
            }
        }
    } 
}
