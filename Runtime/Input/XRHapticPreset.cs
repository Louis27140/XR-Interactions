using System;
using UnityEngine;

namespace Louis.XR.Interactions.Input.Feedback
{
    /// <summary>
    /// Preset réutilisable pour feedback haptique XR
    /// Créer via: Create > XR Interactions/Haptic Preset
    /// </summary>
    
    [CreateAssetMenu(fileName = "HapticPreset", menuName = "XR Interactions/Haptic Preset")]
    public class XRHapticPreset : ScriptableObject
    {
        [Header("Paramètres de Base")]
        [Tooltip("Intensité de la vibration (0-1)")]
        [Range(0f, 1f)]
        public float amplitude = 0.5f;
        
        [Tooltip("Durée de la vibration (secondes)")]
        [Range(0f, 1f)]
        public float duration = 0.1f;

        [Tooltip("Type d'actuateur haptique à utiliser")]
        public XRHaptic.HapticActuator actuator = XRHaptic.HapticActuator.Global;

        [Header("Pattern Complexe")]
        [Tooltip("Utiliser une courbe d'intensité au lieu d'une impulsion simple ?")]
        public bool usePattern = false;
        
        [Tooltip("Courbe d'intensité dans le temps (X: 0-1 = temps normalisé, Y: 0-1 = amplitude)")]
        public AnimationCurve intensityCurve = AnimationCurve.Linear(0, 1, 1, 0);

        [Header("Métadonnées")]
        [Tooltip("Nom descriptif du preset")]
        public string presetName = "Default";
        
        [Tooltip("Description de l'usage recommandé")]
        [TextArea(2, 4)]
        public string description;

        /// <summary>
        /// Joue ce preset sur une main spécifique
        /// </summary>
        public void Play(XRHandSide hand)
        {
            XRHaptic.Instance?.PlayPreset(this, hand);
        }

        /// <summary>
        /// Joue ce preset sur les deux mains
        /// </summary>
        public void PlayBothHands()
        {
            XRHaptic.Instance?.PlayPresetBothHands(this);
        }

        #if UNITY_EDITOR
        [ContextMenu("Test - Main Gauche")]
        private void TestLeft()
        {
            if (Application.isPlaying)
                Play(XRHandSide.Left);
            else
                Debug.LogWarning("Le test nécessite le Play Mode");
        }

        [ContextMenu("Test - Main Droite")]
        private void TestRight()
        {
            if (Application.isPlaying)
                Play(XRHandSide.Right);
            else
                Debug.LogWarning("Le test nécessite le Play Mode");
        }

        [ContextMenu("Test - Deux Mains")]
        private void TestBoth()
        {
            if (Application.isPlaying)
                PlayBothHands();
            else
                Debug.LogWarning("Le test nécessite le Play Mode");
        }
        #endif
    }
}
