using Louis.Interactions.Hands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Louis.XR.Interactions.Hands
{

    [RequireComponent(typeof(ActionBasedController))]
    public class HandController : MonoBehaviour
    {
        ActionBasedController controller;

        XRDirectInteractor xrGrab;

        private Hand hand;

        private void Awake()
        {
            hand = GetComponentInChildren<Hand>();
        }

        // Start is called before the first frame update
        void Start()
        {
            controller = GetComponent<ActionBasedController>();
            xrGrab = GetComponent<XRDirectInteractor>();

        }

        // Update is called once per frame
        void Update()
        {
            if (hand != null)
            {
                hand.SetGrip(controller.selectAction.action.ReadValue<float>());
                hand.SetTrigger(controller.activateAction.action.ReadValue<float>());
                bool grabItem = xrGrab.firstInteractableSelected != null ? true : false;
                hand.SetGrab(grabItem);
            }
            else
            {
                hand = GetComponentInChildren<Hand>();
            }
        }
    }
}
