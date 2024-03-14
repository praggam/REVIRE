using System;
using UnityEngine;

// Responsible for all functionalities related to user calibration. Currently implements maximum arms reach calibration which is used to position interactable objects in tasks. Calibration can be done with one or both hands, if both hands used, the hand closer to user is used to measure maximum reach. Eventually may have to save max reach of both hands and differentiate for tasks e.g. to position grabbables closer for impaired hand and farther for the other
public class CalibrationManager : MonoBehaviour
{
    [Tooltip("HMD position of Camera Rig is used to position the calibration lines facing user.")]
    [SerializeField] OVRCameraRig cameraRig = null;

    [Tooltip("Position of the marker is used to determine max hand reach upward/forward.")]
    [SerializeField] GameObject leftHandReach = null;

    [Tooltip("Position of the marker is used to determine max hand reach upward/forward.")]
    [SerializeField] GameObject rightHandReach = null;

    [Tooltip("Game object containing a plane to project measured max reach forward during calibration.")]
    [SerializeField] GameObject projectionForward = null;

    [Tooltip("Game object containing a plane to project measured max reach upward during calibration.")]
    [SerializeField] GameObject projectionUpward = null;

    [Tooltip("Measured maximum forward arms reach for the user.")]
    private static float maxReachForward = 0f;

    [Tooltip("Measured maximum upward arms reach for the user.")]
    private static float maxReachUpward = 0f;

    [SerializeField] GameObject table = null;
    [SerializeField] GameObject sphereCalib = null;

    const float TableHeight = 0.75f;
    const float FingerHeight = 0.02f;
    const float TableWidth = 0.45f;
    const float TableInitialY = 0.007f;
    const float TableInitialX = 3.75f;

    private void OnEnable()
    {
        Debug.Assert(leftHandReach && rightHandReach,
            "Calibration Manager: Hand references not assigned.");
    }

    // Used by 'Recenter View' button. Only works in Unity Editor Mode as this functionality was disabled for compiled apps by Oculus.
    public void ResetHMDPosition()
    {
        OVRManager.display.RecenterPose();


        //SetMaxReachForward();
    }

    public void RotateCameraRigLeft()
    {
        Vector3 rot = cameraRig.transform.rotation.eulerAngles;
        cameraRig.transform.eulerAngles = new Vector3(rot.x, rot.y + 1, rot.z);
    }

    public void RotateCameraRigRight()
    {
        Vector3 rot = cameraRig.transform.rotation.eulerAngles;
        cameraRig.transform.eulerAngles = new Vector3(rot.x, rot.y - 1, rot.z);
    }

    // save farthest hand position in world space placed on the desk. If both hands are tracked, pick the reach of the hand reaching closer. Called through UI.
    public void SetMaxReachForward()
    {
        Vector3 rot = cameraRig.transform.rotation.eulerAngles;

        //int counter = 0;
        //while (Math.Abs(rightHandReach.transform.position.x - leftHandReach.transform.position.x) > 0.001 && counter < 90)
        //{
        //    var dir = rightHandReach.transform.position.x > leftHandReach.transform.position.x
        //    ? 1
        //    : -1;

        //    cameraRig.transform.eulerAngles = new Vector3(rot.x, rot.y - dir, rot.z);
        //    counter++;
        //}

        float calibCameraY = rightHandReach.transform.position.y;
        float calibCameraX = rightHandReach.transform.position.x;

        var tablePos = table.transform.position;
        Vector3 camPos = cameraRig.transform.position;

        var diff_x = rightHandReach.transform.position.x - tablePos.x - TableWidth;
        var diff_y = rightHandReach.transform.position.y - TableHeight + FingerHeight;

        cameraRig.transform.position = new Vector3(camPos.x - diff_x, camPos.y - diff_y, camPos.z);


        return;



        float calibTableY = rightHandReach.transform.position.y - TableHeight + FingerHeight;
        float calibTableX = rightHandReach.transform.position.x - TableWidth;



        table.transform.position = new Vector3(
            calibTableX,
            calibTableY,
            tablePos.z);


        float offsetY = calibTableY - 0.007f;
        float offsetX = calibTableX - 3.75f;



        SettingsManager.Instance.SetOffsetX(offsetX);
        SettingsManager.Instance.SetOffsetY(offsetY);

        return;

        float tmpReachUpward;

        if (HandsManager.Instance.BothHandsTracked)
        {
            float l = leftHandReach.transform.position.x;
            float r = rightHandReach.transform.position.x;
            maxReachForward = l >= r ? l : r;
            tmpReachUpward = leftHandReach.transform.position.y;
        }
        else if (HandsManager.Instance.LeftHandActive)
        {
            maxReachForward = leftHandReach.transform.position.x;
            tmpReachUpward = leftHandReach.transform.position.y;
        }
        else if (HandsManager.Instance.RightHandActive)
        {
            maxReachForward = rightHandReach.transform.position.x;
            tmpReachUpward = rightHandReach.transform.position.y;
        }
        else
        {
            // TODO display warning once notifications implemented and retry getting position
            Debug.LogWarning("Calibration Manager: Hands inactive.");
            return;
        }

        DrawLine(projectionForward, new Vector3(
            x: maxReachForward,
            y: tmpReachUpward,
            z: projectionForward.transform.position.z));
    }

    // save highest hand position in world space. If both hands are tracked, pick the reach of the hand reaching closer.  Called via UI.
    public void SetMaxReachUpward()
    {
        float tmpReachForward;

        if (HandsManager.Instance.BothHandsTracked)
        {
            float l = leftHandReach.transform.position.y;
            float r = rightHandReach.transform.position.y;
            maxReachUpward = l >= r ? l : r;
            tmpReachForward = leftHandReach.transform.position.x;
        }
        else if (HandsManager.Instance.LeftHandActive)
        {
            maxReachUpward = leftHandReach.transform.position.y;
            tmpReachForward = leftHandReach.transform.position.x;
        }
        else if (HandsManager.Instance.RightHandActive)
        {
            maxReachUpward = rightHandReach.transform.position.y;
            tmpReachForward = rightHandReach.transform.position.x;
        }
        else
        {
            // TODO display warning once notifications implemented and retry getting position
            Debug.LogWarning("Calibration Manager: Hands inactive.");
            return;
        }

        DrawLine(projectionUpward, new Vector3(
                x: tmpReachForward,
                y: maxReachUpward,
                z: projectionUpward.transform.position.z));
    }

    // save in persistent storage. Called via UI on calibration end.
    public void SaveMaxReach()
    {

        SettingsManager.Instance.SetMaxArmsReach(maxReachUpward, maxReachForward);
    }

    // use referenced projection game object to position the plane at measured arms reach and display it
    public void DrawLine(GameObject line, Vector3 position)
    {
        line.transform.position = position;

        // make sure the front of the plane is facing user by rotating it
        if (cameraRig.centerEyeAnchor.transform.position.x >= line.transform.position.x)
        {
            line.transform.Rotate(line.transform.forward, 180);
        }

        //if(cameraRig.centerEyeAnchor.transform.up.y >= line.transform.up.y)
        if (cameraRig.centerEyeAnchor.transform.position.y >= line.transform.position.y)
        {
            line.transform.Rotate(line.transform.up, 180);
        }

        line.SetActive(true);
    }

    // Called via UI to hide both projection lines on calibration panel close.
    public void HideLine()
    {
        projectionForward.SetActive(false);
        projectionUpward.SetActive(false);
    }
}
