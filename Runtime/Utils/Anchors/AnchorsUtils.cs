using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Louis.XR.Interactions.Utils.Anchors
{

    public static class AnchorsUtils
    {
        public static XRAnchor SelectBestAnchor(
        List<XRAnchor> anchors,
        HandUsage hand,
        Vector3 interactorPos,
        Quaternion interactorRot,
        float maxSnapDistance = 0, bool useAngle = false, float maxAngle = 0f, float angleWeight = 0f)
        {
            List<XRAnchor> candidates = new List<XRAnchor>();

            foreach (var anchor in anchors)
            {
                if (anchor == null)
                    continue;

                if (anchor.role != AnchorRole.Any && anchor.role != AnchorRole.Grab)
                    continue;

                if (anchor.handside == HandUsage.Both || anchor.handside == hand)
                    candidates.Add(anchor);
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"[AnchorsUtils] Aucun anchor de grab trouv� pour la main {hand}. Aucun anchor s�lectionn�.");
                return null;
            }

            XRAnchor best = null;
            float bestScore = float.MaxValue;

            foreach (var anchor in candidates)
            {
                if (anchor == null)
                    continue;

                var at = anchor.transform;

                
                float dist = Vector3.Distance(at.position, interactorPos);
                if (dist > maxSnapDistance)
                    continue; 

                float distSqr = dist * dist;
                float priority = Mathf.Max(anchor.priority, 1f);

                
                float score = distSqr;

                if (useAngle)
                {
                    
                    float angle = Quaternion.Angle(at.rotation, interactorRot);
                    if (angle > maxAngle)
                        continue; 

                    float normAngle = angle / maxAngle;                  
                    score += normAngle * angleWeight;                    
                }

                score /= priority;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = anchor;
                }
            }

            return best;
        }

        public static XRAnchor SelectBestAnchorInventory(List<XRAnchor> anchors)
        {
            XRAnchor best = null;
            float bestPrio = float.MinValue;

            foreach (var a in anchors)
            {
                if (a == null) continue;

                if (a.role != AnchorRole.Inventory && a.role != AnchorRole.Any)
                    continue;

                if (a.priority > bestPrio)
                {
                    bestPrio = a.priority;
                    best = a;
                }
            }

            Debug.Log($"[AnchorsUtils] Selected inventory anchor: {(best != null ? best.name : "none")} with priority {bestPrio}");

            return best;
        }

        /// <summary>
        /// Sélectionne l'anchor le plus proche sans aucun critère de filtrage.
        /// Utilisé comme fallback quand SelectBestAnchor retourne null.
        /// </summary>
        public static XRAnchor SelectClosestAnchor(
            List<XRAnchor> anchors,
            Vector3 interactorPos)
        {
            if (anchors == null || anchors.Count == 0)
            {
                Debug.LogError($"[AnchorsUtils] Aucun anchor disponible pour SelectClosestAnchor.");
                return null;
            }

            XRAnchor closest = null;
            float closestDistSqr = float.MaxValue;

            foreach (var anchor in anchors)
            {
                if (anchor == null)
                    continue;

                float distSqr = Vector3.SqrMagnitude(anchor.transform.position - interactorPos);

                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    closest = anchor;
                }
            }

            if (closest != null)
            {
                Debug.LogWarning($"[AnchorsUtils] Fallback: anchor le plus proche sélectionné: {closest.name} (distance: {Mathf.Sqrt(closestDistSqr):F3}m)");
            }

            return closest;
        }
    }
}
