using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Responsible for detecting gestures. There should be 1 gesture detector per hand in the scene. Detector iterates over all available hand gestures for the hand and chooses the gesture with bone positions closest to the current state. Uses threshold variable of the gesture to reject gesture if maximum distance tolerance is exceeded. If no match is found, gesture is set to Default. 

// Detector communicates every change to HandController, when a new gesture is detected of when current gesture is no longer valid (changing to Default).
public class GestureDetector : MonoBehaviour
{
    [SerializeField] OVRCustomSkeleton skeleton = null; 
    [SerializeField] OVRHand hand = null;
    [SerializeField] List<Gesture> gestures = new List<Gesture>();

    Gesture previousGesture = null;
    List<OVRBone> fingerBones;
    HandController handController = null;

    void Start()
    {
        handController = hand.gameObject.GetComponent<HandController>();
        CheckInvalidGestures();
        StartCoroutine(GetFingerBones());
    }

    public virtual void Update()
    {
        if (!hand.HandConfidence.Equals(OVRHand.TrackingConfidence.High))
            return;

        Gesture currentGesture = DetectGesture();

        if(currentGesture == null)
        { 
            if(previousGesture != null)
            {
                handController.PoseChanged(HandPose.Default);
                previousGesture = null;
            }
        }
        else if(!currentGesture.Equals(previousGesture))
        {
            //handController.Pose = currentGesture.handPose;
            previousGesture = currentGesture;

            // call hand pose changed event 
            handController.PoseChanged(currentGesture.handPose);
        }   
    }

    protected Gesture DetectGesture()
    {
        Gesture currentGesture = null;
        float currentMin = Mathf.Infinity;

        foreach (Gesture gesture in gestures)
        {
            if (gesture == null)
            {
                Debug.LogError("Invalid gesture in Gesture Detector. This should never happen.");
                continue;
            }
                

            float sumDistance = 0;
            bool isRejected = false;

            for (int i = 0; i < fingerBones?.Count; ++i)
            {
                Vector3 currentData = skeleton.transform.InverseTransformPoint(fingerBones[i].Transform.position);
                float distance = Vector3.Distance(currentData, gesture.bonePositions[i]);

                // check if distance within threshold, if not, don't consider this gesture  
                if (distance > gesture.detectionThreshold)
                {
                    isRejected = true;
                    break;
                }

                sumDistance += distance;
            }

            if (!isRejected && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }
        }
        return currentGesture;
    }

    // wait until skeleton bones are initialized in OVRSkeleton to assign bones
    private IEnumerator GetFingerBones()
    {
        while(skeleton.Bones.Count < 24)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        fingerBones = new List<OVRBone>(skeleton.Bones);
    }

    // if gesture list contains a gesture for the wrong hand or empty list element, remove it
    private void CheckInvalidGestures()
    {
        foreach (Gesture gesture in gestures)
        {
            if (gesture == null || gesture.skeletonType.Equals(skeleton.GetType()))
            {
                Debug.LogError(string.Format("Invalid gesture in Gesture Detector {0}. Please check if all gestures are  assigned correctly in the Inpector. Empty list elements are not permitted.", skeleton.GetSkeletonType(), gesture));
                break;
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Autofill New Gestures")]
    public void AddAllGestures()
    {
        string[] assets = AssetDatabase.FindAssets("t: Gesture");

        foreach (string asset in assets)
        {
            string guid = AssetDatabase.GUIDToAssetPath(asset);
            Gesture gesture = AssetDatabase.LoadAssetAtPath<Gesture>(guid);
            if (gesture != null && !gestures.Contains(gesture) && gesture.skeletonType.Equals(skeleton.GetSkeletonType()))
            {

                gestures.Add(gesture);
            }
        }
    }
#endif

}
