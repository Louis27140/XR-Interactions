using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Louis.Interactions.Hands {

    [RequireComponent(typeof(Animator))]
    public class Hand : MonoBehaviour
    {
        Animator anim;
        private float gripValue;
        private float triggerValue;
        private bool grabValue = false;

        private float currentGrip;
        private float currentTrigger;

        [SerializeField]
        private float speed = 2.5f;

        [SerializeField]
        private Transform fingerTip;

        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor controller;

        private MeshCollider handCollider;
        private SkinnedMeshRenderer handMeshRenderer;

        [SerializeField] private bool disableCollider = false;

        // Start is called before the first frame update
        void Start()
        {
            anim = GetComponent<Animator>();
            controller = GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();

            GameObject pokeInteractorGO = new GameObject("Poke Interactor", typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor));
            pokeInteractorGO.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor>().attachTransform = fingerTip;

            pokeInteractorGO.transform.parent = transform.parent;

            handMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            handCollider = GetComponentInChildren<MeshCollider>();

            if (disableCollider && handCollider != null)
            {
                handCollider.enabled = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            AnimateHand();
        }

        private void AnimateHand()
        {
            if (currentGrip != gripValue)
            {
                currentGrip = Mathf.MoveTowards(currentGrip, gripValue, Time.deltaTime * speed);
                anim.SetFloat("Grip", currentGrip);
            }
            if (currentTrigger != triggerValue)
            {
                currentTrigger = Mathf.MoveTowards(currentTrigger, triggerValue, Time.deltaTime * speed);
                anim.SetFloat("Trigger", currentTrigger);
            }
            anim.SetBool("Grab", grabValue);
        }

        internal void SetGrab(bool v)
        {
            grabValue = v;
        }

        internal void SetGrip(float v)
        {
            gripValue = v;
        }

        internal void SetTrigger(float v)
        {
            triggerValue = v;
        }

        public Collider GetCollider()
        {
            return handCollider;
        }

        public void ToggleMesh(bool isActive)
        {
            handMeshRenderer.enabled = isActive;
        }
    }


}
