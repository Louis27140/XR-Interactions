using Louis.XR.Interactions.Utils.Anchors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Louis.XR.Interactions.Grab
{

    public class XRContext
    {
        public IXRSelectInteractor interactor;

        public XRGenericInteractable interactable;

        public HandUsage hand;

        public Vector3 interactorPosition;
        public Quaternion interactorRotation;

        public Transform attachTransform;

        public List<XRAnchor> anchors;

        public float deltaTime;

        public bool isRemote;

        public bool isHeld;

        public XRInteractionManager manager;
    }
}
