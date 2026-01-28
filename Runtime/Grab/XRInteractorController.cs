using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Louis.XR.Interactions.Grab
{


    public class XRInteractorController : MonoBehaviour
    {
        [SerializeField] private XRBaseInteractor directInteractor;
        [SerializeField] private XRBaseInteractor rayInteractor;
        [SerializeField] private XRBaseInteractor uiInteractor;

        [SerializeField] private GameObject dynamicHand;

        [SerializeField] private GameObject staticHand;

        private void Reset()
        {
            directInteractor = GetComponentInParent<XRBaseInteractor>();
        }

        private void OnEnable()
        {
            directInteractor.selectEntered.AddListener(OnGrab);
            directInteractor.selectExited.AddListener(OnRelease);

            if (staticHand != null)
                staticHand.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            directInteractor.selectEntered.RemoveListener(OnGrab);
            directInteractor.selectExited.RemoveListener(OnRelease);

            DisableHandleMode();
        }

        private void OnGrab(SelectEnterEventArgs args)
        {
            if (rayInteractor != null)
                rayInteractor.enabled = false;

            if(uiInteractor != null)
                uiInteractor.enabled = false;

            var grab = args.interactableObject as XRGenericInteractable;
            if (grab == null)
                return;

            if (!grab.UseHandle) return;

            EnableHandleMode(grab.attachTransform.transform);

        }

        private void OnRelease(SelectExitEventArgs args)
        {
            if (rayInteractor != null)
                rayInteractor.enabled = true;

            if (uiInteractor != null)
                uiInteractor.enabled = true;

            DisableHandleMode();
        }

        private void EnableHandleMode(Transform anchor)
        {
            // d�sactiver la main dynamique
            if (dynamicHand != null)
                dynamicHand.SetActive(false);

            // placer la main statique
            if (staticHand != null)
            {
                staticHand.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
                staticHand.transform.SetParent(anchor);
                staticHand.SetActive(true);
            }
        }

        private void DisableHandleMode()
        {
            if (staticHand != null)
            {
                staticHand.transform.SetParent(null);
                staticHand.SetActive(false);
            }

            if (dynamicHand != null)
                dynamicHand.SetActive(true);
        }


    }
}
