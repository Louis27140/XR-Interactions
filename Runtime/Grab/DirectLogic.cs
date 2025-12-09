using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Louis.XR.Interactions.Grab {

    [Serializable]
    public class DirectLogic : XRDirectLogicBase
    {
        public override void OnSelectEntered(XRContext ctx)
        {
            // ne touche à RIEN
            // XRGrabInteractable gère déjà l'attach, la pose, etc.
        }

        public override void Process(XRContext ctx)
        {
            // aucun override
        }

        public override void OnSelectExited(XRContext ctx)
        {
            base.OnSelectExited(ctx);
        }

        public override bool OnSelectEntering(XRContext ctx)
        {
            base.OnSelectEntering(ctx);
            return true;
        }

        public override void OnSelectExiting(XRContext ctx)
        {
        }

        public override void OnFixedUpdate(XRContext ctx)
        {
            base.OnFixedUpdate(ctx);
        }
    }
}
