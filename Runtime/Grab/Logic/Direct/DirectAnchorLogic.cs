using System;
using System.Collections.Generic;
using Louis.XR.Interactions.Utils.Anchors;
using UnityEngine;


namespace Louis.XR.Interactions.Grab.Logic.Direct
{
    [Serializable]
    public class DirectAnchorLogic : XRDirectLogicBase
    {
    [SerializeField] private float maxSnapDistance = 0.12f;

    [SerializeField] private bool useAngle = false;

    [SerializeField] private float maxAngle = 60f;
    [SerializeField] private float angleWeight = 0.5f;   

    public override bool CanSelect(XRContext ctx)
    {
        if (ctx.anchors == null || ctx.anchors.Count == 0)
            return false;

        return true;
    }

    public override bool OnSelectEntering(XRContext ctx)
    {
        XRAnchor bestAnchor = AnchorsUtils.SelectBestAnchor(
            ctx.anchors, ctx.hand,
            ctx.interactorPosition,
            ctx.interactorRotation,
            maxSnapDistance,
            useAngle,
            maxAngle,
            angleWeight
            );

        // Fallback: si aucun anchor ne correspond aux critères, prendre le plus proche
        if (bestAnchor == null)
        {
            bestAnchor = AnchorsUtils.SelectClosestAnchor(ctx.anchors, ctx.interactorPosition);
        }

        if (bestAnchor == null)
            return false;

        ctx.interactable.attachTransform = bestAnchor.transform;

        return true;
    }

    public override void OnSelectEntered(XRContext ctx)
    {
        base.OnSelectEntered(ctx);
    }

        public override void OnSelectExited(XRContext ctx)
        {
            base.OnSelectExited(ctx);
        }
    }
}
