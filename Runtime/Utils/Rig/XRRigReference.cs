using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Louis.XR.Interactions.Utils.Rig
{
    /// <summary>
    /// Référence au rig XR
    /// </summary>
    public class XRRigReference : MonoBehaviour
    {
    private static XRRigReference instance;
        public static XRRigReference Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<XRRigReference>();
                    if (instance == null)
                    {
                        var origin = FindObjectOfType<XROrigin>();
                        if (origin != null)
                        {
                            instance = origin.gameObject.AddComponent<XRRigReference>();
                        }
                    }
                }
                return instance;
            }
        }

        [Header("XR Rig References")]
        [SerializeField] private Transform rigTransform;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Transform rightHandTransform;

        public Transform Rig => rigTransform != null ? rigTransform : transform;
        public Transform Camera => cameraTransform;
        public Transform LeftHand => leftHandTransform;
        public Transform RightHand => rightHandTransform;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(this);
                return;
            }

            AutoFindReferences();
        }

        private void AutoFindReferences()
        {
            if (rigTransform == null)
            {
                rigTransform = transform;
            }

            if (cameraTransform == null)
            {
                cameraTransform = GetComponentInChildren<Camera>()?.transform;
            }

            // Chercher les controllers XR
            var controllers = GetComponentsInChildren<XRBaseController>();
            foreach (var controller in controllers)
            {
                if (controller.name.ToLower().Contains("left") && leftHandTransform == null)
                {
                    leftHandTransform = controller.transform;
                }
                else if (controller.name.ToLower().Contains("right") && rightHandTransform == null)
                {
                    rightHandTransform = controller.transform;
                }
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            AutoFindReferences();
        }
    }
}