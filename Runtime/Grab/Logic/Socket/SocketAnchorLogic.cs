using Louis.XR.Interactions.Utils.Anchors;
using UnityEngine;

namespace Louis.XR.Interactions.Grab.Logic.Socket
{
    [System.Serializable]
    public class SocketAnchorLogic : XRSocketLogicBase
    {

        public override bool OnSelectEntering(XRContext ctx)
        {
            XRAnchor inventoryAnchor = AnchorsUtils.SelectBestAnchorInventory(ctx.anchors);

            // Fallback: si aucun anchor d'inventaire, prendre le plus proche
            if (inventoryAnchor == null)
            {
                inventoryAnchor = AnchorsUtils.SelectClosestAnchor(ctx.anchors, ctx.interactorPosition);
            }

            if (inventoryAnchor != null)
            {
                ctx.interactable.attachTransform = inventoryAnchor.transform;
                return true;
            }

            Debug.LogWarning($"[SocketAnchorLogic] Aucun anchor disponible sur {ctx.interactable.name}", ctx.interactable as Object);
            return false;
        }

        public override void OnSelectEntered(XRContext ctx)
        {
            base.OnSelectEntered(ctx);
        }

        public override void OnSelectExiting(XRContext ctx)
        {
            base.OnSelectExiting(ctx);
        }

        public override void OnSelectExited(XRContext ctx)
        {
            XRAnchor defaultAnchor = AnchorsUtils.SelectBestAnchor(ctx.anchors, ctx.hand, ctx.interactorPosition, ctx.interactorRotation);

            // Fallback: si aucun anchor par défaut, prendre le plus proche
            if (defaultAnchor == null)
            {
                defaultAnchor = AnchorsUtils.SelectClosestAnchor(ctx.anchors, ctx.interactorPosition);
            }

            if (defaultAnchor != null)
            {
                ctx.interactable.attachTransform = defaultAnchor.transform;
            }
            else
            {
                Debug.LogWarning($"[SocketAnchorLogic] Aucun anchor disponible sur {ctx.interactable.name}", ctx.interactable as Object);
                ctx.interactable.attachTransform = null;
            }
        }
    }
}