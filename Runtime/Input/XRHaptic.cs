using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace Louis.XR.Interactions.Input.Feedback
{
    /// <summary>
    /// Gestionnaire centralisé pour le feedback haptique XR
    /// Utilise XRInputRouter pour récupérer les devices haptiques
    /// Adapté pour XR Interaction Toolkit 3.2.2+
    /// </summary>
    public class XRHaptic : MonoBehaviour
    {
        public static XRHaptic Instance { get; private set; }

        [Header("Presets par Défaut")]
        [Tooltip("Preset utilisé lors du grab d'un objet")]
        [SerializeField] private XRHapticPreset onGrabPreset;
        
        [Tooltip("Preset utilisé lors du release d'un objet")]
        [SerializeField] private XRHapticPreset onReleasePreset;
        
        [Tooltip("Preset utilisé lors du hover sur un objet")]
        [SerializeField] private XRHapticPreset onHoverPreset;
        
        [Tooltip("Preset utilisé lors d'une collision")]
        [SerializeField] private XRHapticPreset onCollisionPreset;
        
        [Tooltip("Preset utilisé lors d'un appui sur un bouton")]
        [SerializeField] private XRHapticPreset onButtonPressPreset;

        /// <summary>
        /// Type d'actuateur haptique sur le contrôleur
        /// </summary>
        public enum HapticActuator
        {
            Global,      // Vibreur principal du contrôleur
            Trigger,     // Vibreur dans la gâchette (si disponible)
            Thumbstick   // Vibreur dans le stick (si disponible)
        }

        // Coroutines actives pour chaque main
        private Dictionary<XRHandSide, Coroutine> activePatterns = new Dictionary<XRHandSide, Coroutine>();

        // Cache des players XRI (Nouveau dans XRI 3.x)
        private Dictionary<XRHandSide, HapticImpulsePlayer> hapticPlayers = new Dictionary<XRHandSide, HapticImpulsePlayer>();

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialiser le dictionnaire
            activePatterns[XRHandSide.Left] = null;
            activePatterns[XRHandSide.Right] = null;
            hapticPlayers[XRHandSide.Left] = null;
            hapticPlayers[XRHandSide.Right] = null;
        }

        /// <summary>
        /// Récupère ou trouve le HapticImpulsePlayer pour la main donnée (API XRI 3.x)
        /// </summary>
        private HapticImpulsePlayer GetHapticPlayer(XRHandSide hand)
        {
            if (hapticPlayers[hand] != null) return hapticPlayers[hand];

            // Recherche basique par convention de nom ou composant XRBaseController
            var controllers = FindObjectsOfType<TrackedPoseDriver>();
            foreach (var controller in controllers)
            {
                // Heuristique simple basée sur le nom si XRNode pas dispo facilement
                string name = controller.name.ToLower();
                bool isLeft = name.Contains("left");
                bool isRight = name.Contains("right");

                if ((hand == XRHandSide.Left && isLeft) || (hand == XRHandSide.Right && isRight))
                {
                    var player = controller.GetComponent<HapticImpulsePlayer>();
                    // Ajouter le composant s'il manque (setup auto)
                    if (player == null) player = controller.gameObject.AddComponent<HapticImpulsePlayer>();
                    
                    hapticPlayers[hand] = player;
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// Envoie une impulsion haptique simple via XRInputRouter
        /// </summary>
        /// <param name="hand">Main cible (gauche/droite)</param>
        /// <param name="amplitude">Intensité de la vibration (0-1)</param>
        /// <param name="duration">Durée en secondes</param>
        /// <param name="actuator">Type d'actuateur à utiliser</param>
        public void SendHapticImpulse(XRHandSide hand, float amplitude, float duration, HapticActuator actuator = HapticActuator.Global)
        {
            if (actuator == HapticActuator.Global)
            {
                var player = GetHapticPlayer(hand);
                if (player != null)
                {
                    player.SendHapticImpulse(Mathf.Clamp01(amplitude), duration);
                    return; // Géré par XRI, on sort
                }
            }

            // 2. Fallback sur XRInputRouter / InputSystem pour les cas spécifiques (Trigger/Thumbstick) ou si XRI échoue
            if (XRInputRouter.Instance == null)
            {
                Debug.LogWarning("[XRHaptic] XRInputRouter.Instance est null");
                return;
            }

            // Récupérer le device selon l'actuateur demandé
            InputDevice device = actuator switch
            {
                HapticActuator.Trigger => XRInputRouter.Instance.GetHapticTrigger(hand),
                HapticActuator.Thumbstick => XRInputRouter.Instance.GetHapticThumbstick(hand),
                _ => XRInputRouter.Instance.GetHapticDevice(hand)
            };

            if (device == default) return;

            // Utilisation directe de l'interface IHaptics de l'Input System
            if (device is IXRHapticImpulseChannel haptics)
            {
                haptics.SendHapticImpulse(Mathf.Clamp01(amplitude), Mathf.Clamp01(amplitude));
                StartCoroutine(StopHapticAfterDelay(haptics, duration));
            }
        }

        private IEnumerator StopHapticAfterDelay(IXRHapticImpulseChannel haptics, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (haptics != null)
            {

                haptics.SendHapticImpulse(0f, 0f);
            }
        }

        /// <summary>
        /// Joue un preset haptique
        /// </summary>
        /// <param name="preset">Preset ScriptableObject à jouer</param>
        /// <param name="hand">Main cible</param>
        public void PlayPreset(XRHapticPreset preset, XRHandSide hand)
        {
            if (preset == null)
            {
                Debug.LogWarning("[XRHaptic] Preset null fourni");
                return;
            }

            // Arrêter tout pattern en cours sur cette main
            StopPattern(hand);

            if (preset.usePattern && preset.intensityCurve != null && preset.intensityCurve.keys.Length > 1)
            {
                // Jouer pattern complexe avec courbe d'intensité
                activePatterns[hand] = StartCoroutine(PlayPatternCoroutine(hand, preset));
            }
            else
            {
                // Impulsion simple
                SendHapticImpulse(hand, preset.amplitude, preset.duration, preset.actuator);
            }
        }
        
        /// <summary>
        /// Joue un pattern haptique avec courbe d'intensité personnalisée
        /// </summary>
        public void PlayPattern(XRHandSide hand, AnimationCurve intensityCurve, float duration, HapticActuator actuator = HapticActuator.Global)
        {
            StopPattern(hand);
            activePatterns[hand] = StartCoroutine(PlayPatternCoroutine(hand, intensityCurve, duration, actuator));
        }

        private IEnumerator PlayPatternCoroutine(XRHandSide hand, XRHapticPreset preset)
        {
            yield return PlayPatternCoroutine(hand, preset.intensityCurve, preset.duration, preset.actuator);
        }

        private IEnumerator PlayPatternCoroutine(XRHandSide hand, AnimationCurve curve, float duration, HapticActuator actuator)
        {
            float elapsed = 0f;
            const float sampleRate = 0.02f; // 50 Hz pour smoothness

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float amplitude = curve.Evaluate(t);
                
                SendHapticImpulse(hand, amplitude, sampleRate, actuator);
                
                elapsed += sampleRate;
                yield return new WaitForSeconds(sampleRate);
            }

            activePatterns[hand] = null;
        }
        
        /// <summary>
        /// Arrête le pattern haptique en cours sur une main
        /// </summary>
        public void StopPattern(XRHandSide hand)
        {
            if (activePatterns[hand] != null)
            {
                StopCoroutine(activePatterns[hand]);
                activePatterns[hand] = null;
            }
            
            // Arrêter toute vibration résiduelle
            SendHapticImpulse(hand, 0f, 0f);
        }

        /// <summary>
        /// Arrête tous les patterns actifs
        /// </summary>
        public void StopAllPatterns()
        {
            StopPattern(XRHandSide.Left);
            StopPattern(XRHandSide.Right);
        }

        /// <summary>
        /// Envoie une impulsion aux deux mains simultanément
        /// </summary>
        public void SendHapticImpulseBothHands(float amplitude, float duration, HapticActuator actuator = HapticActuator.Global)
        {
            SendHapticImpulse(XRHandSide.Left, amplitude, duration, actuator);
            SendHapticImpulse(XRHandSide.Right, amplitude, duration, actuator);
        }

        /// <summary>
        /// Joue un preset sur les deux mains
        /// </summary>
        public void PlayPresetBothHands(XRHapticPreset preset)
        {
            PlayPreset(preset, XRHandSide.Left);
            PlayPreset(preset, XRHandSide.Right);
        }

        // -------------------------------------------------------
        //  PRESETS PAR DÉFAUT (helpers publics)
        // -------------------------------------------------------

        /// <summary>
        /// Joue le feedback de grab
        /// </summary>
        public void PlayGrabFeedback(XRHandSide hand)
        {
            if (onGrabPreset != null)
                PlayPreset(onGrabPreset, hand);
        }

        /// <summary>
        /// Joue le feedback de release
        /// </summary>
        public void PlayReleaseFeedback(XRHandSide hand)
        {
            if (onReleasePreset != null)
                PlayPreset(onReleasePreset, hand);
        }

        /// <summary>
        /// Joue le feedback de hover
        /// </summary>
        public void PlayHoverFeedback(XRHandSide hand)
        {
            if (onHoverPreset != null)
                PlayPreset(onHoverPreset, hand);
        }

        /// <summary>
        /// Joue le feedback de collision
        /// </summary>
        public void PlayCollisionFeedback(XRHandSide hand)
        {
            if (onCollisionPreset != null)
                PlayPreset(onCollisionPreset, hand);
        }

        /// <summary>
        /// Joue le feedback de bouton
        /// </summary>
        public void PlayButtonPressFeedback(XRHandSide hand)
        {
            if (onButtonPressPreset != null)
                PlayPreset(onButtonPressPreset, hand);
        }

        private void OnDestroy()
        {
            StopAllPatterns();
        }
    }
}
