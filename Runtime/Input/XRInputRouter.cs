using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using Louis.Core.Input;

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
    public class XRInputRouter : MonoBehaviour, IInputSource
    {
        public static XRInputRouter Instance {get; private set;}

        // IInputSource implementation
        public string SourceName => "XR Controllers";
        public bool IsActive => Instance != null;

        // Registry optimisé des InputDefinitions actifs (ceux avec callbacks)
        private static HashSet<InputDefinition> activeInputDefinitions = new HashSet<InputDefinition>();
        
        // Liste réutilisée pour éviter les allocations dans Update()
        private static List<InputDefinition> sortedInputsCache = new List<InputDefinition>();
        
        // Cache des valeurs de l'enum XRHandSide pour éviter les allocations
        private static readonly XRHandSide[] allHandSides = (XRHandSide[])System.Enum.GetValues(typeof(XRHandSide));

        // Stack de contextes actifs (plus récent = top of stack)
        // Le contexte au sommet détermine quels inputs peuvent s'exécuter
        private static Stack<int> contextStack = new Stack<int>();

        /// <summary>
        /// Pousse un nouveau contexte sur le stack
        /// Les inputs dont le contextMask inclut ce contexte pourront s'exécuter
        /// </summary>
        public static void PushContext(int contextIndex)
        {
            contextStack.Push(contextIndex);
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            string contextName = Instance != null && Instance.contextSettings != null
                ? Instance.contextSettings.GetContextName(contextIndex)
                : $"Context {contextIndex}";
            
            Debug.Log($"[XRInputRouter] Pushed context: {contextName} (stack depth: {contextStack.Count})");
#endif
        }

        /// <summary>
        /// Retire le contexte actuel du stack
        /// Retourne au contexte précédent (ou Default si le stack devient vide)
        /// </summary>
        public static void PopContext()
        {
            if (contextStack.Count > 0)
            {
                int popped = contextStack.Pop();
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                string contextName = Instance != null && Instance.contextSettings != null
                    ? Instance.contextSettings.GetContextName(popped)
                    : $"Context {popped}";
                
                Debug.Log($"[XRInputRouter] Popped context: {contextName} (stack depth: {contextStack.Count})");
#endif
            }
            else
            {
                Debug.LogWarning("[XRInputRouter] Attempted to pop context from empty stack!");
            }
        }

        /// <summary>
        /// Réinitialise le stack de contextes (pousse le contexte Default)
        /// </summary>
        public static void ClearContextStack()
        {
            contextStack.Clear();
            // IMPORTANT: Pousser le contexte Default (0) pour éviter un stack vide
            contextStack.Push(0);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("[XRInputRouter] Context stack cleared (Default context pushed).");
#endif
        }

        /// <summary>
        /// Vérifie si un masque de contexte est compatible avec le contexte actuel du stack
        /// </summary>
        private static bool IsContextMaskValid(int contextMask)
        {
            // Le stack ne devrait jamais être vide (Default toujours présent)
            int currentContext = contextStack.Count > 0 ? contextStack.Peek() : 0;

            // Calcul explicite du bit à vérifier
            int bitToCheck = 1 << currentContext;
            bool isValid = (contextMask & bitToCheck) != 0;
            
            // Logs désactivés par défaut (trop de spam)
            // Active "Enable Debug Logs" dans l'inspecteur si nécessaire
            
            return isValid;
        }
        
        /// <summary>
        /// DEBUG: Teste explicitement si un masque passe la vérification
        /// </summary>
        public static void DebugTestMask(int contextMask)
        {
            int currentContext = contextStack.Count > 0 ? contextStack.Peek() : 0;
            int bitToCheck = 1 << currentContext;
            bool isValid = (contextMask & bitToCheck) != 0;
            
            string contextName = Instance?.contextSettings?.GetContextName(currentContext) ?? $"Context {currentContext}";
            
            Debug.Log($"=== MASK TEST ===\n" +
                      $"Input mask:     {contextMask} = 0b{System.Convert.ToString(contextMask, 2).PadLeft(32, '0')}\n" +
                      $"Current context: {contextName} (index {currentContext})\n" +
                      $"Bit to check:   {bitToCheck} = 0b{System.Convert.ToString(bitToCheck, 2).PadLeft(32, '0')}\n" +
                      $"mask & bit:     {contextMask & bitToCheck}\n" +
                      $"Result:         {(isValid ? "✓ PASS" : "✗ FAIL")}");
        }

        /// <summary>
        /// Enregistre un InputDefinition pour recevoir des updates automatiques
        /// Appelé automatiquement quand on s'abonne à un callback
        /// </summary>
        public static void Register(InputDefinition input)
        {
            if (input != null)
            {
                activeInputDefinitions.Add(input);
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (Instance != null && Instance.enableDebugLogs)
                    Debug.Log($"[XRInputRouter] Registered: {input.InputName} ({input.GetType().Name}) - Priority: {input.Priority}");
#endif
            }
        }

        /// <summary>
        /// Désenregistre un InputDefinition
        /// Appelé automatiquement quand on se désabonne de tous les callbacks
        /// </summary>
        public static void Unregister(InputDefinition input)
        {
            if (input != null)
            {
                activeInputDefinitions.Remove(input);
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (Instance != null && Instance.enableDebugLogs)
                    Debug.Log($"[XRInputRouter] Unregistered: {input.InputName} ({input.GetType().Name})");
#endif
            }
        }

        /// <summary>
        /// Récupère tous les noms d'inputs disponibles depuis l'InputActionAsset
        /// Utilisé par le PropertyDrawer pour afficher le dropdown
        /// </summary>
        public static string[] GetAvailableInputNames()
        {
            var actionInfos = GetAvailableInputsWithTypes();
            if (actionInfos == null || actionInfos.Length == 0)
                return new string[0];
            
            return actionInfos.Select(a => a.name).ToArray();
        }

        /// <summary>
        /// Structure pour stocker le nom, le type et la main d'une action
        /// </summary>
        public struct InputActionInfo
        {
            public string name;
            public InputType type;
            public XRHandSide handSide;

            public InputActionInfo(string name, InputType type, XRHandSide handSide)
            {
                this.name = name;
                this.type = type;
                this.handSide = handSide;
            }
            
            /// <summary>
            /// Retourne le nom complet avec l'indication de la main (pour affichage)
            /// </summary>
            public string GetDisplayName()
            {
                return $"{name} ({handSide})";
            }
        }

        /// <summary>
        /// Récupère toutes les actions avec leurs types depuis l'InputActionAsset
        /// Utilisé par le PropertyDrawer pour grouper correctement les inputs
        /// Fonctionne aussi en mode Editor sans Instance
        /// </summary>
        public static InputActionInfo[] GetAvailableInputsWithTypes()
        {
            // Noms des maps par défaut (utilisés si pas d'instance)
            string leftMapName = DEFAULT_LEFT_MAP_NAME;
            string rightMapName = DEFAULT_RIGHT_MAP_NAME;
            
            // Si on a une instance, utiliser ses valeurs
            if (Instance != null)
            {
                leftMapName = Instance.leftMapName;
                rightMapName = Instance.rightMapName;
            }
            
            // Chercher l'InputActionAsset
            InputActionAsset asset = FindInputActionAsset(leftMapName);
            if (asset == null)
                return new InputActionInfo[0];

            // Récupérer toutes les actions avec leurs types et leur main
            var actionInfos = new List<InputActionInfo>();
            
            var leftMap = asset.FindActionMap(leftMapName, false);
            var rightMap = asset.FindActionMap(rightMapName, false);

            // Ajouter les actions de la main gauche
            if (leftMap != null)
            {
                foreach (var action in leftMap.actions)
                {
                    actionInfos.Add(new InputActionInfo(action.name, GetInputTypeFromAction(action), XRHandSide.Left));
                }
            }

            // Ajouter les actions de la main droite
            if (rightMap != null)
            {
                foreach (var action in rightMap.actions)
                {
                    actionInfos.Add(new InputActionInfo(action.name, GetInputTypeFromAction(action), XRHandSide.Right));
                }
            }

            actionInfos.Sort((a, b) => string.Compare(a.name, b.name));
            return actionInfos.ToArray();
        }

        /// <summary>
        /// Trouve l'InputActionAsset contenant les maps XR
        /// Fonctionne en mode Editor et Runtime
        /// </summary>
        private static InputActionAsset FindInputActionAsset(string mapNameToFind)
        {
            // En runtime, chercher via InputActionManager dans la scène
            var manager = UnityEngine.Object.FindObjectOfType<InputActionManager>();
            if (manager != null && manager.actionAssets != null)
            {
                foreach (var a in manager.actionAssets)
                {
                    if (a != null && a.FindActionMap(mapNameToFind, false) != null)
                    {
                        return a;
                    }
                }
            }
            
#if UNITY_EDITOR
            // En mode Editor, chercher dans tous les InputActionAssets du projet
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:InputActionAsset");
            foreach (var guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                if (asset != null && asset.FindActionMap(mapNameToFind, false) != null)
                {
                    return asset;
                }
            }
#endif
            
            return null;
        }

        // Constantes pour les noms de maps par défaut
        private const string DEFAULT_LEFT_MAP_NAME = "LouisXR LeftHand";
        private const string DEFAULT_RIGHT_MAP_NAME = "LouisXR RightHand";

        /// <summary>
        /// Détermine le type d'input depuis une InputAction
        /// </summary>
        private static InputType GetInputTypeFromAction(InputAction action)
        {
            // Analyser le type de contrôle attendu
            var expectedControlType = action.expectedControlType;

            if (expectedControlType == "Vector2")
                return InputType.Vector2;
            else if (expectedControlType == "Button" || expectedControlType == "")
                return InputType.Bool;
            else if (expectedControlType == "Axis" || expectedControlType == "Analog")
                return InputType.Float;
            
            // Par défaut, essayer de deviner depuis le nom
            string name = action.name.ToLower();
            if (name.Contains("thumbstick") || name.Contains("move") || name.Contains("stick"))
                return InputType.Vector2;
            else if (name.Contains("value") || name.Contains("trigger") || name.Contains("grip"))
                return InputType.Float;
            else
                return InputType.Bool;
        }

        [Tooltip("Configuration des contextes d'input (Create > Core/Input/Input Context Settings)")]
        [SerializeField] private InputContextSettings contextSettings;

        /// <summary>
        /// Accès public aux settings de contextes
        /// </summary>
        public InputContextSettings ContextSettings => contextSettings;
        
        [Header("Debug")]
        [Tooltip("Active les logs détaillés pour le débogage")]
        [SerializeField] private bool enableDebugLogs = false;
        
        [Tooltip("Affiche l'état des inputs dans l'inspecteur (Editor uniquement)")]
        [SerializeField] private bool showDebugInfo = false;

        [Header("Map Names")]
        [SerializeField] private string leftMapName = "LouisXR LeftHand";
        [SerializeField] private string rightMapName = "LouisXR RightHand";

        // Dictionnaire pour stocker toutes les actions chargées dynamiquement
        private Dictionary<(string actionName, XRHandSide hand), InputAction> actionCache = new Dictionary<(string, XRHandSide), InputAction>();

        /// <summary>
        /// Récupère le InputDevice haptique pour une main donnée
        /// </summary>
        public InputDevice GetHapticDevice(XRHandSide hand)
        {
            var hapticAction = GetAction("Haptic Device", hand);
            
            if (hapticAction == null || hapticAction.activeControl == null)
            {
                return default;
            }

            return hapticAction.activeControl.device;
        }

        public InputDevice GetHapticTrigger(XRHandSide hand)
        {
            var hapticAction = GetAction("Trigger Haptic", hand);
            
            if (hapticAction == null || hapticAction.activeControl == null)
            {
                return default;
            }

            return hapticAction.activeControl.device;
        }

        public InputDevice GetHapticThumbstick(XRHandSide hand)
        {
            var hapticAction = GetAction("Joystick Haptic", hand);
            
            if (hapticAction == null || hapticAction.activeControl == null)
            {
                return default;
            }

            return hapticAction.activeControl.device;
        }
        
        /// <summary>
        /// Récupère une action depuis le cache par nom et main
        /// </summary>
        private InputAction GetAction(string actionName, XRHandSide hand)
        {
            var key = (actionName, hand);
            return actionCache.TryGetValue(key, out var action) ? action : null;
        }

        private void Awake()
        {
            // Setup registration delegates pour le système d'input optimisé
            // Les InputDefinitions appellent ces delegates quand on s'abonne/désabonne aux callbacks
            InputDefinition.OnRegister = Register;
            InputDefinition.OnUnregister = Unregister;
            InputDefinition.OnCheckContext = IsContextMaskValid;

            // Initialiser le stack de contextes avec Default (index 0)
            ClearContextStack();

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

            //recherche du bon InputActionAsset contenant le map demandé
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
                Debug.LogError("[XRInputRouter] Aucun InputActionAsset ne contient les maps demandés !");
                return;
            }

            // Charger toutes les actions dynamiquement dans le dictionnaire
            var leftMap = asset.FindActionMap(leftMapName, true);
            var rightMap = asset.FindActionMap(rightMapName, true);
            
            if (leftMap != null)
            {
                foreach (var action in leftMap.actions)
                {
                    action.Enable();
                    actionCache[(action.name, XRHandSide.Left)] = action;
                }
            }
            
            if (rightMap != null)
            {
                foreach (var action in rightMap.actions)
                {
                    action.Enable();
                    actionCache[(action.name, XRHandSide.Right)] = action;
                }
            }
            
            Debug.Log($"[XRInputRouter] Chargé {actionCache.Count} actions depuis les ActionMaps");
        }

        // -------------------------------------------------------
        //  INPUT API
        // -------------------------------------------------------

        // Trigger
        public float TriggerValue(XRHandSide side)
            => GetAction("Select Value", side)?.ReadValue<float>() ?? 0f;

        public bool TriggerPressed(XRHandSide side, float th = 0.5f)
            => TriggerValue(side) > th;

        public bool TriggerTouched(XRHandSide side)
        {
            var a = GetAction("Select", side);
            return a != null && a.ReadValue<float>() > 0.01f;
        }

        // Grip
        public float GripValue(XRHandSide side)
            => GetAction("Activate Value", side)?.ReadValue<float>() ?? 0f;

        public bool GripPressed(XRHandSide side, float th = 0.5f)
            => GripValue(side) > th;

        public bool GripTouched(XRHandSide side)
            => GetAction("Activate", side)?.ReadValue<float>() > 0.01f;

        // Primary (A / X)
        public bool PrimaryPressedThisFrame(XRHandSide side)
            => GetAction("Primary Button", side)?.WasPressedThisFrame() ?? false;

        public bool PrimaryHeld(XRHandSide side)
            => GetAction("Primary Button", side)?.IsPressed() ?? false;

        public bool PrimaryTouched(XRHandSide side)
            => GetAction("Primary Touch", side)?.ReadValue<float>() > 0.01f;

        // Secondary (B / Y)
        public bool SecondaryPressedThisFrame(XRHandSide side)
            => GetAction("Secondary Button", side)?.WasPressedThisFrame() ?? false;

        public bool SecondaryHeld(XRHandSide side)
            => GetAction("Secondary Button", side)?.IsPressed() ?? false;

        public bool SecondaryTouched(XRHandSide side)
            => GetAction("Secondary Touch", side)?.ReadValue<float>() > 0.01f;

        // Thumbstick
        public Vector2 Thumbstick(XRHandSide side)
            => GetAction("Move", side)?.ReadValue<Vector2>() ?? Vector2.zero;

        public bool ThumbstickClicked(XRHandSide side)
            => GetAction("Primary 2D Axis Click", side)?.IsPressed() ?? false;

        // Menu
        public bool MenuPressedThisFrame(XRHandSide side)
            => GetAction("Menu", side)?.WasPressedThisFrame() ?? false;

        public bool MenuHeld(XRHandSide side)
            => GetAction("Menu", side)?.IsPressed() ?? false;

        private void OnDestroy()
        {
            // Nettoyer les delegates et l'instance singleton
            if (Instance == this)
            {
                Instance = null;
                InputDefinition.OnRegister = null;
                InputDefinition.OnUnregister = null;
                InputDefinition.OnCheckContext = null;
                activeInputDefinitions.Clear();
                ClearContextStack();
            }
        }

        // Structure pour stocker les callbacks par type et main
        private Dictionary<(XRButtonType, XRHandSide), List<Action>> buttonPressCallbacks = 
            new Dictionary<(XRButtonType, XRHandSide), List<Action>>();

        private void Update()
        {
            // Réutiliser la liste existante au lieu d'en allouer une nouvelle
            sortedInputsCache.Clear();
            sortedInputsCache.AddRange(activeInputDefinitions);
            
            // Trier par priorité descendante (O(n log n) mais n est petit)
            if (sortedInputsCache.Count > 1)
            {
                sortedInputsCache.Sort((a, b) => ((int)b.Priority).CompareTo((int)a.Priority));
            }

            // Exécuter les callbacks dans l'ordre de priorité
            for (int i = 0; i < sortedInputsCache.Count; i++)
            {
                var input = sortedInputsCache[i];
                
                if (input is FloatInputDefinition floatInput && floatInput.HasCallbacks)
                {
                    floatInput.UpdateCallbacks(floatInput.Channel);
                }
                else if (input is Vector2InputDefinition vec2Input && vec2Input.HasCallbacks)
                {
                    vec2Input.UpdateCallbacks(vec2Input.Channel);
                }
                else if (input is BoolInputDefinition boolInput && boolInput.HasCallbacks)
                {
                    boolInput.UpdateCallbacks(boolInput.Channel);
                }
            }
            
            // Invoquer les callbacks pour chaque combinaison (ancien système)
            // Utiliser le cache d'enum pour éviter les allocations
            for (int s = 0; s < allHandSides.Length; s++)
            {
                XRHandSide side = allHandSides[s];
                
                // Edge-detection pour Primary/Secondary
                if (PrimaryPressedThisFrame(side))
                    InvokeCallbacks(XRButtonType.Primary, side);
                
                if (SecondaryPressedThisFrame(side))
                    InvokeCallbacks(XRButtonType.Secondary, side);
                
                // Edge-detection pour Trigger
                var triggerAction = GetAction("Select Value", side);
                if (triggerAction != null && triggerAction.WasPressedThisFrame())
                    InvokeCallbacks(XRButtonType.Trigger, side);
                
                // Edge-detection pour Grip
                var gripAction = GetAction("Activate Value", side);
                if (gripAction != null && gripAction.WasPressedThisFrame())
                    InvokeCallbacks(XRButtonType.Grip, side);

                // Edge-detection pour Menu
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

        // -------------------------------------------------------
        //  IInputSource Implementation
        // -------------------------------------------------------

        /// <summary>
        /// Implémentation IInputSource - Lit une valeur float par nom
        /// </summary>
        public float ReadFloat(string inputName, int channel = 0)
        {
            XRHandSide side = (XRHandSide)channel;
            var action = GetAction(inputName, side);
            
            if (action == null)
            {
                Debug.LogWarning($"[XRInputRouter] Float action '{inputName}' not found for {side} hand");
                return 0f;
            }
            
            return action.ReadValue<float>();
        }

        /// <summary>
        /// Implémentation IInputSource - Lit une valeur Vector2 par nom
        /// </summary>
        public Vector2 ReadVector2(string inputName, int channel = 0)
        {
            XRHandSide side = (XRHandSide)channel;
            var action = GetAction(inputName, side);
            
            if (action == null)
            {
                Debug.LogWarning($"[XRInputRouter] Vector2 action '{inputName}' not found for {side} hand");
                return Vector2.zero;
            }
            
            return action.ReadValue<Vector2>();
        }

        /// <summary>
        /// Implémentation IInputSource - Lit un état bool par nom
        /// </summary>
        public bool ReadBool(string inputName, int channel = 0)
        {
            XRHandSide side = (XRHandSide)channel;
            var action = GetAction(inputName, side);
            
            if (action == null)
            {
                Debug.LogWarning($"[XRInputRouter] Bool action '{inputName}' not found for {side} hand");
                return false;
            }
            
            return action.IsPressed();
        }
        
        // -------------------------------------------------------
        //  DEBUG UTILITIES
        // -------------------------------------------------------
        
        /// <summary>
        /// Retourne le nombre d'inputs actifs avec callbacks
        /// </summary>
        public static int GetActiveInputCount() => activeInputDefinitions.Count;
        
        /// <summary>
        /// Retourne la liste des inputs actifs (pour debug)
        /// </summary>
        public static List<string> GetActiveInputNames()
        {
            var names = new List<string>();
            foreach (var input in activeInputDefinitions)
            {
                names.Add($"{input.InputName} ({input.GetType().Name}, Priority: {input.Priority})");
            }
            return names;
        }
        
        /// <summary>
        /// Affiche l'état complet du système dans la console
        /// </summary>
        public static void DumpDebugInfo()
        {
            if (Instance == null)
            {
                Debug.Log("[XRInputRouter] Instance not initialized");
                return;
            }
            
            Debug.Log("========== XRInputRouter Debug Info ==========");
            Debug.Log($"Active Inputs: {activeInputDefinitions.Count}");
            Debug.Log($"Context Stack Depth: {contextStack.Count}");
            
            // Afficher le contexte actuel avec son nom
            int currentContext = contextStack.Count > 0 ? contextStack.Peek() : 0;
            string currentContextName = Instance.contextSettings != null 
                ? Instance.contextSettings.GetContextName(currentContext) 
                : $"Context {currentContext}";
            Debug.Log($"Current Context: {currentContextName} (index {currentContext})");
            
            Debug.Log($"Total Actions Loaded: {Instance.actionCache.Count}");
            Debug.Log($"Delegates Set: OnRegister={InputDefinition.OnRegister != null}, OnUnregister={InputDefinition.OnUnregister != null}, OnCheckContext={InputDefinition.OnCheckContext != null}");
            
            Debug.Log("\n--- Context Stack (top to bottom) ---");
            if (contextStack.Count > 0)
            {
                var stackArray = contextStack.ToArray();
                for (int i = 0; i < stackArray.Length; i++)
                {
                    string name = Instance.contextSettings != null 
                        ? Instance.contextSettings.GetContextName(stackArray[i]) 
                        : $"Context {stackArray[i]}";
                    Debug.Log($"  [{i}] {name} (index {stackArray[i]})");
                }
            }
            else
            {
                Debug.Log("  (empty - will use Default context 0)");
            }
            
            Debug.Log("\n--- Active InputDefinitions ---");
            foreach (var input in activeInputDefinitions)
            {
                string maskBinary = System.Convert.ToString(input.ContextMask, 2).PadLeft(8, '0');
                bool contextValid = InputDefinition.OnCheckContext?.Invoke(input.ContextMask) ?? true;
                string contextStatus = contextValid ? "✓" : "✗";
                Debug.Log($"  {contextStatus} {input.InputName} ({input.GetType().Name}) - Priority: {input.Priority}, Channel: {GetInputChannel(input)}, Mask: 0b{maskBinary}");
            }
            
            Debug.Log("\n--- Loaded Actions ---");
            foreach (var kvp in Instance.actionCache)
            {
                var enabled = kvp.Value.enabled ? "✓" : "✗";
                Debug.Log($"  {enabled} {kvp.Key.actionName} ({kvp.Key.hand})");
            }
            
            Debug.Log("=============================================");
        }
        
        private static int GetInputChannel(InputDefinition input)
        {
            if (input is FloatInputDefinition floatInput)
                return floatInput.Channel;
            if (input is Vector2InputDefinition vec2Input)
                return vec2Input.Channel;
            if (input is BoolInputDefinition boolInput)
                return boolInput.Channel;
            return 0;
        }
        
        /// <summary>
        /// Teste si une action est disponible et activée
        /// </summary>
        public bool TestAction(string actionName, XRHandSide hand)
        {
            var action = GetAction(actionName, hand);
            if (action == null)
            {
                Debug.LogWarning($"[XRInputRouter] Action '{actionName}' not found for {hand} hand");
                return false;
            }
            
            Debug.Log($"[XRInputRouter] Action '{actionName}' ({hand}): Enabled={action.enabled}, Control={action.activeControl?.name ?? "None"}");
            return action.enabled;
        }
        
    }
}
