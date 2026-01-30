using System.Collections;
using System.Collections.Generic;
using Louis.Core.Input;
using Louis.Core.Utils.Positioning;
using Louis.XR.Interactions.Input;
using Louis.XR.Interactions.Utils.Rig;
using UnityEngine;

namespace Louis.XR.Interactions.Inventory
{
    /// <summary>
    /// Contrôleur pour une étagère d'inventaire en XR
    /// </summary>
    public class InventoryShelfController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform shelf;

        [Header("Positioning")]
        [Tooltip("Configuration de positionnement (Create > Core > Positioning Config)")]
        [SerializeField] private RelativePositioningConfig positioningConfig;

        [Tooltip("Utiliser la caméra (tête) plutôt que le rig ?")]
        [SerializeField] private bool useHeadReference = true;

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private AnimationCurve appearCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Input")]
        [SerializeField] private XRHandSide handType = XRHandSide.Left;

        [SerializeField] private BoolInputDefinition toggleInventoryInput;

        private bool isVisible = false;
        private float animationTimer = 0f;
        private Vector3 hiddenScale = Vector3.zero;
        private Vector3 visibleScale = Vector3.one;

        private List<InventoryShelfSlot> shelfSlots = new List<InventoryShelfSlot>();

        private Transform ReferenceTransform =>
            XRRigReference.Instance != null
                ? (useHeadReference ? XRRigReference.Instance.Camera : XRRigReference.Instance.Rig)
                : null;

        void OnEnable()
        {
            if (toggleInventoryInput != null)
            {
                // Définir la main (0=Left, 1=Right)
                toggleInventoryInput.SetChannel((int)handType);
                toggleInventoryInput.OnPressed += OnToggleInventory;
            }
        }

        void OnDisable()
        {
            if (toggleInventoryInput != null)
            {
                toggleInventoryInput.OnPressed -= OnToggleInventory;
            }
        }

        private void Awake()
        {
            if (shelf == null)
                shelf = transform;

            if(shelfSlots.Count == 0)
                shelfSlots.AddRange(shelf.GetComponentsInChildren<InventoryShelfSlot>());

            visibleScale = shelf.localScale;

            if (positioningConfig == null)
            {
                Debug.LogWarning($"[InventoryShelfController] Aucune config sur {gameObject.name}. Utilisation de valeurs par défaut.");
            }

            shelf.localScale = hiddenScale;
            
            // Snap initial
            var reference = ReferenceTransform;
            if (reference != null)
            {
                RelativePositioning.SnapToPosition(shelf, reference, positioningConfig);
            }
        }

        private void Update()
        {
            // Animation d'échelle
            if (animationTimer < animationDuration)
            {
                animationTimer += Time.deltaTime;
                float t = Mathf.Clamp01(animationTimer / animationDuration);
                float curveValue = appearCurve.Evaluate(t);
                
                shelf.localScale = Vector3.Lerp(
                    isVisible ? hiddenScale : visibleScale,
                    isVisible ? visibleScale : hiddenScale,
                    curveValue
                );
            }
        }

        private void OnToggleInventory()
        {

            isVisible = !isVisible;
            animationTimer = 0f;

            shelfSlots.ForEach(slot => slot.SetActive(isVisible));

            if (isVisible)
            {
                var reference = ReferenceTransform;
                if (reference != null)
                {
                    RelativePositioning.SnapToPosition(shelf, reference, positioningConfig);
                }
                
            }
        }

        private void OnDrawGizmosSelected()
        {
            var reference = ReferenceTransform;
            if (reference != null && positioningConfig != null)
            {
                RelativePositioning.DrawGizmos(reference, positioningConfig, Color.cyan);
            }
        }
    }
}
