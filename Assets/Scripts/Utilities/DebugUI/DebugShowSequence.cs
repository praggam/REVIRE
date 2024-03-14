using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugShowSequence : MonoBehaviour
{
    [SerializeField] GestureSequence sequence = null;
    [SerializeField] TMP_Text sequenceName = null;
    [SerializeField] TMP_Text sequenceData = null;

    private int lastValue = 0;

    private void Awake()
    {
        sequenceName.text = sequence.name;
        sequence.OnUpdate += UpdateText;
    }

    private void OnDisable() => sequence.OnUpdate -= UpdateText;

    private void UpdateText() 
    {
        int idx = sequence.CurrentStep;
        if (idx != lastValue)
        {
            sequenceData.color = Color.white;
            lastValue = sequence.CurrentStep;
            sequenceData.text = "";
            List<Gesture> seq = sequence.sequenceSteps;
            for (int i = 0; i < idx; ++i)
            {
                sequenceData.text += string.Format("\n[{0}]: {1}", i, seq[i].handPose);
            }

            if(idx == sequence.sequenceSteps.Count)
            {
                StartCoroutine(ShowColor());
            }
        }
    }

    private IEnumerator ShowColor()
    {
        sequenceData.color = Color.green;
        yield return new WaitForSeconds(2f);
        sequenceData.color = Color.white;
    }

}
