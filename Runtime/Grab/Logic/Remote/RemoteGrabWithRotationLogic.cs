using UnityEngine;
using Louis.XR.Interactions.Grab;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Louis.XR.Interactions.Input;
using Louis.XR.Interactions.Grab.Logic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Louis.Core.Input;
using Louis.XR.Interactions.Utils.Anchors;

public class RemoteGrabWithRotationLogic : XRRemoteLogicBase
{
    [Header("Input Definitions")]
    [SerializeField] private Vector2InputDefinition joystickInput;
    [SerializeField] private BoolInputDefinition toggleRotationInput;
    [SerializeField] private BoolInputDefinition resetInput;

    [Header("Settings")]
    public bool isRotating = false;
    public float speed = 1f;
    public float rotationSpeed = 100.0f; // Degrés par seconde

    [Header("Runtime State")]
    private float currentDistance; // Stocke la distance courante
    private bool wasButtonPressed = false; // Pour détecter l'edge du bouton

    // Stocker les valeurs originales pour les restaurer
    private XRBaseInteractable.MovementType originalMovementType;
    private bool originalTrackRotation;

    public override bool OnSelectEntering(XRContext context)
    {
        base.OnSelectEntering(context);

        // IMPORTANT: Pousser le contexte "Grab" pour désactiver les autres inputs (shelf, etc.)
        // Index 4 = "Grab" selon InputContextSettings
        XRInputRouter.PushContext(4);

        // Initialiser la distance courante au moment du grab
        currentDistance = Vector3.Distance(context.interactor.transform.position, context.interactable.transform.position);

        // Sauvegarder les valeurs originales
        originalMovementType = context.interactable.movementType;
        originalTrackRotation = context.interactable.trackRotation;

        // Debug des paramètres qui pourraient bloquer la rotation
        Debug.Log($"[RemoteGrabWithRotation] trackRotation: {context.interactable.trackRotation}");
        Debug.Log($"[RemoteGrabWithRotation] movementType: {context.interactable.movementType}");
        Debug.Log($"[RemoteGrabWithRotation] throwOnDetach: {context.interactable.throwOnDetach}");

        var rb = context.interactable.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"[RemoteGrabWithRotation] Rigidbody constraints: {rb.constraints}");
            Debug.Log($"[RemoteGrabWithRotation] Rigidbody isKinematic: {rb.isKinematic}");
            Debug.Log($"[RemoteGrabWithRotation] Rigidbody useGravity: {rb.useGravity}");
        }
        else
        {
            Debug.LogWarning("[RemoteGrabWithRotation] No Rigidbody found!");
        }

        return true;
    }

    public override void Process(XRContext context)
    {
        if (context.interactor is not XRRayInteractor) return;

        // Validation des InputDefinitions
        if (joystickInput == null || toggleRotationInput == null || resetInput == null)
        {
            Debug.LogWarning("[RemoteGrabWithRotation] InputDefinitions not assigned in Inspector!");
            return;
        }

        // Lire les inputs avec le channel dynamique depuis context.hand
        // Le claiming est automatique avec la priorité configurée dans l'Inspector
        // context.hand est un HandUsage (Left=1, Right=2) donc on doit convertir en XRHandSide (Left=0, Right=1)
        int channel = (int)context.hand - 1; // Left(1)→0, Right(2)→1
        
        // Sécurité : vérifier que le channel est valide
        if (channel < 0 || channel > 1)
        {
            Debug.LogWarning($"[RemoteGrabWithRotation] Invalid channel {channel} from hand {context.hand}");
            return;
        }
        
        Vector2 joystickValue = joystickInput.GetValue(channel);
        bool buttonPressed = toggleRotationInput.GetValue(channel);
        bool joystickClicked = resetInput.GetValue(channel);

        // Détection de l'edge (transition de non-pressé à pressé)
        if (buttonPressed && !wasButtonPressed)
        {
            Debug.Log("Toggle rotation mode");
            isRotating = !isRotating;

            // Désactiver trackRotation une seule fois au premier toggle
            // On ne le réactive PAS pendant le grab pour préserver les rotations
            if (isRotating && context.interactable.trackRotation)
            {
                context.interactable.trackRotation = false;
                Debug.Log("[RemoteGrabWithRotation] Rotation mode enabled - trackRotation disabled");
            }
        }
        wasButtonPressed = buttonPressed; // Mémoriser l'état pour la prochaine frame

        // Handle joystick click to reset immediately
        if (joystickClicked)
        {
            ResetTransform(context);
        }

        // Apply transformations based on current mode
        if (isRotating)
        {
            // Joystick Y (haut/bas) → rotation sur axe X local (pitch)
            // Joystick X (gauche/droite) → rotation sur axe Y WORLD (yaw)
            float pitchRotation = -joystickValue.y * rotationSpeed * context.deltaTime;
            float yawRotation = joystickValue.x * rotationSpeed * context.deltaTime;

            Debug.Log($"Applying rotation - Pitch: {pitchRotation}, Yaw: {yawRotation}");

            // Appliquer les rotations en utilisant des axes fixes:
            // - Yaw autour de l'axe Y world (toujours vertical)
            // - Pitch autour de l'axe X local de l'objet (pour rotation naturelle)
            context.interactable.transform.Rotate(Vector3.up, yawRotation, Space.World);
            context.interactable.transform.Rotate(Vector3.right, pitchRotation, Space.Self);
        }
        else
        {
            // Accumuler la distance basée sur l'input vertical
            currentDistance += joystickValue.y * speed * context.deltaTime;
            currentDistance = Mathf.Max(0.1f, currentDistance); // Empêcher distance négative

            // Calculer la nouvelle position à la distance stockée
            Vector3 direction = (context.interactable.transform.position - context.interactor.transform.position).normalized;
            Vector3 targetPosition = context.interactor.transform.position + direction * currentDistance;

            context.interactable.transform.position = targetPosition;
        }
    }

    private void ResetTransform(XRContext context)
    {
        isRotating = false;

        // Restaurer les paramètres originaux
        context.interactable.trackRotation = originalTrackRotation;
        context.interactable.movementType = originalMovementType;

        context.interactable.transform.localScale = Vector3.one;
        context.interactable.transform.localRotation = Quaternion.identity;

        Debug.Log("[RemoteGrabWithRotation] Transform reset - settings restored");
    }

    public override void OnSelectExited(XRContext context)
    {
        base.OnSelectExited(context);
        isRotating = false;

        // IMPORTANT: Retirer le contexte "Grab" pour réactiver les autres inputs
        XRInputRouter.PopContext();

        // Restaurer les paramètres originaux au release
        context.interactable.trackRotation = originalTrackRotation;
        context.interactable.movementType = originalMovementType;

        Debug.Log("[RemoteGrabWithRotation] Object released - settings restored");
    }
}