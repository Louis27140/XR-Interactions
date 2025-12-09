using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Louis.XR.Interactions.Grab
{

    public class XRContext
    {
        // Interactor qui a initié le grab
        public IXRSelectInteractor interactor;

        // Interactable en cours (ton XRGenericGrab)
        public XRGenericInteractable interactable;

        // Main utilisée (Left / Right)
        public HandUsage hand;

        // Position/Roation du point de référence (main)
        public Vector3 interactorPosition;
        public Quaternion interactorRotation;

        // Le attachTransform que la logique peut déplacer
        public Transform attachTransform;

        // Liste des anchors (après filtrage ou brute)
        public List<XRAnchor> anchors;

        // Delta time du frame (utile pour Process)
        public float deltaTime;

        // True si c'est un interactor à distance (ray / indirect)
        public bool isRemote;

        // True si l'objet est déjà tenu
        public bool isHeld;

        public XRInteractionManager manager;
    }
}
