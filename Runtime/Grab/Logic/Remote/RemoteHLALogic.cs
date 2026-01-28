using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


namespace Louis.XR.Interactions.Grab.Logic.Remote
{
    [Serializable]
    public class RemoteHLALogic : XRRemoteLogicBase
    {
        [Header("Remote trigger")]
        public float velocityThreshold = 2f;

        [Header("Travel")]
        public float travelTime = 0.35f;

        [Header("Auto grab")]
        public bool isAutoGrabbed = true;
        public float autoGrabDistance = 0.2f;

        [Header("Trajectory")]
        public bool useArc = true;
        public float arcHeight = 0.2f;

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
        private Rigidbody rb;

        private XRRayInteractor ray;
        private XRDirectInteractor directInteractor;
        private IXRSelectInteractor directSelect;

        private Vector3 previousRayPos;
        private bool previousRayPosInitialized = false;

        private bool canJump = false;
        private bool isFlying = false;
        private float flyT = 0f;
        private Vector3 startPos;
        private Vector3 frozenTargetPos;

        public override bool OnSelectEntering(XRContext ctx)
        {
            return true;
        }

        public override void OnSelectEntered(XRContext ctx)
        {
            grab = ctx.interactable;
            if (grab == null)
                return;

            if (rb == null)
                rb = grab.GetComponent<Rigidbody>();

            if (ctx.interactor is XRRayInteractor xrRay)
            {
                ray = xrRay;
                directInteractor = ray.GetComponentInParent<XRDirectInteractor>();
                directSelect = directInteractor;

                grab.trackPosition = false;
                grab.trackRotation = false;
                grab.throwOnDetach = false;

                previousRayPosInitialized = false;
                canJump = true;
                isFlying = false;
            }
            else
            {
                ray = null;
                directInteractor = null;
                directSelect = null;

                grab.trackPosition = true;
                grab.trackRotation = true;
                grab.throwOnDetach = true;

                canJump = false;
                isFlying = false;
            }
        }

        public override void OnSelectExited(XRContext ctx)
        {
            isFlying = false;
            canJump = false;
            previousRayPosInitialized = false;

            if (rb != null)
                rb.isKinematic = false;
        }

        public override void OnFixedUpdate(XRContext ctx)
        {

            if (grab == null)
                grab = ctx.interactable;
            if (grab == null)
                return;

            if (rb == null)
                rb = grab.GetComponent<Rigidbody>();

            if (isFlying)
            {
                FlyStep(ctx);
                return;
            }

            if (!ctx.isRemote)
                return;

            if (!ctx.isHeld)
                return;

            if (ray == null || !canJump)
                return;

            Vector3 currentRayPos = ray.transform.position;
            float dt = ctx.deltaTime;

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
                StartFlying(ctx);
            }
        }

        private Transform GetObjAttach()
        {
            return grab.attachTransform != null ? grab.attachTransform : grab.transform;
        }

        private Transform GetHandAttach()
        {
            if (directInteractor == null)
                return null;

            return directInteractor.attachTransform != null
                ? directInteractor.attachTransform
                : directInteractor.transform;
        }

        private void StartFlying(XRContext ctx)
        {
            Transform objAttach = GetObjAttach();
            Transform handAttach = GetHandAttach();
            if (objAttach == null || handAttach == null)
                return;

            grab.trackPosition = false;
            grab.trackRotation = false;
            grab.throwOnDetach = false;

            startPos = objAttach.position;
            frozenTargetPos = handAttach.position;

            flyT = 0f;
            isFlying = true;
            canJump = false;

            if (rb != null)
                rb.isKinematic = true;
        }

        private void FlyStep(XRContext ctx)
        {
            float dt = ctx.deltaTime;

            flyT += dt / travelTime;
            float t = Mathf.Clamp01(flyT);

            Vector3 pos = Vector3.Lerp(startPos, frozenTargetPos, t);

            if (useArc)
            {
                float h = Mathf.Sin(t * Mathf.PI) * arcHeight;
                pos += Vector3.up * h;
            }

            rb.MovePosition(pos);

            if (t >= 1f)
                EndFlying(ctx);
        }

        private void EndFlying(XRContext ctx)
        {
            isFlying = false;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.MovePosition(frozenTargetPos);
            }

            if (grab != null)
            {
                grab.trackPosition = true;
                grab.trackRotation = true;
                grab.throwOnDetach = true;
            }

            if (!isAutoGrabbed || directInteractor == null)
                return;

            float dist = Vector3.Distance(
                GetHandAttach().position,
                GetObjAttach().position);

            if (dist > autoGrabDistance)
                return;

            var manager = ctx.manager ?? directInteractor.interactionManager;
            var selectInteractable = grab as IXRSelectInteractable;
            var directSel = directInteractor as IXRSelectInteractor;

            if (manager == null || selectInteractable == null || directSel == null)
                return;

            if (ray != null)
            {
                var raySel = ray as IXRSelectInteractor;
                if (raySel != null && grab.isSelected)
                    manager.SelectExit(raySel, selectInteractable);
            }

            manager.SelectEnter(directSel, selectInteractable);
        }
    }
}
