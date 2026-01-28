using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace Louis.XR.Interactions.Input
{
    public enum XRHandSide
    {
        Left,
        Right
    }

    /// <summary>
    /// Type de bouton XR
    /// </summary>
    public enum XRButtonType
    {
        Primary,
        Secondary,
        Trigger,
        Grip,
        Menu
    }

    [DefaultExecutionOrder(-100)]
    public class XRInputRouter : MonoBehaviour
    {
        public static XRInputRouter Instance {get; private set;}

        [Header("Map Names")]
        [SerializeField] private string leftMapName = "XRI LeftHand Interaction";
        [SerializeField] private string rightMapName = "XRI RightHand Interaction";

        [Header("Action Names")]
        [SerializeField] private string triggerName = "Activate";
        [SerializeField] private string gripName = "Select";
        [SerializeField] private string primaryName = "Primary Button";
        [SerializeField] private string secondaryName = "Secondary Button";
        [SerializeField] private string thumbstickName = "Move";
        [SerializeField] private string thumbstickClickName = "Thumbstick Click";
        [SerializeField] private string triggerTouchName = "Trigger Touch";
        [SerializeField] private string gripTouchName = "Grip Touch";
        [SerializeField] private string primaryTouchName = "Primary Touch";
        [SerializeField] private string secondaryTouchName = "Secondary Touch";
        [SerializeField] private string menuName = "Menu";

        [Header("Haptics")]
        [SerializeField] private string hapticDeviceName = "Haptic Device";
        [SerializeField] private string hapticTriggerName = "Haptic Trigger";
        [SerializeField] private string hapticThumbstickName = "Haptic Thumbstick";

        // LEFT
        private InputAction l_trigger, l_grip, l_primary, l_secondary;
        private InputAction l_stick, l_stickClick;
        private InputAction l_triggerTouch, l_gripTouch, l_primaryTouch, l_secondaryTouch;
        private InputAction l_menu;
        private InputAction l_hapticDevice;
        private InputAction l_hapticTrigger;
        private InputAction l_hapticThumbstick;

        // RIGHT
        private InputAction r_trigger, r_grip, r_primary, r_secondary;
        private InputAction r_stick, r_stickClick;
        private InputAction r_triggerTouch, r_gripTouch, r_primaryTouch, r_secondaryTouch;

        private InputAction r_menu;
        private InputAction r_hapticDevice;
        private InputAction r_hapticTrigger;
        private InputAction r_hapticThumbstick;

        /// <summary>
        /// Récupère le InputDevice haptique pour une main donnée
        /// </summary>
        public InputDevice GetHapticDevice(XRHandSide hand)
        {
            InputAction hapticAction = hand == XRHandSide.Left ? l_hapticDevice : r_hapticDevice;
            
            if (hapticAction == null || hapticAction.activeControl == null)
            {
                return default;
            }

            return hapticAction.activeControl.device;
        }

        public InputDevice GetHapticTrigger(XRHandSide hand)
        {
            InputAction hapticAction = hand == XRHandSide.Left ? l_hapticTrigger : r_hapticTrigger;
            
            if (hapticAction == null || hapticAction.activeControl == null)
            {
                return default;
            }

            return hapticAction.activeControl.device;
        }

        public InputDevice GetHapticThumbstick(XRHandSide hand)
        {
            InputAction hapticAction = hand == XRHandSide.Left ? l_hapticThumbstick : r_hapticThumbstick;
            
            if (hapticAction == null || hapticAction.activeControl == null)
            {
                return default;
            }

            return hapticAction.activeControl.device;
        }

        private void Awake()
        {
            // Initialiser tous les types de boutons pour chaque main
            foreach (XRButtonType buttonType in System.Enum.GetValues(typeof(XRButtonType)))
            {
                foreach (XRHandSide side in System.Enum.GetValues(typeof(XRHandSide)))
                {
                    buttonPressCallbacks[(buttonType, side)] = new List<Action>();
                }
            }

            // 1) Fetch InputActionAsset automatiquement
            var manager = FindObjectOfType<InputActionManager>();
            if (manager == null || manager.actionAssets == null)
            {
                Debug.LogError("[XRInputRouter] InputActionManager or its actions not found!");
                return;
            }

            Instance = this;

            //recherche du bon InputActionAsset contenant le map demand�
            InputActionAsset asset = null;
            foreach (var a in manager.actionAssets)
            {
                if (a != null && a.FindActionMap(leftMapName, false) != null && a.FindActionMap(rightMapName, false) != null)
                {
                    asset = a;
                    break;
                }
            }

            if (asset == null)
            {
                Debug.LogError("[XRInputRouter] Aucun InputActionAsset ne contient les maps demand�s !");
                return;
            }

            // 2) Bind LEFT
            var left = asset.FindActionMap(leftMapName, true);
            l_trigger = Bind(left, triggerName);
            l_grip = Bind(left, gripName);
            l_primary = Bind(left, primaryName);
            l_secondary = Bind(left, secondaryName);
            l_stick = Bind(left, thumbstickName);
            l_stickClick = Bind(left, thumbstickClickName);
            l_triggerTouch = Bind(left, triggerTouchName);
            l_gripTouch = Bind(left, gripTouchName);
            l_primaryTouch = Bind(left, primaryTouchName);
            l_secondaryTouch = Bind(left, secondaryTouchName);
            l_menu = Bind(left, menuName);
            l_hapticDevice = Bind(left, hapticDeviceName);
            l_hapticTrigger = Bind(left, hapticTriggerName);
            l_hapticThumbstick = Bind(left, hapticThumbstickName);

            // 3) Bind RIGHT
            var right = asset.FindActionMap(rightMapName, true);
            r_trigger = Bind(right, triggerName);
            r_grip = Bind(right, gripName);
            r_primary = Bind(right, primaryName);
            r_secondary = Bind(right, secondaryName);
            r_stick = Bind(right, thumbstickName);
            r_stickClick = Bind(right, thumbstickClickName);
            r_triggerTouch = Bind(right, triggerTouchName);
            r_gripTouch = Bind(right, gripTouchName);
            r_primaryTouch = Bind(right, primaryTouchName);
            r_secondaryTouch = Bind(right, secondaryTouchName);
            r_menu = Bind(right, menuName);
            r_hapticDevice = Bind(right, hapticDeviceName);
            r_hapticTrigger = Bind(right, hapticTriggerName);
            r_hapticThumbstick = Bind(right, hapticThumbstickName);
        }

        private InputAction Bind(InputActionMap map, string actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName)) return null;

            var act = map.FindAction(actionName, false);
            if (act != null) act.Enable();
            return act;
        }

        // -------------------------------------------------------
        //  INPUT API
        // -------------------------------------------------------

        private InputAction A(XRHandSide side, InputAction left, InputAction right)
            => side == XRHandSide.Left ? left : right;

        // Trigger
        public float TriggerValue(XRHandSide side)
            => A(side, l_trigger, r_trigger)?.ReadValue<float>() ?? 0f;

        public bool TriggerPressed(XRHandSide side, float th = 0.5f)
            => TriggerValue(side) > th;

        public bool TriggerTouched(XRHandSide side)
        {
            var a = A(side, l_triggerTouch, r_triggerTouch);
            return a != null && a.ReadValue<float>() > 0.01f;
        }

        // Grip
        public float GripValue(XRHandSide side)
            => A(side, l_grip, r_grip)?.ReadValue<float>() ?? 0f;

        public bool GripPressed(XRHandSide side, float th = 0.5f)
            => GripValue(side) > th;

        public bool GripTouched(XRHandSide side)
            => A(side, l_gripTouch, r_gripTouch)?.ReadValue<float>() > 0.01f;

        // Primary (A / X)
        public bool PrimaryPressedThisFrame(XRHandSide side)
            => A(side, l_primary, r_primary)?.WasPressedThisFrame() ?? false;

        public bool PrimaryHeld(XRHandSide side)
            => A(side, l_primary, r_primary)?.IsPressed() ?? false;

        public bool PrimaryTouched(XRHandSide side)
            => A(side, l_primaryTouch, r_primaryTouch)?.ReadValue<float>() > 0.01f;

        // Secondary (B / Y)
        public bool SecondaryPressedThisFrame(XRHandSide side)
            => A(side, l_secondary, r_secondary)?.WasPressedThisFrame() ?? false;

        public bool SecondaryHeld(XRHandSide side)
            => A(side, l_secondary, r_secondary)?.IsPressed() ?? false;

        public bool SecondaryTouched(XRHandSide side)
            => A(side, l_secondaryTouch, r_secondaryTouch)?.ReadValue<float>() > 0.01f;

        // Thumbstick
        public Vector2 Thumbstick(XRHandSide side)
            => A(side, l_stick, r_stick)?.ReadValue<Vector2>() ?? Vector2.zero;

        public bool ThumbstickClicked(XRHandSide side)
            => A(side, l_stickClick, r_stickClick)?.IsPressed() ?? false;

        // Menu
        public bool MenuPressedThisFrame(XRHandSide side)
        => A(side, l_menu, r_menu)?.WasPressedThisFrame() ?? false;

        public bool MenuHeld(XRHandSide side)
            => A(side, l_menu, r_menu)?.IsPressed() ?? false;

        // Structure pour stocker les callbacks par type et main
        private Dictionary<(XRButtonType, XRHandSide), List<Action>> buttonPressCallbacks = 
            new Dictionary<(XRButtonType, XRHandSide), List<Action>>();

        private void Update()
        {            
            // Invoquer les callbacks pour chaque combinaison
            foreach (XRHandSide side in System.Enum.GetValues(typeof(XRHandSide)))
            {
                if (PrimaryPressedThisFrame(side))
                    InvokeCallbacks(XRButtonType.Primary, side);
                
                if (SecondaryPressedThisFrame(side))
                    InvokeCallbacks(XRButtonType.Secondary, side);
                
                if (TriggerPressed(side))
                    InvokeCallbacks(XRButtonType.Trigger, side);
                
                if (GripPressed(side))
                    InvokeCallbacks(XRButtonType.Grip, side);

                if (MenuPressedThisFrame(side))
                    InvokeCallbacks(XRButtonType.Menu, side);
            }
        }

        private void InvokeCallbacks(XRButtonType buttonType, XRHandSide hand)
        {
            var callbacks = buttonPressCallbacks[(buttonType, hand)];
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                callbacks[i]?.Invoke();
            }
        }

        /// <summary>
        /// Enregistre un callback pour un bouton spécifique
        /// </summary>
        public void RegisterButtonPress(XRButtonType buttonType, XRHandSide hand, Action callback)
        {
            if (callback != null)
            {
                var key = (buttonType, hand);
                if (!buttonPressCallbacks[key].Contains(callback))
                {
                    buttonPressCallbacks[key].Add(callback);
                }
            }
        }

        /// <summary>
        /// Désenregistre un callback pour un bouton spécifique
        /// </summary>
        public void UnregisterButtonPress(XRButtonType buttonType, XRHandSide hand, Action callback)
        {
            if (callback != null)
            {
                var key = (buttonType, hand);
                buttonPressCallbacks[key].Remove(callback);
            }
        }

        // Méthodes helper pour garder la compatibilité
        public void RegisterPrimaryButtonPress(XRHandSide hand, Action callback) 
            => RegisterButtonPress(XRButtonType.Primary, hand, callback);
        
        public void UnregisterPrimaryButtonPress(XRHandSide hand, Action callback) 
            => UnregisterButtonPress(XRButtonType.Primary, hand, callback);
        
        public void RegisterSecondaryButtonPress(XRHandSide hand, Action callback) 
            => RegisterButtonPress(XRButtonType.Secondary, hand, callback);
        
        public void UnregisterSecondaryButtonPress(XRHandSide hand, Action callback) 
            => UnregisterButtonPress(XRButtonType.Secondary, hand, callback);
        
        public void RegisterTriggerPress(XRHandSide hand, Action callback) 
            => RegisterButtonPress(XRButtonType.Trigger, hand, callback);
        
        public void UnregisterTriggerPress(XRHandSide hand, Action callback) 
            => UnregisterButtonPress(XRButtonType.Trigger, hand, callback);
        
        public void RegisterGripPress(XRHandSide hand, Action callback) 
            => RegisterButtonPress(XRButtonType.Grip, hand, callback);
        
        public void UnregisterGripPress(XRHandSide hand, Action callback) 
            => UnregisterButtonPress(XRButtonType.Grip, hand, callback);

        public void RegisterMenuPress(XRHandSide hand, Action callback) 
            => RegisterButtonPress(XRButtonType.Menu, hand, callback);

        public void UnregisterMenuPress(XRHandSide hand, Action callback) 
            => UnregisterButtonPress(XRButtonType.Menu, hand, callback);

        
    }
}
