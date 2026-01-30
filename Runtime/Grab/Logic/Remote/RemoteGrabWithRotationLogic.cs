using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Louis.XR.Interactions.Input;
using Louis.Core.Input;

namespace Louis.XR.Interactions.Grab.Logic.Remote
{
    public class RemoteGrabWithRotationLogic : XRRemoteLogicBase
    {
        [Header("Input Definitions")]
        [SerializeField] private Vector2InputDefinition joystickInput;
        [SerializeField] private BoolInputDefinition toggleRotationInput;
        [SerializeField] private BoolInputDefinition resetInput;

        [Header("Settings")]
        [SerializeField] private float speed = 1f;
        [SerializeField] private float rotationSpeed = 100.0f;

        [Header("Runtime State")]
        private bool isRotating = false;
        private float currentDistance;
        private bool wasButtonPressed = false;

        private XRBaseInteractable.MovementType originalMovementType;
        private bool originalTrackRotation;

        public override bool OnSelectEntering(XRContext context)
        {
            base.OnSelectEntering(context);

            XRInputRouter.PushContext(4);

            currentDistance = Vector3.Distance(context.interactor.transform.position, context.interactable.transform.position);

            originalMovementType = context.interactable.movementType;
            originalTrackRotation = context.interactable.trackRotation;

            return true;
        }

        public override void Process(XRContext context)
        {
            if (context.interactor is not XRRayInteractor) return;

            if (joystickInput == null || toggleRotationInput == null || resetInput == null)
            {
                Debug.LogWarning("[RemoteGrabWithRotation] InputDefinitions not assigned in Inspector!");
                return;
            }

            int channel = (int)context.hand - 1;

            if (channel < 0 || channel > 1)
            {
                Debug.LogWarning($"[RemoteGrabWithRotation] Invalid channel {channel} from hand {context.hand}");
                return;
            }

            Vector2 joystickValue = joystickInput.GetValue(channel);
            bool buttonPressed = toggleRotationInput.GetValue(channel);
            bool joystickClicked = resetInput.GetValue(channel);

            if (buttonPressed && !wasButtonPressed)
            {
                isRotating = !isRotating;

                if (isRotating && context.interactable.trackRotation)
                {
                    context.interactable.trackRotation = false;
                }
            }
            wasButtonPressed = buttonPressed;

            if (joystickClicked)
            {
                ResetTransform(context);
            }

            if (isRotating)
            {
                float pitchRotation = -joystickValue.y * rotationSpeed * context.deltaTime;
                float yawRotation = joystickValue.x * rotationSpeed * context.deltaTime;

                context.interactable.transform.Rotate(Vector3.up, yawRotation, Space.World);
                context.interactable.transform.Rotate(Vector3.right, pitchRotation, Space.Self);
            }
            else
            {
                currentDistance += joystickValue.y * speed * context.deltaTime;
                currentDistance = Mathf.Max(0.1f, currentDistance);

                Vector3 direction = (context.interactable.transform.position - context.interactor.transform.position).normalized;
                Vector3 targetPosition = context.interactor.transform.position + direction * currentDistance;

                context.interactable.transform.position = targetPosition;
            }
        }

        private void ResetTransform(XRContext context)
        {
            isRotating = false;

            context.interactable.trackRotation = originalTrackRotation;
            context.interactable.movementType = originalMovementType;

            context.interactable.transform.localScale = Vector3.one;
            context.interactable.transform.localRotation = Quaternion.identity;
        }

        public override void OnSelectExited(XRContext context)
        {
            base.OnSelectExited(context);
            isRotating = false;

            XRInputRouter.PopContext();

            context.interactable.trackRotation = originalTrackRotation;
            context.interactable.movementType = originalMovementType;
        }
    }
}
