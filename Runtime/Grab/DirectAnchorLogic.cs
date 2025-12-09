using Louis.XR.Interactions.Grab;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DirectAnchorLogic : XRDirectLogicBase
{
    public float maxSnapDistance = 0.12f; // 12 cm

    public bool useAngle = false;         // angle optional dans le score

    public float maxAngle = 60f;          // angle max en degrés
    public float angleWeight = 0.5f;      // poids de l'angle dans le score

    public override bool CanSelect(XRContext ctx)
    {
        if (ctx.anchors == null || ctx.anchors.Count == 0)
            return false;

        return true;
    }

    public override bool OnSelectEntering(XRContext ctx)
    {
        // ici on part du principe que CanSelect a déjà validé
        XRAnchor bestAnchor = SelectBestAnchor(
            ctx.anchors, ctx.hand,
            ctx.interactorPosition,
            ctx.interactorRotation);

        if (bestAnchor == null)
            return false;

        ctx.interactable.attachTransform = bestAnchor.transform;

        return true;
    }

    public override void OnSelectEntered(XRContext ctx)
    {
        base.OnSelectEntered(ctx);
    }

    private XRAnchor SelectBestAnchor(
        List<XRAnchor> anchors,
        HandUsage hand,
        Vector3 interactorPos,
        Quaternion interactorRot)
    {
        List<XRAnchor> candidates = new List<XRAnchor>();

        foreach (var anchor in anchors)
        {
            if (anchor == null)
                continue;

            if (anchor.handside == HandUsage.Both || anchor.handside == hand)
                candidates.Add(anchor);
        }

        if (candidates.Count == 0)
            candidates.AddRange(anchors);

        XRAnchor best = null;
        float bestScore = float.MaxValue;

        foreach (var anchor in candidates)
        {
            if (anchor == null)
                continue;

            var at = anchor.transform;

            // distance
            float dist = Vector3.Distance(at.position, interactorPos);
            if (dist > maxSnapDistance)
                continue; // trop loin pas de snap

            float distSqr = dist * dist;
            float priority = Mathf.Max(anchor.priority, 1f);

            // score de base sur la distance
            float score = distSqr;

            if (useAngle)
            {
                // angle : on compare rotations globales pour l'instant
                float angle = Quaternion.Angle(at.rotation, interactorRot);
                if (angle > maxAngle)
                    continue; // trop désaligné on skip

                // score combiné (angle optionnel)
                float normAngle = angle / maxAngle;                  // 0..1
                score += normAngle * angleWeight;                    // pondère un peu par l'angle
            }

            // appliquer la priorité (plus de priorité -> score plus faible)
            score /= priority;

            if (score < bestScore)
            {
                bestScore = score;
                best = anchor;
            }
        }

        return best;
    }
}
