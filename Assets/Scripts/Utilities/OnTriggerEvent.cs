using UnityEngine;
using System;

public class OnTriggerEvent : MonoBehaviour
{
    public event Action<Collider> TriggerEntered;
    public event Action<Collider> TriggerExited;

    public bool debug = false;

    private void OnTriggerEnter(Collider other)
    { 
        if(debug) Debug.Log(string.Format("Trigger enter: {0}.", other));
        TriggerEntered?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if(debug) Debug.Log(string.Format("Trigger exit: {0}.", other));
        TriggerExited?.Invoke(other);
    }
}
