using Louis.XR.Interactions.Grab.Logic;
using Louis.XR.Interactions.Grab.Logic.Direct;
using Louis.XR.Interactions.Grab.Logic.Remote;
using Louis.XR.Interactions.Grab.Logic.Socket;
using Louis.XR.Interactions.Input;
using Louis.XR.Interactions.Input.Feedback;
using Louis.XR.Interactions.Utils.Anchors;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Louis.XR.Interactions.Grab
{
    [DisallowMultipleComponent]
    public class XRGenericInteractable : XRGrabInteractable
    {
        [Header("Grab Logic")]
        [SerializeReference]
        public XRDirectLogicBase directLogic;

        [SerializeReference]
        public XRRemoteLogicBase remoteLogic;

        [SerializeReference]
        public XRSocketLogicBase socketLogic;

        private readonly List<XRAnchor> anchors = new List<XRAnchor>();

        [SerializeField] private bool useHandle = false;

        public bool UseHandle => useHandle;

        [SerializeField]
        [Tooltip("Allows or blocks grab (direct and remote).")]
        private bool allowGrab = true;

        public bool AllowGrab
        {
            get => allowGrab;
            set => allowGrab = value;
        }

        [Header("Haptics Feedback")]
        [SerializeField] private XRHapticPreset onGrabPreset;
        [SerializeField] private XRHapticPreset onReleasePreset;
        [SerializeField] private XRHapticPreset onHoverPreset;

        protected override void Awake()
        {
            base.Awake();

            if (attachTransform == null)
                attachTransform = transform;

            anchors.Clear();
            anchors.AddRange(GetComponentsInChildren<XRAnchor>(true));

            // Enable multi-selection mode if using TwoHandDirectLogic
            if (directLogic is TwoHandDirectLogic)
            {
                selectMode = InteractableSelectMode.Multiple;
            }
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (rb.mass <= 0)
                {
                    rb.mass = 1f;
                }

                var colliders = GetComponentsInChildren<Collider>();
                if (colliders.Length == 0)
                {
                    Debug.LogError($"[XRGenericInteractable] {name} n'a aucun Collider !", this);
                }
            }
        }

        private void FixedUpdate()
        {
            if (!isSelected)
                return;

            if (firstInteractorSelecting is IXRSelectInteractor interactor)
            {
                var ctx = BuildContext(interactor);
                ctx.deltaTime = Time.fixedDeltaTime;

                if (interactor is XRRayInteractor)
                {
                    ctx.isRemote = true;
                    remoteLogic?.OnFixedUpdate(ctx);
                }
            }
        }

        public override bool IsHoverableBy(IXRHoverInteractor interactor)
        {
            if (!base.IsHoverableBy(interactor))
                return false;

            bool isRay = interactor is XRRayInteractor;

            if (!allowGrab && !(interactor is XRSocketInteractor))
            {
                return false;
            }

            if (isRay)
            {
                return remoteLogic != null;
            }

            return true;
        }

        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            if (!base.IsSelectableBy(interactor))
            {
                return false;
            }

            bool isRay = interactor is XRRayInteractor;
            bool isSocket = interactor is XRSocketInteractor;

            if (isSocket)
            {
                return socketLogic != null;
            }

            if (!allowGrab)
            {
                return false;
            }

            if (!isRay && directLogic != null)
            {
                var ctx = BuildContext(interactor);
                ctx.isRemote = false;
                ctx.isHeld = isSelected;
                return directLogic.CanSelect(ctx);
            }

            return true;
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            var interactor = args.interactorObject as IXRSelectInteractor;

            if (interactor != null)
            {
                bool isRay = interactor is XRRayInteractor;
                bool isSocket  = interactor is XRSocketInteractor;
                var ctx = BuildContext(interactor);
                ctx.isRemote = isRay;

                if (isRay)
                    remoteLogic?.OnSelectEntering(ctx);
                else if (isSocket)
                    socketLogic?.OnSelectEntering(ctx);
                else
                {
                    movementType = directLogic.movementOverride;

                    directLogic?.OnSelectEntering(ctx);
                }
            }

            base.OnSelectEntering(args);


        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {

            var interactor = args.interactorObject as IXRSelectInteractor;
            if (interactor == null)
                return;

            bool isRay = interactor is XRRayInteractor;
            bool isSocket = interactor is XRSocketInteractor;
            var ctx = BuildContext(interactor);
            ctx.isRemote = isRay;

            if (isRay)
                remoteLogic?.OnSelectEntered(ctx);
            else if (isSocket)
                socketLogic?.OnSelectEntered(ctx);
            else
                directLogic?.OnSelectEntered(ctx);


            base.OnSelectEntered(args);

            // Detect second hand grab for two-hand mode
            if (directLogic is TwoHandDirectLogic twoHandLogic && interactorsSelecting.Count >= 2)
            {
                var multiCtx = BuildMultiContext();
                if (multiCtx != null)
                {
                    twoHandLogic.OnSecondHandGrabbed(multiCtx);
                }
            }

            XRHandSide hand = DetermineHand(args.interactorObject);

            // Jouer feedback haptique
            if (onGrabPreset != null)
            {
                XRHaptic.Instance?.PlayPreset(onGrabPreset, hand);
            } else
            {
                XRHaptic.Instance?.PlayGrabFeedback(hand);
            }


        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (!isSelected)
                return;

            // Check for two-hand grab mode
            if (directLogic is TwoHandDirectLogic twoHandLogic && interactorsSelecting.Count >= 2)
            {
                var multiCtx = BuildMultiContext();
                if (multiCtx != null)
                {
                    twoHandLogic.ProcessMulti(multiCtx);
                    return;  // Skip single-hand logic
                }
            }

            // Single-hand logic (existing code)
            if (firstInteractorSelecting is not IXRSelectInteractor interactor)
                return;

            bool isRay = interactor is XRRayInteractor;
            bool isSocket = interactor is XRSocketInteractor;
            var ctx = BuildContext(interactor);
            ctx.isRemote = isRay;

            if (isRay)
                remoteLogic?.Process(ctx);
            else if (isSocket)
                socketLogic?.Process(ctx);
            else
                directLogic?.Process(ctx);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            var interactor = args.interactorObject as IXRSelectInteractor;

            // Detect second hand release for two-hand mode (before base.OnSelectExited)
            if (directLogic is TwoHandDirectLogic twoHandLogic && interactor != null)
            {
                // Check if it's the second hand being released (not the primary)
                if (interactorsSelecting.Count >= 2 && interactorsSelecting[1] == interactor)
                {
                    twoHandLogic.OnSecondHandReleased();
                }
            }

            if (interactor != null)
            {
                bool isRay = interactor is XRRayInteractor;
                bool isSocket = interactor is XRSocketInteractor;

                var ctx = BuildContext(interactor);
                ctx.isRemote = isRay;

                if (isRay)
                    remoteLogic?.OnSelectExited(ctx);
                else if (isSocket)
                    socketLogic?.OnSelectExited(ctx);
                else
                    directLogic?.OnSelectExited(ctx);
            }

            base.OnSelectExited(args);


            XRHandSide hand = DetermineHand(args.interactorObject);
            if (onReleasePreset != null)
            {
                XRHaptic.Instance?.PlayPreset(onReleasePreset, hand);
            }
        }

        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);

            XRHandSide hand = DetermineHand(args.interactorObject);
            if (onHoverPreset != null)
            {
                XRHaptic.Instance?.PlayPreset(onHoverPreset, hand);
            }
        }

        private XRContext BuildContext(IXRSelectInteractor interactor)
        {
            var comp = (Component)interactor;
            bool isRay = interactor is XRRayInteractor;

            var ctx = new XRContext
            {
                interactor = interactor,
                interactable = this,
                hand = GetHandUsageFromInteractor(interactor),

                interactorPosition = comp.transform.position,
                interactorRotation = comp.transform.rotation,

                attachTransform = attachTransform,
                anchors = anchors,

                deltaTime = Time.deltaTime,
                isRemote = isRay,
                isHeld = isSelected,
                manager = interactionManager
            };

            return ctx;
        }

        private HandUsage GetHandUsageFromInteractor(IXRSelectInteractor interactor)
        {
            // Utiliser la propriété Handedness native de XRBaseInteractable
            var baseInteractor = interactor as XRBaseInteractor;
            if (baseInteractor != null)
            {
                switch (baseInteractor.handedness)
                {
                    case InteractorHandedness.Left:
                        return HandUsage.Left;
                    case InteractorHandedness.Right:
                        return HandUsage.Right;
                    case InteractorHandedness.None:
                    default:
                        return HandUsage.Both;
                }
            }

            // Fallback: parser le nom si Handedness n'est pas disponible
            string name = interactor.transform.name.ToLower();
            if (name.Contains("left"))
                return HandUsage.Left;
            if (name.Contains("right"))
                return HandUsage.Right;

            return HandUsage.Both;
        }

        private XRHandSide DetermineHand(IXRInteractor interactor)
        {
            // Utiliser Handedness si disponible
            var baseInteractor = interactor as XRBaseInteractor;
            if (baseInteractor != null)
            {
                return baseInteractor.handedness == InteractorHandedness.Left ? XRHandSide.Left : XRHandSide.Right;
            }

            // Fallback: parser le nom
            string name = interactor.transform.name.ToLower();
            return name.Contains("left") ? XRHandSide.Left : XRHandSide.Right;
        }

        /// <summary>
        /// Builds a multi-interactor context for two-hand grab interactions.
        /// Returns null if less than 2 interactors are selecting.
        /// </summary>
        private XRMultiContext BuildMultiContext()
        {
            if (interactorsSelecting.Count < 2)
                return null;

            var primary = interactorsSelecting[0];
            var secondary = interactorsSelecting[1];

            var primaryComp = (Component)primary;
            var secondaryComp = (Component)secondary;

            var ctx = new XRMultiContext
            {
                // Fill base XRContext fields from primary interactor
                interactor = primary,
                interactable = this,
                hand = GetHandUsageFromInteractor(primary),
                interactorPosition = primaryComp.transform.position,
                interactorRotation = primaryComp.transform.rotation,
                attachTransform = attachTransform,
                anchors = anchors,
                deltaTime = Time.deltaTime,
                isRemote = false,
                isHeld = isSelected,
                manager = interactionManager,

                // Fill XRMultiContext fields for secondary interactor
                allInteractors = new List<IXRSelectInteractor>(interactorsSelecting),
                secondaryInteractor = secondary,
                secondaryInteractorPosition = secondaryComp.transform.position,
                secondaryInteractorRotation = secondaryComp.transform.rotation,
                secondaryHand = GetHandUsageFromInteractor(secondary)
            };

            // Calculate two-hand specific data
            ctx.centerPosition = (ctx.interactorPosition + ctx.secondaryInteractorPosition) * 0.5f;
            ctx.handsDirection = (ctx.secondaryInteractorPosition - ctx.interactorPosition).normalized;
            ctx.handsDistance = Vector3.Distance(ctx.interactorPosition, ctx.secondaryInteractorPosition);

            return ctx;
        }
    }
}
