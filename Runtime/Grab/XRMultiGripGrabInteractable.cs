using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Louis.XR.Interactions.Tools
{
    [DisallowMultipleComponent]
    public class XRMultiGripGrabInteractable : XRGrabInteractable
    {
        [System.Serializable]
        private struct GripConfig
        {
            [Tooltip("Rotation locale COMPLETE de l'attachTransform pour ce grip (Euler degrés).")]
            public Vector3 localAttachEuler;
        }

        [Header("Grips")]
        [SerializeField] private GripConfig[] grips;

        [Header("Debug")]
        [SerializeField] private bool verboseDebug = false;

        private Quaternion[] _gripLocalRotations;
        private Quaternion _baseLocalRotation;

        protected override void Awake()
        {
            base.Awake();
            if (attachTransform == null)
                attachTransform = transform;

            _baseLocalRotation = attachTransform.localRotation;
            CacheGrips();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (attachTransform == null)
                attachTransform = transform;

            _baseLocalRotation = attachTransform.localRotation;
            CacheGrips();
        }
#endif

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            // 1) Reset avant calcul
            attachTransform.localRotation = _baseLocalRotation;

            // 2) Récup rotation monde de la main
            Transform handAttach = args.interactorObject.GetAttachTransform(this);
            Quaternion handWorldRot = handAttach.rotation;

            // 3) Rotation de la main dans le repère de l'objet
            Quaternion objectWorldRot = transform.rotation;
            Quaternion handLocal = Quaternion.Inverse(objectWorldRot) * handWorldRot;

            // 4) Sélection du grip le plus proche
            int bestIndex = -1;
            float bestAngle = float.MaxValue;

            for (int i = 0; i < _gripLocalRotations.Length; i++)
            {
                float angle = Quaternion.Angle(handLocal, _gripLocalRotations[i]);
                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    bestIndex = i;
                }
            }

            if (bestIndex >= 0)
            {
                // 5) appliquer la rotation locale AVANT base.OnSelectEntering
                attachTransform.localRotation = _gripLocalRotations[bestIndex];

                if (verboseDebug)
                {
                    Debug.Log($"[XRMultiGrip] '{name}' grip={bestIndex} angle={bestAngle:F1}°");
                }
            }

            // 6) maintenant XRGrab calcule avec cet attach, et TOUT FONCTIONNE
            base.OnSelectEntering(args);
        }

        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            base.OnSelectExiting(args);
            attachTransform.localRotation = _baseLocalRotation;
        }

        private void CacheGrips()
        {
            if (grips == null || grips.Length == 0)
            {
                _gripLocalRotations = new Quaternion[0];
                return;
            }

            _gripLocalRotations = new Quaternion[grips.Length];

            for (int i = 0; i < grips.Length; i++)
                _gripLocalRotations[i] = Quaternion.Euler(grips[i].localAttachEuler);
        }
    }
}
