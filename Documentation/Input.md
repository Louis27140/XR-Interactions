# Système d'Input & Haptique

Routage centralisé des inputs XR et gestion avancée du feedback haptique.

## Architecture Input

Le système repose sur un **Singleton** `XRInputRouter` qui centralise la lecture des actions Unity Input System et offre une interface simplifiée aux autres scripts. Il gère également une pile de contextes (Context Stack) pour résoudre les conflits d'inputs.

```mermaid
flowchart TB
    subgraph "Unity Input System"
        IAA[InputActionAsset]
        IAM[InputActionManager]
    end
    
    subgraph "XRInputRouter (Singleton)"
        AC[Action Cache]
        CS[Context Stack]
        API[Input API]
    end
    
    subgraph "Core.Input"
        ID[InputDefinition]
        FID[FloatInputDefinition]
        VID[Vector2InputDefinition]
        BID[BoolInputDefinition]
    end
    
    subgraph "Consumers"
        TM[TeleportationManager]
        GL[GrabLogic]
        UI[UI Navigation]
    end
    
    IAA --> IAM
    IAM --> AC
    AC --> API
    API --> ID
    ID --> FID
    ID --> VID
    ID --> BID
    FID --> TM
    VID --> GL
    BID --> UI
    CS -.->|"IsContextValid()"| ID
```

### Context Stack (Gestion des conflits)

Le Context Stack permet de bloquer dynamiquement certains inputs selon l'état de l'application (ex: bloquer la locomotion quand on manipule un objet).

```mermaid
sequenceDiagram
    participant User
    participant Grab as RemoteGrabLogic
    participant Router as XRInputRouter
    participant Teleport as TeleportationManager
    
    Note over Router: Stack: [Default]
    User->>Teleport: Joystick Up
    Teleport->>Router: ReadVector2("Move")
    Router-->>Teleport: ✓ Vector2(0, 0.8)
    
    User->>Grab: Grab object
    Grab->>Router: PushContext("Grab")
    Note over Router: Stack: [Default, Grab]
    
    User->>Teleport: Joystick Up
    Teleport->>Router: ReadVector2("Move")
    Note over Router: Teleport contextMask<br/>n'inclut pas "Grab"
    Router-->>Teleport: ✗ Vector2.zero (blocked)
    
    User->>Grab: Release object
    Grab->>Router: PopContext()
    Note over Router: Stack: [Default]
    Teleport->>Router: ReadVector2("Move")
    Router-->>Teleport: ✓ (works again)
```

## Système Haptique

Système de feedback vibratoire supportant des impulsions simples et des patterns complexes via des courbes d'animation.

```mermaid
classDiagram
    class XRHaptic {
        <<MonoBehaviour Singleton>>
        +XRHaptic Instance$
        +XRHapticPreset grabPreset
        +XRHapticPreset releasePreset
        +XRHapticPreset hoverPreset
        +SendImpulse(hand, amplitude, duration) void
        +SendImpulse(hand, preset) void
    }
    
    class XRHapticPreset {
        <<ScriptableObject>>
        +AnimationCurve amplitudeCurve
        +float duration
        +float amplitudeMultiplier
        +bool loop
    }
    
    XRHaptic --> XRHapticPreset : uses
```

### XRHapticPreset

Un ScriptableObject définissant une "sensation" haptique.
- `amplitudeCurve` : Permet de dessiner la variation d'intensité de la vibration dans le temps (ex: battement de coeur, impact sec, montée progressive).
- `loop` : Si vrai, la vibration se répète (utile pour minigun, moteur, etc.).

### Utilisation

```csharp
// Vibration simple
XRHaptic.Instance.SendImpulse(XRHandSide.Right, 0.5f, 0.1f);

// Vibration via Preset
[SerializeField] private XRHapticPreset recoilPreset;
XRHaptic.Instance.SendImpulse(XRHandSide.Right, recoilPreset);
```
