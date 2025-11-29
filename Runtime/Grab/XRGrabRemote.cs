using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Louis.XR.Interactions.Grab
{

    public class XRGrabRemote : XRGrabInteractable
    {
        [Header("Remote trigger")]
        [Tooltip("Minimum velocity of the ray to trigger the remote grab")]
        public float velocityThreshold = 2f;

        [Tooltip("Time it takes for the object to reach the frozen target")]
        public float travelTime = 0.35f;

        [Header("Auto grab")]
        [Tooltip("If true, object will be grabbed at the end of traveling")]
        public bool isAutoGrabbed = false;

        [Tooltip("Max distance between hand and object attach point to auto grab")]
        public float autoGrabDistance = 0.2f;

        [Header("Trajectory")]
        [Tooltip("Use a small arc instead of a straight line")]
        public bool useArc = true;

        [Tooltip("Maximum height of the arc in meters")]
        public float arcHeight = 0.2f;

        private XRDirectInteractor grabInteractor;
        private IXRSelectInteractor grabSelectInteractor;

        private XRRayInteractor ray;
        private Rigidbody rb;

        private Vector3 previousRayPos;
        private bool previousRayPosInitialized = false;

        private bool canJump = true;
        private bool isFlying = false;
        private float flyT = 0f;
        private Vector3 startPos;
        private Vector3 frozenTargetPos;

        protected override void Awake()
        {
            base.Awake();
            rb = GetComponent<Rigidbody>();
        }

        private Transform GetObjectAttach()
        {
            return attachTransform != null ? attachTransform : transform;
        }

        private Transform GetHandAttach()
        {
            if (grabInteractor == null)
                return null;

            return grabInteractor.attachTransform != null
                ? grabInteractor.attachTransform
                : grabInteractor.transform;
        }

        private void FixedUpdate()
        {
            // Phase de vol controle
            if (isFlying)
            {
                flyT += Time.fixedDeltaTime / travelTime;
                float t = Mathf.Clamp01(flyT);

                Vector3 basePos = Vector3.Lerp(startPos, frozenTargetPos, t);
                Vector3 finalPos = basePos;

                if (useArc)
                {
                    float height = Mathf.Sin(t * Mathf.PI) * arcHeight;
                    finalPos += Vector3.up * height;
                }

                rb.MovePosition(finalPos);

                if (t >= 1f)
                {
                    EndFlying();
                }

                return;
            }

            // Detection du geste de remote grab
            if (isSelected && firstInteractorSelecting is XRRayInteractor && canJump && ray != null)
            {
                float dt = Time.fixedDeltaTime;
                Vector3 currentRayPos = ray.transform.position;

                if (!previousRayPosInitialized)
                {
                    previousRayPos = currentRayPos;
                    previousRayPosInitialized = true;
                    return;
                }

                Vector3 velocity = (currentRayPos - previousRayPos) / dt;
                previousRayPos = currentRayPos;

                if (velocity.magnitude > velocityThreshold)
                {
                    StartFlying();
                }
            }
        }

        private void StartFlying()
        {
            Transform objAttach = GetObjectAttach();
            Transform handAttach = GetHandAttach();

            if (handAttach == null)
            {
                // Debug.Log("XRGrabRemote: no hand attach found, abort remote grab.");
                return;
            }

            // Force explicitely the ray to release this object
            var selectInteractor = ray as IXRSelectInteractor;
            var selectInteractable = this as IXRSelectInteractable;

            if (selectInteractor != null && selectInteractable != null)
            {
                ray.interactionManager.SelectExit(selectInteractor, selectInteractable);
            }


            // On decroche l'objet du ray
            Drop();

            // On freeze depart et destination
            startPos = objAttach.position;
            frozenTargetPos = handAttach.position;

            flyT = 0f;
            isFlying = true;
            canJump = false;

            rb.isKinematic = true;
        }

        private void EndFlying()
        {

            isFlying = false;
            rb.isKinematic = false;

            // Snap final sur la cible gele
            rb.MovePosition(frozenTargetPos);

            // Tentative d'autograb
            if (isAutoGrabbed && grabInteractor != null && !isSelected)
            {
                Transform handAttach = GetHandAttach();
                Vector3 handPos = handAttach != null ? handAttach.position : grabInteractor.transform.position;

                float distance = Vector3.Distance(handPos, GetObjectAttach().position);

                // Debug.Log($"XRGrabRemote: end flying, distance to hand = {distance}");

                if (distance <= autoGrabDistance)
                {
                    // On recupere un manager valide
                    XRInteractionManager manager = interactionManager;
                    if (manager == null && grabInteractor != null)
                        manager = grabInteractor.interactionManager;

                    if (manager != null)
                    {
                        var selectInteractor = grabInteractor as IXRSelectInteractor;
                        var selectInteractable = this as IXRSelectInteractable;

                        if (selectInteractor != null && selectInteractable != null)
                        {
                            manager.SelectEnter(selectInteractor, selectInteractable);
                        }
                    }
                }
            }
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (args.interactorObject is XRRayInteractor xrRay)
            {
                // Remote grab mode
                trackPosition = false;
                trackRotation = false;
                throwOnDetach = false;

                ray = xrRay;
                previousRayPosInitialized = false;

                // La main directe est dans le parent du ray
                grabInteractor = ray.GetComponentInParent<XRDirectInteractor>();
                grabSelectInteractor = grabInteractor as IXRSelectInteractor;

                canJump = true;
                isFlying = false;
            }
            else
            {
                // Grab direct classique
                trackPosition = true;
                trackRotation = true;
                throwOnDetach = true;

                ray = null;
                grabInteractor = null;
                grabSelectInteractor = null;
                canJump = false;
                isFlying = false;
            }

            base.OnSelectEntered(args);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            isFlying = false;
            canJump = false;
            previousRayPosInitialized = false;
            rb.isKinematic = false;
        }
    }
}
