using System;
using System.Collections.Generic;
using UnityEngine;

// Gesture sequence used to recognize non-static gestures such as "count to 3" or "grip-release" which can be used as trigger to show menu and other actions. Keeps track of current progress in sequence and increments if next gesture in sequence was detected, if any other gesture was detected, sequence is reset to start. Once all steps are matched, an event is invoked to signalize SequenceDetector.

// Sequences are not utilized in current implementation.
[CreateAssetMenu(fileName = "GestureSequence", menuName = "Custom/Gesture Sequence")]
public class GestureSequence : ScriptableObject
{
    public new string name = "New Sequence";

    [Tooltip("Sequence of gestures in order of occurence. The same gesture may repear multiple times provided there is another gesture in-between them.")]
    public List<Gesture> sequenceSteps = new List<Gesture>();

    [Tooltip("Speed [in seconds] in which the whole gesture must be performed to be recognized. Currently not in use.")]
    public float sequenceSpeedThreshold = 1f;

    [Tooltip("Current progress in the sequence. Increments when current hand pose matches next pose in sequence.")]
    private int currentStep = 0;

    public int CurrentStep { get => currentStep; set => currentStep = value; }

    // this can be used to display text in debugger etc when sequence step was updated
    public event Action OnUpdate;

    // If current pose matches the next step in the sequence, increment the step, otherwise reset the sequence to 0. Return true if the whole sequence matched (sequence detected).
    public bool TryMatchSequenceStep(HandPose pose)
    {
        if (sequenceSteps[currentStep].handPose.Equals(pose))
        {
            currentStep = (currentStep < sequenceSteps.Count + 1)? currentStep + 1 : 0;            
        }
        else
        {
            ResetSequence();
        }

        OnUpdate?.Invoke();
        return currentStep == sequenceSteps.Count;
    }

    // TODO if needed
    //private bool SequenceTimeout()
    //{
    //    sequenceDuration = DateTime.Now.Subtract(lastIncrementTime).Milliseconds / 100;
    //    return (sequenceDuration >= sequenceSpeedThreshold);
    //}

    public void ResetSequence()
    {
        currentStep = 0;
    }
}
