using Louis.XR.Interactions.Input;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;



namespace Louis.XR.Interactions.Locomotion
{
    public class XRTeleportationManager : MonoBehaviour
    {
        [System.Serializable]
        private class HandTeleportation
        {
            public XRHandSide handSide = XRHandSide.Right;
            public XRRayInteractor ray;

            [HideInInspector] public bool aiming;
        }

        [Header("Teleportation Provider")]
        [SerializeField] private TeleportationProvider provider;

        [Header("Hands")]
        [SerializeField] private HandTeleportation leftHand = new HandTeleportation { handSide = XRHandSide.Left };
        [SerializeField] private HandTeleportation rightHand = new HandTeleportation { handSide = XRHandSide.Right };

        [Header("Settings")]
        [SerializeField] private float aimStartThreshold = 0.7f;
        [SerializeField] private float aimStopThreshold = 0.2f;

        private void Start()
        {
            DisableHand(leftHand);
            DisableHand(rightHand);
        }

        private void DisableHand(HandTeleportation h)
        {
            if (h.ray != null)
                h.ray.gameObject.SetActive(false);

            h.aiming = false;
        }

        private void Update()
        {
            if (provider == null || XRInputRouter.Instance == null)
                return;

            UpdateHand(leftHand);
            UpdateHand(rightHand);
        }

        private void UpdateHand(HandTeleportation h)
        {
            if (h.ray == null)
                return;

            Vector2 stick = XRInputRouter.Instance.Thumbstick(h.handSide);

            // START AIM
            if (!h.aiming && stick.y > aimStartThreshold)
            {
                h.aiming = true;
                h.ray.gameObject.SetActive(true);
                return;
            }

            // AIM LOOP
            if (h.aiming)
            {
                // STOP AIM -> TRY TELEPORT
                if (stick.y < aimStopThreshold)
                {
                    TryTeleport(h);
                    DisableHand(h);
                }
            }
        }

        private void TryTeleport(HandTeleportation h)
        {
            if (!h.ray.TryGetCurrent3DRaycastHit(out var hit))
                return;

            provider.QueueTeleportRequest(new TeleportRequest
            {
                destinationPosition = hit.point,
                matchOrientation = MatchOrientation.WorldSpaceUp,
            });
        }
    }
}
