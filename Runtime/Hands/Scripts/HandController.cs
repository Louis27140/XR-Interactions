using Louis.Interactions.Hands;
using Louis.XR.Interactions.Input;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace Louis.XR.Interactions.Hands
{

    public class HandController : MonoBehaviour
    {

        public XRHandSide handSide = XRHandSide.Right;

        XRDirectInteractor xrGrab;

        private Hand hand;

        public GameObject model;

        private void Awake()
        {
            hand = GetComponentInChildren<Hand>();
        }

        // Start is called before the first frame update
        void Start()
        {
            xrGrab = GetComponent<XRDirectInteractor>();

        }

        // Update is called once per frame
        void Update()
        {
            if (hand != null)
            {
                hand.SetGrip(XRInputRouter.Instance.GripValue(handSide));
                hand.SetTrigger(XRInputRouter.Instance.TriggerValue(handSide));
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
