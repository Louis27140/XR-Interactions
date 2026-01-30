using System;
using Louis.XR.Interactions.Utils.Anchors;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Louis.XR.Interactions.Grab.Logic.Direct
{
    /// <summary>
    /// Two-hand grab logic that enables manipulating objects with both hands simultaneously.
    /// Supports position averaging, rotation alignment along hands axis, and distance-based scaling.
    /// Gracefully falls back to single-hand behavior when only one hand is grabbing.
    /// </summary>
    [Serializable]
    public class TwoHandDirectLogic : XRDirectLogicBase
    {
    [Header("Rotation")]
    [SerializeField]
    [Tooltip("Align object's forward axis to the vector between hands (Half-Life Alyx style)")]
    private bool alignToHandsAxis = true;

    [Header("Scaling")]
    [SerializeField]
    [Tooltip("Enable scaling based on the distance between hands")]
    private bool enableScaling = true;

    [SerializeField]
    [Tooltip("Minimum scale multiplier when hands are close together")]
    private float minScale = 0.5f;

    [SerializeField]
    [Tooltip("Maximum scale multiplier when hands are far apart")]
    private float maxScale = 2.0f;

    [Header("Anchor")]
    [SerializeField]
    [Tooltip("Use anchor snapping for the primary hand (first hand to grab)")]
    private bool usePrimaryHandAnchor = true;

    [SerializeField]
    [Tooltip("Maximum distance to snap to an anchor")]
    private float maxSnapDistance = 0.12f;

    // State tracking (stored per-instance)
    private float initialHandsDistance;
    private Vector3 initialScale;
    private Vector3 lastUp = Vector3.up;  // For rotation continuity
    private Transform primaryAnchor;
    private Transform secondaryAnchor;
    private bool isInTwoHandMode = false;

    public override bool CanSelect(XRContext ctx)
    {
        return true;
    }

    /// <summary>
    /// Called when the primary hand (first hand) grabs the object.
    /// Selects the best anchor if anchor snapping is enabled.
    /// </summary>
    public override bool OnSelectEntering(XRContext ctx)
    {
        base.OnSelectEntering(ctx);

        // Select anchor for primary hand if enabled
        if (usePrimaryHandAnchor && ctx.anchors != null && ctx.anchors.Count > 0)
        {
            var anchor = AnchorsUtils.SelectBestAnchor(
                ctx.anchors,
                ctx.hand,
                ctx.interactorPosition,
                ctx.interactorRotation,
                maxSnapDistance,
                false,  // Don't use angle filtering
                0f,
                0f
            );

            // Fallback: si aucun anchor ne correspond aux critères, prendre le plus proche
            if (anchor == null)
            {
                anchor = AnchorsUtils.SelectClosestAnchor(ctx.anchors, ctx.interactorPosition);
            }

            if (anchor != null)
            {
                primaryAnchor = anchor.transform;
                ctx.interactable.attachTransform = primaryAnchor;
            }
        }
        return true;
    }

    /// <summary>
    /// Called after the primary hand grab is confirmed.
    /// Stores initial scale for potential scaling operations.
    /// </summary>
    public override void OnSelectEntered(XRContext ctx)
    {
        base.OnSelectEntered(ctx);

        if (ctx.interactable != null)
        {
            initialScale = ctx.interactable.transform.localScale;
        }
    }

    /// <summary>
    /// Called when the second hand grabs the object.
    /// Activates two-hand mode and initializes two-hand state.
    /// Finds an anchor for the second hand with priority: unused anchor > closest > free grab.
    /// </summary>
    public void OnSecondHandGrabbed(XRMultiContext ctx)
    {
        isInTwoHandMode = true;
        initialHandsDistance = ctx.handsDistance;

        if (ctx.interactable != null)
        {
            lastUp = ctx.interactable.transform.up;
        }

        // Try to find an anchor for the second hand
        if (usePrimaryHandAnchor && ctx.anchors != null && ctx.anchors.Count > 0)
        {
            secondaryAnchor = SelectSecondaryAnchor(ctx);
        }
    }

    /// <summary>
    /// Selects the best anchor for the second hand.
    /// Priority: 1) Unused anchor (different from primary), 2) Closest to hand position, 3) null (free grab).
    /// </summary>
    private Transform SelectSecondaryAnchor(XRMultiContext ctx)
    {
        if (ctx.anchors == null || ctx.anchors.Count == 0)
            return null;

        XRAnchor bestUnusedAnchor = null;
        XRAnchor closestAnchor = null;
        float closestDistance = float.MaxValue;

        foreach (var anchor in ctx.anchors)
        {
            // Skip if same role not compatible
            if (anchor.role != AnchorRole.Grab && anchor.role != AnchorRole.Any)
                continue;

            // Check hand compatibility
            if (anchor.handside != HandUsage.None &&
                anchor.handside != HandUsage.Both &&
                anchor.handside != ctx.secondaryHand)
                continue;

            float distance = Vector3.Distance(anchor.transform.position, ctx.secondaryInteractorPosition);

            // Priority 1: Find unused anchor (not the primary anchor)
            if (primaryAnchor == null || anchor.transform != primaryAnchor)
            {
                if (distance <= maxSnapDistance)
                {
                    if (bestUnusedAnchor == null)
                    {
                        bestUnusedAnchor = anchor;
                        closestDistance = distance;
                    }
                    else if (distance < closestDistance)
                    {
                        bestUnusedAnchor = anchor;
                        closestDistance = distance;
                    }
                }
            }

            // Priority 2: Track closest anchor overall (even if it's primary, as fallback)
            if (distance < closestDistance)
            {
                closestAnchor = anchor;
                closestDistance = distance;
            }
        }

        // Return unused anchor if found, otherwise closest, otherwise null
        if (bestUnusedAnchor != null)
            return bestUnusedAnchor.transform;

        if (closestAnchor != null && closestDistance <= maxSnapDistance)
            return closestAnchor.transform;

        // Fallback: prendre l'anchor le plus proche sans restrictions
        var fallbackAnchor = AnchorsUtils.SelectClosestAnchor(ctx.anchors, ctx.secondaryInteractorPosition);
        if (fallbackAnchor != null)
            return fallbackAnchor.transform;

        return null;
    }

    /// <summary>
    /// Called when the second hand releases the object.
    /// Deactivates two-hand mode and returns to single-hand behavior.
    /// </summary>
    public void OnSecondHandReleased()
    {
        isInTwoHandMode = false;
        secondaryAnchor = null;
    }

    /// <summary>
    /// Process single-hand grab (fallback behavior).
    /// Uses default XR Toolkit behavior when only one hand is grabbing.
    /// </summary>
    public override void Process(XRContext ctx)
    {
        base.Process(ctx);
    }

    /// <summary>
    /// Process two-hand grab every frame.
    /// Calculates and applies position, rotation, and optional scaling based on both hands.
    /// </summary>
    public void ProcessMulti(XRMultiContext ctx)
    {
        if (!isInTwoHandMode || ctx.interactorCount < 2)
            return;

        var interactable = ctx.interactable;
        if (interactable == null)
            return;

        // Calculate target pose
        Vector3 targetPosition = CalculatePosition(ctx);
        Quaternion targetRotation = CalculateRotation(ctx);

        // Apply scaling if enabled
        if (enableScaling)
        {
            ApplyScaling(ctx);
        }

        // Apply transformation
        // Note: We modify transform directly after base.ProcessInteractable has run
        // This allows us to override the default grab behavior
        interactable.transform.position = targetPosition;
        interactable.transform.rotation = targetRotation;
    }

    /// <summary>
    /// Calculates the target position for the object based on both hands.
    /// Uses the center point between both hands.
    /// </summary>
    private Vector3 CalculatePosition(XRMultiContext ctx)
    {
        return ctx.centerPosition;
    }

    /// <summary>
    /// Calculates the target rotation for the object based on both hands.
    /// Uses "hammer handle" algorithm to align object along the axis between hands.
    /// </summary>
    private Quaternion CalculateRotation(XRMultiContext ctx)
    {
        if (alignToHandsAxis)
        {
            return CalculateHandsAxisRotation(ctx);
        }
        else
        {
            // Average rotation between both hands
            return Quaternion.Slerp(ctx.interactorRotation, ctx.secondaryInteractorRotation, 0.5f);
        }
    }

    /// <summary>
    /// "Hammer handle" rotation algorithm.
    /// Aligns the object's forward axis along the vector between the two hands.
    /// Maintains smooth rotation without flipping using continuity tracking.
    /// Inspired by Unity's XRDualGrabFreeTransformer.
    /// </summary>
    private Quaternion CalculateHandsAxisRotation(XRMultiContext ctx)
    {
        Vector3 forward = ctx.handsDirection;

        // Fallback if hands are too close together
        if (forward == Vector3.zero || ctx.handsDistance < 0.01f)
        {
            forward = ctx.interactorRotation * Vector3.forward;
        }

        // Calculate up vector from averaged hand rotations
        Vector3 primaryUp = ctx.interactorRotation * Vector3.up;
        Vector3 secondaryUp = ctx.secondaryInteractorRotation * Vector3.up;
        Vector3 up = Vector3.Slerp(primaryUp, secondaryUp, 0.5f);

        // Calculate right vector for cross product
        Vector3 primaryRight = ctx.interactorRotation * Vector3.right;
        Vector3 secondaryRight = ctx.secondaryInteractorRotation * Vector3.right;
        Vector3 right = Vector3.Slerp(primaryRight, secondaryRight, 0.5f);

        // Calculate perpendicular up to avoid gimbal lock
        Vector3 crossUp = Vector3.Cross(forward, right);
        float angleDiff = Mathf.PingPong(Vector3.Angle(up, forward), 90f);
        up = Vector3.Slerp(crossUp, up, angleDiff / 90f);

        // Recalculate to ensure orthogonality
        Vector3 crossRight = Vector3.Cross(up, forward);
        up = Vector3.Cross(forward, crossRight);

        // Maintain flip continuity - prevent sudden 180° flips
        if (Vector3.Dot(up, lastUp) <= 0f)
        {
            up = -up;
        }

        lastUp = up;

        return Quaternion.LookRotation(forward, up);
    }

    /// <summary>
    /// Applies scaling based on the distance between hands.
    /// Scale ratio = current distance / initial distance, clamped to min/max scale.
    /// </summary>
    private void ApplyScaling(XRMultiContext ctx)
    {
        if (initialHandsDistance <= 0)
            return;

        float scaleRatio = ctx.handsDistance / initialHandsDistance;
        scaleRatio = Mathf.Clamp(scaleRatio, minScale, maxScale);

        Vector3 newScale = initialScale * scaleRatio;
        ctx.interactable.transform.localScale = newScale;
    }

    /// <summary>
    /// Called when the object is completely released (all hands).
    /// Resets all two-hand state.
    /// </summary>
    public override void OnSelectExited(XRContext ctx)
    {
        base.OnSelectExited(ctx);

        // Reset state
        isInTwoHandMode = false;
        primaryAnchor = null;
        secondaryAnchor = null;
        initialHandsDistance = 0;
        lastUp = Vector3.up;
    }
    }
}
