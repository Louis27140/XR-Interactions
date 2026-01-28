using System;

namespace Louis.XR.Interactions.Grab.Logic
{
    [Serializable]
    public class XRRemoteLogicBase
    {
        public virtual bool CanSelect(XRContext ctx) => true;
        public virtual bool OnSelectEntering(XRContext ctx) => true;
        public virtual void OnSelectEntered(XRContext ctx) { }
        public virtual void Process(XRContext ctx) { }
        public virtual void OnSelectExiting(XRContext ctx) { }
        public virtual void OnSelectExited(XRContext ctx) { }
        public virtual void OnFixedUpdate(XRContext ctx) { }
    }
}