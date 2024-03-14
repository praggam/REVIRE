using System;
using System.Timers;
using UnityEngine;

public class HandsManager : Singleton<HandsManager>
{
    [SerializeField]
    private OVRHand handLeft = null;

    [SerializeField]
    private OVRHand handRight = null;

    [Tooltip("Enable to allow kinematic grabbing of objects by right/left hand.")]
    public bool kinematicGrabbing = true;

    [Tooltip("Enable to allow both-handed grabbing of objects. Note that it is not recommended to use both-handed grabbing with kinematic grabber at the same time.")]
    public bool bothHandedGrabbing = false;

    public bool BothHandsTracked => IsHandActive(handLeft) && IsHandActive(handRight);
    public bool LeftHandActive => IsHandActive(handLeft);
    public bool RightHandActive => IsHandActive(handRight);

    public OVRHand HandLeft { get => handLeft; set => handLeft = value; }
    public OVRHand HandRight { get => handRight; set => handRight = value; }

    private DateTime positioningUpdateTime = DateTime.Now;
    private Timer timer = new Timer(100);
    private bool rightHandPositioning = false;
    private bool leftHandPositioning = false;

    public event Action OnBothHandsInRestLocation;

    private void Awake()
    {
        InitializeSingleton(this);
        HandLeft.OnHandMove += HandLeft_OnHandMove;
        HandRight.OnHandMove += HandRight_OnHandMove;
        timer.Elapsed += Timer_Elapsed;
        
    }

    public void StartHandTracking()
    {
        timer.Start();
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        rightHandPositioning = true;
        leftHandPositioning = true;
        //Debug.LogWarning("TRACKING &&&&&&&&&");
    }

    private void HandRight_OnHandMove(Vector3 obj)
    {
        if (!rightHandPositioning)
            return;

        rightHandPositioning = false;

        checkBothHandsInRestLocation();

        //float[] pos = { 80f, obj.x, obj.y, obj.z };

        SendLslForPosition(80, obj);

    }

    private void HandLeft_OnHandMove(Vector3 obj)
    {
        if (!leftHandPositioning)
            return;
        
        leftHandPositioning = false;

        checkBothHandsInRestLocation();

        //float[] pos = { 90f, obj.x, obj.y, obj.z };
        //LSLSender.SendLsl("Left Hand Position", pos);

        SendLslForPosition(90, obj);


    }

    private void SendLslForPosition(float prefix, Vector3 obj)
    {
        string side = (prefix == 80) ? "Right" : "Left";

        float decoded_x = decodePositionNumber(prefix + 1, obj.x);
        float decoded_y = decodePositionNumber(prefix + 2, obj.y);
        float decoded_z = decodePositionNumber(prefix + 3, obj.z);
        LSLSender.SendLslForHandPos(side + " Hand Position X", new float[] { decoded_x });
        LSLSender.SendLslForHandPos(side + " Hand Position Y", new float[] { decoded_y });
        LSLSender.SendLslForHandPos(side + " Hand Position Z", new float[] { decoded_z });
    }

    private float decodePositionNumber(float prefix, float number)
    {
        float sign = (number > 0) ? 1 : -1;
        return sign * float.Parse(prefix.ToString() + Mathf.Abs(number).ToString());
    }

    const float TableHeight = 0.75f;
    const float TableWidth = 0.45f;
    public void checkBothHandsInRestLocation()
    {
        float baseX = SettingsManager.Instance.Settings.OffsetX;
        float baseY = SettingsManager.Instance.Settings.OffsetY;

        float tolerance = 0.04f;

        float hly = handLeft.transform.position.y;
        float hry = handRight.transform.position.y;
        if (
            hly < baseY + TableHeight + tolerance &&
            hly > baseY + TableHeight - tolerance &&
            hry < baseY + TableHeight + tolerance &&
            hry > baseY + TableHeight - tolerance)
        {
            GameManager.Instance.HandsManager_OnBothHandsInRestLocation();
        }
    }

    public void ResetGrabbables()
    {
        // reset active streams
        StreamBehaviour[] streams = FindObjectsOfType<StreamBehaviour>();
        foreach (StreamBehaviour s in streams)
        {
            Destroy(s.gameObject);
        }

        // reset positions
        Grabbable[] grabbables = FindObjectsOfType<Grabbable>();
        for (int i = 0; i < grabbables.Length; ++i)
        {
            if (grabbables[i].name == "Glass2")
            {
                Container glass2 = grabbables[i].GetComponent<Container>();
                glass2.filled = glass2.maxCapacity;
            }
            if (grabbables[i].name == "Glass")
            {
                Container glass = grabbables[i].GetComponent<Container>();
                glass.filled = 0;
            }

            grabbables[i].ResetPosition();
            grabbables[i].IsHeld = false;
        }
        LSLSender.SendLsl("Reset Object Clicked!", new float[] { 400 });
    }

    private bool IsHandActive(OVRHand hand)
    {
        return 
            hand != null &&
            hand.IsTracked &&
            hand.HandConfidence.Equals(OVRHand.TrackingConfidence.High);
    }

    public void ResetObjects()
    {
        // reset active streams
        StreamBehaviour[] streams = FindObjectsOfType<StreamBehaviour>();
        foreach (StreamBehaviour s in streams)
        {
            Destroy(s.gameObject);
        }

        // reset positions
        Grabbable[] grabbables = FindObjectsOfType<Grabbable>();
        for (int i = 0; i < grabbables.Length; ++i)
        {
            if(grabbables[i].name != "Box2")
            {
                grabbables[i].ResetPosition();
                Container box = grabbables[i].GetComponent<Container>();
                
            }
            if (grabbables[i].name == "Glass2")
            {
                Container glass2 = grabbables[i].GetComponent<Container>();
                glass2.filled = glass2.maxCapacity;
            }
            if (grabbables[i].name == "Glass")
            {
                Container glass = grabbables[i].GetComponent<Container>();
                glass.filled = 0;
            }

            grabbables[i].IsHeld = false;
        }
    }
}