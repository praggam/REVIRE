using UnityEngine;
using System.Threading;

// This script can be attached to a grabbable object itself to change behaviour when it is grabbed or to other objects such as hand fingers or environment objects.
public class OnGrabBehaviour : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Material to change to when this behaviour is triggered.")]
    private Material materialOnGrabEnter = null;

    [SerializeField]
    [Tooltip("Grabbable GameObject that should trigger this behaviour. If empty, this behaviour will trigger for any object grabbed by the specified hand. ")]
    private GameObject grabbableTrigger = null;

    private Material materialOnGrabExit = null;
    private Renderer rend = null;

    [SerializeField]
    [Tooltip("HandController that should trigger this behaviour. If empty, this behaviour will trigger for both hands.")]
    private BaseGrabber grabbingHand = null;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        materialOnGrabExit = rend.material;

        BaseGrabber.OnGrabEnter += OnGrabEnter;
        BaseGrabber.OnGrabExit += OnGrabExit;
    }

    private void OnDestroy()
    {
        BaseGrabber.OnGrabEnter -= OnGrabEnter;
        BaseGrabber.OnGrabExit -= OnGrabExit;
    }

    private void OnGrabEnter(Grabbable go, BaseGrabber hand)
    {
        if (grabbableTrigger && go != grabbableTrigger) return;
        if (grabbingHand && hand != grabbingHand) return;

        if (materialOnGrabEnter != null)
            rend.material = materialOnGrabEnter;
        
        int grabObjNo = 0;
        switch (go.name)
        {
            case "Glass": if (go.IsHeld) grabObjNo = 111; break;
            case "Bottle": grabObjNo = 112; break;
            case "Glass2": grabObjNo = 213; break;
            case "Box": grabObjNo = 314; break;
        }
        float[] data = new float[] { grabObjNo };
        LSLSender.SendLsl("Grabbing " + go.name, data);
        //Thread FirstThread = new Thread(() => LSLSender.SendLsl("Grabbing " + go.name, data));
        //FirstThread.Start();

    }

    private void OnGrabExit(Grabbable go, BaseGrabber hand)
    {
        if (grabbableTrigger && go != grabbableTrigger) return;
        if (grabbingHand && hand != grabbingHand) return;
        
        if (materialOnGrabExit != null)
            rend.material = materialOnGrabExit;

        if (go.AutoRegrabbing == false)
        {
            int grabObjNo = 0;
            switch (go.name)
            {
                case "Glass": grabObjNo = 121; break;
                case "Bottle": grabObjNo = 122; break;
                case "Glass2": grabObjNo = 223; break;
                case "Box": grabObjNo = 324; break;
            }
            float[] data = new float[] { grabObjNo };
            LSLSender.SendLsl("Released " + go.name, data);
        }
    }
}
