using Louis.XR.Interactions.Hands;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Louis.XR.Interactions.Grab
{

    public class XRHandleInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
    {
        [SerializeField] private Transform frontHandleAttach;
        [SerializeField] private Transform backHandleAttach;

        [SerializeField] private Transform HandleRoot;
        private Dictionary<string, Transform> handleAttachPoints = new Dictionary<string, Transform>();

        [SerializeField] private GameObject LHand;
        [SerializeField] private GameObject RHand;

        [SerializeField] private List<Collider> doorColliders = new List<Collider>();

        private readonly List<(Collider, Collider)> ignoredPairs = new List<(Collider, Collider)>();

        private bool collisionsDisabled = false;

        private Coroutine restoreRoutine = null;

        protected override void Awake()
        {
            base.Awake();

            if (doorColliders == null || doorColliders.Count == 0)
            {
                doorColliders = GetComponents<Collider>().ToList();
            }

            if (HandleRoot != null)
            {
                foreach (Transform child in HandleRoot)
                {
                    handleAttachPoints[child.name] = child;
                }
            }
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            Transform targetAttach = GetHandleAttachForInteractor(args.interactorObject);
            attachTransform = targetAttach;

            EnableIgnoreCollisions(args.interactorObject);
            AppearHandOnHandle(args.interactorObject);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            restoreRoutine = StartCoroutine(DisableIgnoreCollisions(1f));
            DisappearHandOnHandle(args.interactorObject);
        }

        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);

            EnableIgnoreCollisions(args.interactorObject);
        }

        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);
            restoreRoutine = StartCoroutine(DisableIgnoreCollisions(.5f));
        }

        private IEnumerator DisableIgnoreCollisions(float delay)
        {
            if (!collisionsDisabled) yield break;

            yield return new WaitForSeconds(delay);

            collisionsDisabled = false;

            foreach (var (a, b) in ignoredPairs)
            {
                if (!a || !b) continue;
                Physics.IgnoreCollision(a, b, false);
            }
            ignoredPairs.Clear();
            restoreRoutine = null;
        }

        private void EnableIgnoreCollisions(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
        {
            var interactorCollider = GetInteractorCollider(interactor);
            if (interactorCollider == null) return;

            // On annule un restore en cours
            if (restoreRoutine != null)
            {
                StopCoroutine(restoreRoutine);
                restoreRoutine = null;
            }

            if (collisionsDisabled) return;
            collisionsDisabled = true;

            foreach (var doorCollider in doorColliders)
            {
                if (!doorCollider) continue;
                Physics.IgnoreCollision(interactorCollider, doorCollider, true);
                ignoredPairs.Add((interactorCollider, doorCollider));
            }
        }

        private Collider GetInteractorCollider(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
        {
            var controller = interactor.transform.GetComponent<HandController>();
            if (controller == null || controller.model == null) return null;

            var hand = controller.model.GetComponent<Hand>();
            return hand != null ? hand.GetCollider() : null;
        }

        private Transform GetHandleAttachForInteractor(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
        {
            if (frontHandleAttach == null && backHandleAttach == null)
                return attachTransform; // fallback s�cu

            Vector3 handPos = interactor.transform.position;

            // Cas avec une seule poign�e d�finie
            if (frontHandleAttach != null && backHandleAttach == null)
                return frontHandleAttach;

            if (backHandleAttach != null && frontHandleAttach == null)
                return backHandleAttach;

            // Deux poign�es -> on prend la plus proche
            float distFront = Vector3.Distance(handPos, frontHandleAttach.position);
            float distBack = Vector3.Distance(handPos, backHandleAttach.position);

            return distFront <= distBack ? frontHandleAttach : backHandleAttach;
        }

        private void AppearHandOnHandle(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
        {
            bool isLeft = interactor.transform.name.Contains("Left");
            bool isRight = interactor.transform.name.Contains("Right");

            if (!isLeft && !isRight) return;

            GameObject hand = isLeft ? LHand : RHand;

            interactor.transform.GetComponent<HandController>().model.GetComponent<Hand>().ToggleMesh(false);
            hand.SetActive(true);

            // On r�cup�re la pose depuis le dico
            string key = GetPoseKey(isLeft);
            Transform pose = GetHandlePose(key);

            hand.transform.SetParent(attachTransform);

            if (pose != null)
            {
                hand.transform.position = pose.position;
                hand.transform.rotation = pose.rotation;
            }
            else
            {
                // fallback si pas de pose trouv�e
                hand.transform.localPosition = Vector3.zero;
                hand.transform.localRotation = Quaternion.identity;
            }
        }

        private void DisappearHandOnHandle(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
        {
            bool isLeft = interactor.transform.name.Contains("Left");
            bool isRight = interactor.transform.name.Contains("Right");

            if (!isLeft && !isRight) return;

            GameObject hand = isLeft ? LHand : RHand;

            hand.transform.SetParent(null);

            interactor.transform.GetComponent<HandController>().model.GetComponent<Hand>().ToggleMesh(true);
            hand.SetActive(false);
        }

        private string GetPoseKey(bool isLeft)
        {
            // On consid�re que si attachTransform == frontHandleAttach -> front, sinon back
            bool isFront = (attachTransform == frontHandleAttach);

            // Noms attendus : LFront, LBack, RFront, RBack
            return (isLeft ? "L" : "R") + (isFront ? "Front" : "Back");
        }
        private Transform GetHandlePose(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            if (handleAttachPoints.TryGetValue(key, out var pose))
                return pose;

            Debug.LogWarning($"[XRHandleInteractable] Pose '{key}' non trouv�e sur {name}. Fallback sur attachTransform.");
            return null;
        }

    }
}
