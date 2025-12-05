using Codice.Client.Common.GameUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Louis.XR.Interactions.Grab
{
    public class XROrientedGrabInteractable : XRGrabInteractable
    {
        private Vector3 attachLocalPos;
        private Quaternion attachLocalRot;

        protected override void Awake()
        {
            base.Awake();

            if (attachTransform == null)
                attachTransform = attachTransform != null ? attachTransform : this.attachTransform;

            // on stocke la pose locale de l'attach par rapport à la lampe
            attachLocalPos = attachTransform.localPosition;
            attachLocalRot = attachTransform.localRotation;

            // on laisse XR gérer la position mais PAS la rotation
            trackPosition = true;
            trackRotation = false;
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (!isSelected)
                return;

            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                return;

            var interactor = firstInteractorSelecting;
            if (interactor == null)
                return;

            var handAttach = interactor.GetAttachTransform(this);

            // position de la main
            Vector3 handPos = handAttach.position;
            Quaternion handRot = handAttach.rotation;

            // === 1) On enlève le ROLL de la main ===
            // direction "avant" de la main
            Vector3 fwd = handRot * Vector3.forward;

            // on veut que la lampe reste "verticale" -> on force l'up sur Vector3.up
            Vector3 up = Vector3.up;

            // on projette le forward sur le plan horizontal pour virer le roll
            Vector3 fwdOnPlane = Vector3.ProjectOnPlane(fwd, up);
            if (fwdOnPlane.sqrMagnitude < 0.0001f)
            {
                // cas pathologique : on garde l'ancienne direction
                fwdOnPlane = transform.forward;
            }

            Quaternion handNoRoll = Quaternion.LookRotation(fwdOnPlane.normalized, up);

            // === 2) On reconstruit la pose de la lampe à partir de ce handNoRoll ===

            // rotation de la lampe = rotation de la main (sans roll) * inverse de la rotation locale du point d'attach
            Quaternion lampRot = handNoRoll * Quaternion.Inverse(attachLocalRot);

            // position de la lampe = position de la main - rotationLamp * offsetLocal
            Vector3 lampPos = handPos - (lampRot * attachLocalPos);

            transform.SetPositionAndRotation(lampPos, lampRot);
        }

    }
        
}
