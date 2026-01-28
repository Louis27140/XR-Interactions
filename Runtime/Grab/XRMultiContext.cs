using Louis.XR.Interactions.Utils.Anchors;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Louis.XR.Interactions.Grab
{
    /// <summary>
    /// Extended context for multi-interactor grab interactions (e.g., two-hand grab).
    /// Inherits from XRContext and adds data for secondary interactor and computed two-hand properties.
    /// </summary>
    public class XRMultiContext : XRContext
    {
        /// <summary>
        /// List of all interactors currently selecting this interactable.
        /// </summary>
        public List<IXRSelectInteractor> allInteractors;

        /// <summary>
        /// Number of interactors currently selecting this interactable.
        /// </summary>
        public int interactorCount => allInteractors?.Count ?? 0;

        /// <summary>
        /// The secondary interactor (typically the second hand).
        /// </summary>
        public IXRSelectInteractor secondaryInteractor;

        /// <summary>
        /// Position of the secondary interactor in world space.
        /// </summary>
        public Vector3 secondaryInteractorPosition;

        /// <summary>
        /// Rotation of the secondary interactor in world space.
        /// </summary>
        public Quaternion secondaryInteractorRotation;

        /// <summary>
        /// Hand usage for the secondary interactor (Left, Right, Both, or None).
        /// </summary>
        public HandUsage secondaryHand;

        /// <summary>
        /// Center position between the primary and secondary interactors.
        /// Calculated as (primaryPosition + secondaryPosition) / 2.
        /// </summary>
        public Vector3 centerPosition;

        /// <summary>
        /// Normalized direction vector from primary to secondary interactor.
        /// Useful for aligning objects along the axis between hands.
        /// </summary>
        public Vector3 handsDirection;

        /// <summary>
        /// Distance between the primary and secondary interactors.
        /// Used for scaling calculations in two-hand grab.
        /// </summary>
        public float handsDistance;
    }
}
