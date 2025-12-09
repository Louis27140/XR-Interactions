using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Louis.XR.Interactions.Grab
{
    [Serializable]
    public class RemoteInstantLogic : XRRemoteLogicBase
    {
        public override bool CanSelect(XRContext ctx)
        {
            return true;
        }

        public override void OnSelectEntered(XRContext ctx)
        {
            // Ici, l'objet vient d'être sélectionné par le RAY
            var manager = ctx.manager;
            if (manager == null)
                return;

            if (ctx.interactor is not IXRSelectInteractor rayInteractor)
                return;

            // On cherche le XRDirectInteractor correspondant à la main
            var directInteractor = rayInteractor.transform.GetComponentInParent<XRDirectInteractor>();
            if (directInteractor == null)
            {
                // Pas de direct interactor trouvé -> on laisse le grab remote normal
                return;
            }

            ctx.interactable.transform.SetPositionAndRotation(
                directInteractor.transform.position,
                directInteractor.transform.rotation);

            manager.SelectExit(rayInteractor, ctx.interactable);

            manager.SelectEnter((IXRSelectInteractor)directInteractor, ctx.interactable);
        }

        public override void Process(XRContext ctx)
        {
        }

        public override void OnSelectExited(XRContext ctx)
        {
        }
    }
}
