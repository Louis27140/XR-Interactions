using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Louis.XR.Interactions.Teleport
{

    public class TeleportationManager : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset input;

        [Header("XR")]
        [SerializeField] private TeleportationProvider provider;
        [SerializeField] private XRRayInteractor rightTeleportRay;
        [SerializeField] private XRRayInteractor leftTeleportRay;

        private InputAction rightActivate;
        private InputAction rightCancel;
        private InputAction leftActivate;
        private InputAction leftCancel;

        private bool isActive;
        private XRRayInteractor currentRay;

        private void Start()
        {
            rightTeleportRay.enabled = false;
            leftTeleportRay.enabled = false;

            var rightMap = input.FindActionMap("XRI RightHand Locomotion");
            var leftMap = input.FindActionMap("XRI LeftHand Locomotion");

            rightActivate = rightMap.FindAction("Teleport Mode Activate");
            rightCancel = rightMap.FindAction("Teleport Mode Cancel");

            leftActivate = leftMap.FindAction("Teleport Mode Activate");
            leftCancel = leftMap.FindAction("Teleport Mode Cancel");

            rightActivate.Enable();
            rightCancel.Enable();
            leftActivate.Enable();
            leftCancel.Enable();

            rightActivate.performed += OnRightActivate;
            rightActivate.canceled += OnTeleportValidate;

            leftActivate.performed += OnLeftActivate;
            leftActivate.canceled += OnTeleportValidate;

            rightCancel.performed += OnTeleportCancel;
            leftCancel.performed += OnTeleportCancel;
        }

        private void OnDestroy()
        {
            rightActivate.performed -= OnRightActivate;
            rightActivate.canceled -= OnTeleportValidate;
            leftActivate.performed -= OnLeftActivate;
            leftActivate.canceled -= OnTeleportValidate;
            rightCancel.performed -= OnTeleportCancel;
            leftCancel.performed -= OnTeleportCancel;
        }

        private void OnRightActivate(InputAction.CallbackContext ctx)
        {
            TryActivateTeleport(rightTeleportRay);
        }

        private void OnLeftActivate(InputAction.CallbackContext ctx)
        {
            TryActivateTeleport(leftTeleportRay);
        }

        private void TryActivateTeleport(XRRayInteractor ray)
        {
            // If teleport is already active with the other hand, ignore
            if (isActive && currentRay != ray)
                return;

            currentRay = ray;
            currentRay.enabled = true;
            isActive = true;
        }

        private void OnTeleportValidate(InputAction.CallbackContext ctx)
        {
            if (!isActive || currentRay == null)
                return;

            if (!currentRay.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                currentRay.enabled = false;
                currentRay = null;
                isActive = false;
                return;
            }

            if (hit.transform.TryGetComponent<TeleportationAnchor>(out TeleportationAnchor anchor))
            {
                anchor.RequestTeleport();
            }
            else
            {
                // Forward direction of camera, flattened on horizontal plane
                Transform cam = Camera.main.transform;
                Vector3 flatForward = cam.forward;
                flatForward.y = 0f;

                if (flatForward.sqrMagnitude < 0.0001f)
                    flatForward = cam.parent != null ? cam.parent.forward : Vector3.forward;

                flatForward.Normalize();

                TeleportRequest tpRequest = new TeleportRequest
                {
                    destinationPosition = hit.point,
                    destinationRotation = Quaternion.LookRotation(flatForward, Vector3.up),
                    matchOrientation = MatchOrientation.TargetUp
                };

                provider.QueueTeleportRequest(tpRequest);
            }

            currentRay.enabled = false;
            currentRay = null;
            isActive = false;
        }

        private void OnTeleportCancel(InputAction.CallbackContext ctx)
        {
            if (!isActive || currentRay == null)
                return;

            currentRay.enabled = false;
            currentRay = null;
            isActive = false;
        }
    }
}
