using Louis.Core.Tools;
using Louis.XR.Interactions.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Louis.XR.Interactions.Tools
{
    [DisallowMultipleComponent]
    public class XRTriggerToolHandler : MonoBehaviour
    {
        [Header("XR Grabbale Trigger Tool")]
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

        [SerializeField] private float pressTreshold = 0.1f;

        private ITriggerTool tool;
        private bool isHeld = false;
        private bool wasPressed = false;
        private XRHandSide currentHandSide = XRHandSide.Right;

        private void Reset()
        {
            grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            tool = GetComponent<ITriggerTool>();
        }

        private void Awake()
        {
            if (grab == null)
                grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (tool == null)
                tool = GetComponent<ITriggerTool>();
        }

        private void OnEnable()
        {
            if (grab != null)
            {
                grab.selectEntered.AddListener(OnSelectEntered);
                grab.selectExited.AddListener(OnSelectExited);
            }
        }

        private void OnDisable()
        {
            if (grab != null)
            {
                grab.selectEntered.RemoveListener(OnSelectEntered);
                grab.selectExited.RemoveListener(OnSelectExited);
            }
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
                return;

            isHeld = true;
            wasPressed = false;

            Debug.Log($"[{gameObject.name}] Grab entered - isHeld = true", this);

            // D�tection de la main � partir du nom de l'interactor (Left / Right)
            string interactorName = args.interactorObject.transform.name.ToLower();
            if (interactorName.Contains("left"))
                currentHandSide = XRHandSide.Left;
            else if (interactorName.Contains("right"))
                currentHandSide = XRHandSide.Right;
            else
                currentHandSide = XRHandSide.Right;
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor)
                return;

            isHeld = false;

            if (wasPressed && tool != null)
                tool.OnTriggerReleased();

            wasPressed = false;

            Debug.Log($"[{gameObject.name}] Grab exited - isHeld = false", this);
        }

        void Update()
        {
            if (!isHeld || tool == null || XRInputRouter.Instance == null)
                return;

            float value = XRInputRouter.Instance.TriggerValue(currentHandSide);
            bool isPressed = value >= pressTreshold;

            if (isPressed && !wasPressed)
            {
                tool.OnTriggerPressed();
            }
            else if (!isPressed && wasPressed)
            {
                tool.OnTriggerReleased();
            }

            wasPressed = isPressed;
        }
    }
}
