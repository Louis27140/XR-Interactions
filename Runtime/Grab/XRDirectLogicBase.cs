using Louis.XR.Interactions.Grab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable;

namespace Louis.XR.Interactions.Grab
{
    [Serializable]
    public class XRDirectLogicBase
    {
        public MovementType movementOverride = MovementType.Instantaneous;
        public virtual bool CanSelect(XRContext ctx) => true;
        public virtual bool OnSelectEntering(XRContext ctx)
        {
            if (ctx?.interactable != null)
                ctx.interactable.movementType = movementOverride;

            return true;
        }
        public virtual void OnSelectEntered(XRContext ctx) { }
        public virtual void Process(XRContext ctx) { }
        public virtual void OnSelectExiting(XRContext ctx) { }
        public virtual void OnSelectExited(XRContext ctx) { }
        public virtual void OnFixedUpdate(XRContext ctx) { }
    }
}
