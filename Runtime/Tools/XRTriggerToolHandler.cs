using Louis.Core.Input;
using Louis.Core.Tools;
using Louis.XR.Interactions.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Louis.XR.Interactions.Tools
{
    [DisallowMultipleComponent]
    public class XRTriggerToolHandler : MonoBehaviour
    {
        [Header("XR Grabbale Trigger Tool")]
        [SerializeField] private XRGrabInteractable grab;

        [SerializeField] private float pressTreshold = 0.1f;

        [SerializeField] private FloatInputDefinition triggerInput = new FloatInputDefinition("Trigger");

        private ITriggerTool tool;
        private bool isHeld = false;
        private bool wasPressed = false;
        private XRHandSide currentHandSide = XRHandSide.Right;

        private void Reset()
        {
            grab = GetComponent<XRGrabInteractable>();
            tool = GetComponent<ITriggerTool>();
        }

        private void Awake()
        {
            if (grab == null)
                grab = GetComponent<XRGrabInteractable>();
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
            if (args.interactorObject is XRSocketInteractor)
                return;

            isHeld = true;
            wasPressed = false;

            switch(args.interactorObject.handedness)
            {
                case InteractorHandedness.Left:
                    currentHandSide = XRHandSide.Left;
                    break;
                case InteractorHandedness.Right:
                    currentHandSide = XRHandSide.Right;
                    break;
                default:
                    currentHandSide = XRHandSide.Right;
                    break;
            }
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (args.interactorObject is XRSocketInteractor)
                return;

            isHeld = false;

            if (wasPressed && tool != null)
                tool.OnTriggerReleased();

            wasPressed = false;

        }

        void Update()
        {
            if (!isHeld || tool == null || triggerInput == null)
                return;

            float value = triggerInput.GetValue((int)currentHandSide);
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
