using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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

        // Tous les XRAnchor trouvés sous cet objet
        private readonly List<XRAnchor> anchors = new List<XRAnchor>();

        [SerializeField] private bool useHandle = false;

        public bool UseHandle => useHandle;

        protected override void Awake()
        {
            base.Awake();

            // Sécurise un attachTransform valide
            if (attachTransform == null)
                attachTransform = transform;

            // Récupère les anchors enfants (si tu gardes XRAnchor en Mono)
            anchors.Clear();
            anchors.AddRange(GetComponentsInChildren<XRAnchor>(true));
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

            if (isRay)
            {
                return remoteLogic != null;
            }

            return true;
        }

        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            if (!base.IsSelectableBy(interactor))
                return false;

            bool isRay = interactor is XRRayInteractor;

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
                var ctx = BuildContext(interactor);
                ctx.isRemote = isRay;

                if (isRay)
                    remoteLogic?.OnSelectEntering(ctx);
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
            var ctx = BuildContext(interactor);
            ctx.isRemote = isRay;

            if (isRay)
                remoteLogic?.OnSelectEntered(ctx);
            else
                directLogic?.OnSelectEntered(ctx);


            base.OnSelectEntered(args);


        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase); // d’abord

            if (!isSelected)
                return;

            if (firstInteractorSelecting is not IXRSelectInteractor interactor)
                return;

            bool isRay = interactor is XRRayInteractor;
            var ctx = BuildContext(interactor);
            ctx.isRemote = isRay;

            if (isRay)
                remoteLogic?.Process(ctx);
            else
                directLogic?.Process(ctx);

            


        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            var interactor = args.interactorObject as IXRSelectInteractor;

            if (interactor != null)
            {
                bool isRay = interactor is XRRayInteractor;
                var ctx = BuildContext(interactor);
                ctx.isRemote = isRay;

                if (isRay)
                    remoteLogic?.OnSelectExited(ctx);
                else
                    directLogic?.OnSelectExited(ctx);
            }

            base.OnSelectExited(args);
        }

        private XRContext BuildContext(IXRSelectInteractor interactor)
        {
            var comp = (Component)interactor;

            var ctx = new XRContext
            {
                interactor = interactor,
                interactable = this,
                hand = GetHandUsageByName(comp.transform),

                interactorPosition = comp.transform.position,
                interactorRotation = comp.transform.rotation,

                attachTransform = attachTransform,
                anchors = anchors,

                deltaTime = Time.deltaTime,
                isRemote = interactor is XRRayInteractor,
                isHeld = isSelected,
                manager = interactionManager
            };

            return ctx;
        }

        private HandUsage GetHandUsageByName(Transform t)
        {
            var name = t.name;

            if (name.Contains("Left"))
                return HandUsage.Left;

            if (name.Contains("Right"))
                return HandUsage.Right;

            return HandUsage.Both;
        }
    }
}
