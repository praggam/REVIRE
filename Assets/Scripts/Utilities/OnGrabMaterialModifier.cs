using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnGrabMaterialModifier : MonoBehaviour
{
    [SerializeField]
    Material triggerMaterial = null;
    Material defaulMaterial = null;
    Renderer rend = null;

    BaseGrabber handController = null;

    private void OnEnable()
    {
        rend = gameObject.GetComponent<Renderer>();
        handController = GetComponentInParent<BaseGrabber>();
        defaulMaterial = rend.material;

        if(triggerMaterial == null)
        {
            Debug.LogWarning(String.Format("Color Change Beahviour: missing trigger color"));
        }
    }

    private void Awake()
    {
        // register to listen to grab events from hand controller to change color
        BaseGrabber.OnGrabEnter += OnGrabEnterBehaviour;
        BaseGrabber.OnGrabExit += OnGrabExitBehaviour;
    }

    private void OnDisable()
    {
        BaseGrabber.OnGrabEnter -= OnGrabEnterBehaviour;
        BaseGrabber.OnGrabExit -= OnGrabExitBehaviour;
    }
    private void OnGrabEnterBehaviour(Grabbable go, BaseGrabber hc)
    {
        if(hc == handController)
        {
            rend.material = triggerMaterial;
        }
    }

    private void OnGrabExitBehaviour(Grabbable go, BaseGrabber hc)
    {
        if (hc == handController)
        {
            rend.material = defaulMaterial;
        }
    }


}
