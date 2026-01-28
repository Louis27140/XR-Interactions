using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Louis.XR.Interactions.Grab.Logic.Direct
{
    [Serializable]
    public class DirectLogic : XRDirectLogicBase
    {
        public override void OnSelectEntered(XRContext ctx)
        {
            base.OnSelectEntered(ctx);
        }

        public override void Process(XRContext ctx)
        {
            base.Process(ctx);
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
