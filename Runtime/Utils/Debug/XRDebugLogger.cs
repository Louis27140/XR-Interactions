using Louis.Core.Utils.Debugger;
using Louis.XR.Interactions.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Louis.XR.Interactions.Utils.Debugger
{

    public class XRDebugLogger : MonoBehaviour
    {

        [SerializeField] private float holdDuration = 2f;
        [SerializeField] private XRHandSide hand = XRHandSide.Left;

        private float time = 0f;

        private IDebuggerLogger logger;

        private bool isDebuggerActive = false;

        private void Awake()
        {
            logger = GetComponent<IDebuggerLogger>();
        }

        private void Start()
        {
            logger.ToggleDebugger(isDebuggerActive);
        }

        // Update is called once per frame
        void Update()
        {
            bool primaryButtonPressed = XRInputRouter.Instance.PrimaryHeld(hand);
            bool secondaryButtonPressed = XRInputRouter.Instance.SecondaryHeld(hand);

            if(primaryButtonPressed && secondaryButtonPressed)
            {
                time += Time.deltaTime;
                if(time >= holdDuration)
                {
                    isDebuggerActive = !isDebuggerActive;
                    logger.ToggleDebugger(isDebuggerActive);
                    time = 0f; // Reset time to prevent multiple toggles
                }
            }
            else
            {
                time = 0f;
            }

        }
    }
}
