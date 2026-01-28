
using System;
using static UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable;

namespace Louis.XR.Interactions.Grab.Logic
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
