# Louis - XR Interactions

## Description

Package Unity fournissant un système d'interactions XR avancé basé sur le XR Interaction Toolkit de Unity. Ce package étend les fonctionnalités de base avec des systèmes de grab personnalisables, de la téléportation, de la gestion des mains, du clavier VR et bien plus.

## Version

**0.1.0**

## Prérequis

- Unity 2022.3 ou supérieur
- XR Interaction Toolkit 3.2.2
- com.louis.xr.core 0.1.0

## Installation

1. Installer d'abord le package `com.louis.xr.core`
2. Ouvrir le Package Manager dans Unity
3. Sélectionner "Add package from disk..."
4. Naviguer vers le fichier `package.json` de ce package

## ⚡ Démarrage Rapide

Ce package étend **Unity XR Interaction Toolkit** avec des **interactions VR avancées** :

- 🎮 **Grab System** : Système modulaire avec 3 types de grab (direct, remote instant, remote Half-Life Alyx)
- 🎚️ **Anchors** : Points de snap précis avec rôles et compatibilité main
- 🎯 **Input Router** : Routage centralisé des inputs XR (singleton)
- 🖐️ **Hands** : Animation procédurale des mains VR
- 🚀 **Teleportation** : Locomotion par joystick
- 📦 **Inventory XR** : Intégration VR de l'inventaire
- 🔧 **Tools XR** : Gestion d'outils activables par gâchette

**Dépend de :** [com.louis.xr.core](../com.louis.xr.core/README.md) pour les systèmes fondamentaux (Inventory, Tools, etc.)

### Exemple Rapide : Objet grabbable avec snap points

```csharp
using Louis.XR.Interactions.Grab;
using Louis.XR.Interactions.Utils.Anchors;

// 1. Ajouter XRGenericInteractable à votre objet
// 2. Ajouter un Rigidbody
// 3. Créer des GameObjects enfants avec XRAnchor pour les snap points
// 4. Configurer les logiques de grab

public class GrabSetup : MonoBehaviour
{
    void Start()
    {
        var interactable = GetComponent<XRGenericInteractable>();

        // Grab direct avec snap sur anchors
        interactable.directLogic = new DirectAnchorLogic
        {
            maxSnapDistance = 0.12f,
            useAngle = true
        };

        // Grab à distance style Half-Life Alyx
        interactable.remoteLogic = new RemoteHLALogic
        {
            travelTime = 0.35f,
            isAutoGrabbed = true,
            useArc = true
        };
    }
}
```

➡️ **Voir les sections détaillées ci-dessous pour comprendre l'architecture modulaire.**

## Fonctionnalités

Ce package étend Unity XR Interaction Toolkit avec des systèmes avancés organisés de manière modulaire :

- 🎮 **Grab System** : Grab modulaire avec logiques interchangeables (Strategy Pattern)
- 🎚️ **Anchors** : Snap points avec rôles, compatibilité main, priorités
- 🎯 **Input Router** : Centralisation des inputs XR avec callbacks
- 🖐️ **Hands** : Animation procédurale (Grip, Trigger, Grab)
- 🚀 **Teleportation** : Locomotion joystick avec seuils configurables
- 📦 **Inventory XR** : Slots d'inventaire avec sockets XR
- 🔧 **Tools XR** : Bridge ITriggerTool vers XR Input
- ⌨️ **Keyboard XR** : Positionnement automatique du clavier face caméra
- 🐛 **Debug XR** : Activation du debugger par combinaison de boutons

**Dépend de :** [com.louis.xr.core](../com.louis.xr.core/README.md) pour Inventory, Keyboard, Tools et Debug.

### 🎮 Système de Grab Avancé (Grab)

Système modulaire et extensible pour la manipulation d'objets en VR avec support de multiples logiques d'interaction.

#### Architecture

**XRGenericInteractable**
- Interactable universel supportant plusieurs modes de grab
- Hérite de `XRGrabInteractable`
- Support pour :
  - Direct Grab (grab direct)
  - Remote Grab (grab à distance)
  - Socket (placement dans inventaire)
- Propriétés :
  - `directLogic` : Logique pour le grab direct
  - `remoteLogic` : Logique pour le grab à distance
  - `socketLogic` : Logique pour les sockets
  - `useHandle` : Utilisation de poignées
- Gestion automatique des anchors enfants
- Validation des Rigidbody et Colliders

#### Logiques Direct Grab

**XRDirectLogicBase** (Classe abstraite)
- Classe de base pour les logiques de grab direct
- Méthodes virtuelles :
  - `CanSelect(XRContext)` : Détermine si l'objet peut être sélectionné
  - `OnSelectEntering(XRContext)` : Avant la sélection
  - `OnSelectEntered(XRContext)` : Après la sélection
  - `OnSelectExiting(XRContext)` : Avant le relâchement
  - `OnSelectExited(XRContext)` : Après le relâchement
  - `Process(XRContext)` : Traitement continu
  - `OnFixedUpdate(XRContext)` : Mise à jour physique

**DirectLogic**
- Implémentation standard du grab direct
- Utilise le comportement par défaut du XR Interaction Toolkit
- Aucune modification de l'attach ou de la pose

**DirectAnchorLogic**
- Grab direct avec snap sur anchors
- Propriétés :
  - `maxSnapDistance` : Distance maximale de snap (défaut: 0.12m)
  - `useAngle` : Utilise l'angle pour le snap
  - `maxAngle` : Angle maximal pour le snap (défaut: 60°)
  - `angleWeight` : Poids de l'angle dans le calcul (0-1)
- Sélection automatique du meilleur anchor selon :
  - Distance
  - Rotation (optionnel)
  - Main (gauche/droite)
  - Priorité de l'anchor

#### Logiques Remote Grab

**XRRemoteLogicBase** (Classe abstraite)
- Classe de base pour les logiques de grab à distance
- Méthodes identiques à XRDirectLogicBase

**RemoteInstantLogic**
- Grab instantané : l'objet vient immédiatement dans la main
- Processus :
  1. Détection du ray grab
  2. Téléportation de l'objet vers la main
  3. Transfert automatique vers le Direct Interactor
- Idéal pour ramasser rapidement des objets

**RemoteHLALogic** (Hand Launch Arc)
- Grab avec lancement et trajectoire arc
- Propriétés :
  - `velocityThreshold` : Seuil de vélocité pour le lancement (défaut: 2 m/s)
  - `travelTime` : Temps de vol (défaut: 0.35s)
  - `isAutoGrabbed` : Grab automatique à l'arrivée
  - `autoGrabDistance` : Distance pour auto-grab (défaut: 0.2m)
  - `useArc` : Utilise une trajectoire en arc
  - `arcHeight` : Hauteur de l'arc (défaut: 0.2m)
- Fonctionnalités :
  - Vol parabolique vers la main
  - Détection de vélocité du ray pour déclencher
  - Auto-grab quand l'objet arrive près de la main
  - Gestion de la physique pendant le vol

#### Logiques Socket

**XRSocketLogicBase** (Classe abstraite)
- Classe de base pour les logiques de socket

**SocketAnchorLogic**
- Placement dans socket avec snap sur anchor d'inventaire
- Fonctionnalités :
  - Sélection automatique de l'anchor d'inventaire
  - Restauration de l'anchor par défaut au retrait
  - Validation des anchors disponibles

#### Composants Supplémentaires

**XRContext**
- Structure de données partagée entre les logiques
- Contient :
  - Références aux interactors (direct, remote, socket)
  - Transform et position de l'interactor
  - Liste des anchors disponibles
  - Main utilisée (gauche/droite)
  - Manager d'interaction
  - Delta time pour les calculs physiques

**XRHandleInteractable**
- Gestion de poignées pour objets complexes

**XRInteractorController**
- Contrôleur pour les interactors XR

---

### 🖐️ Système de Mains (Hands)

Animation et contrôle procédural des mains virtuelles.

#### Classes principales

**HandController**
- Contrôleur de main basé sur `ActionBasedController`
- Synchronisation automatique avec :
  - Grip (valeur de saisie)
  - Trigger (gâchette)
  - État de grab
- Mise à jour temps réel des poses de main

**Hand**
- Représentation de la main virtuelle
- Gestion des animations procédurales
- Méthodes :
  - `SetGrip(float)` : Position des doigts pour la saisie
  - `SetTrigger(float)` : Position de l'index
  - `SetGrab(bool)` : État de grab actif

---

### 🎯 Système d'Input (Input)

Routage centralisé des inputs XR.

#### Classes principales

**XRInputRouter** (Singleton)
- Router centralisé pour tous les inputs XR
- Support pour les deux mains (gauche/droite)
- Actions disponibles :
  - **Boutons** :
    - Trigger (Activate)
    - Grip (Select)
    - Primary Button
    - Secondary Button
    - Menu
  - **Joystick** :
    - Thumbstick (mouvement)
    - Thumbstick Click
  - **Touch** :
    - Trigger Touch
    - Grip Touch
    - Primary Touch
    - Secondary Touch
- Propriétés configurables :
  - Noms des maps d'actions
  - Noms des actions individuelles
- Détection automatique du `InputActionManager`
- Méthodes d'accès (par main) :
  - `TriggerValue(hand)` : Valeur du trigger
  - `GripValue(hand)` : Valeur du grip
  - `PrimaryHeld(hand)` : Bouton primaire enfoncé
  - `SecondaryHeld(hand)` : Bouton secondaire enfoncé
  - `Thumbstick(hand)` : Position du joystick
  - etc.

**XRHandSide** (Enum)
- `Left` : Main gauche
- `Right` : Main droite

---

### 📦 Système d'Inventaire XR (Inventory)

Extension du système d'inventaire core pour la VR.

#### Classes principales

**InventoryShelfSlot**
- Slot d'inventaire avec socket XR
- Fonctionnalités :
  - Placement automatique des items
  - Détection de slot plein
  - Redirection vers slot vide si occupé
  - Rejet des items si aucun slot disponible
  - Affichage du nom de l'item (TextMeshPro)
  - Synchronisation avec InventoryBehaviour
- Propriétés :
  - `socket` : XRSocketInteractor
  - `snapPoint` : Point de snap
  - `inventoryBehaviour` : Référence à l'inventaire
  - `itemName` : Affichage du nom
- Events gérés :
  - `selectEntered` : Item placé
  - `selectExited` : Item retiré

---

### ⌨️ Clavier XR (Keyboard)

Intégration du clavier virtuel avec le système XR.

#### Classes principales

**XRKeyboardManager**
- Manager de clavier pour VR
- Fonctionnalités :
  - Détection automatique des TMP_InputField
  - Positionnement du clavier face à la caméra
  - Gestion du focus des champs de saisie
  - Intégration avec XRUIInputModule
- Propriétés :
  - `xrCamera` : Caméra XR
  - `keyboardPrefab` : Prefab du clavier
  - `keyboardController` : Contrôleur du clavier
  - `distance` : Distance d'affichage (défaut: 0.5m)
  - `offset` : Décalage vertical
- Détection automatique des clics via `XRUIInputModule`

---

### 🚀 Système de Téléportation (Locomotion)

Téléportation basée sur le joystick.

#### Classes principales

**XRTeleportationManager** (anciennement TeleportationManager)
- Gestion de la téléportation pour les deux mains
- Fonctionnalités :
  - Activation par joystick vers le haut
  - Affichage du ray de téléportation
  - Désactivation automatique du ray
  - Support pour les deux mains indépendamment
- Propriétés :
  - `provider` : TeleportationProvider Unity
  - `leftHand` / `rightHand` : Configuration par main
  - `aimStartThreshold` : Seuil pour activer (défaut: 0.7)
  - `aimStopThreshold` : Seuil pour désactiver (défaut: 0.2)
- Processus :
  1. Joystick vers le haut > seuil → Affiche le ray
  2. Joystick revient en dessous du seuil → Téléporte
  3. Ray désactivé automatiquement

---

### 🔧 Outils XR (Tools)

Intégration des outils avec le système XR.

#### Classes principales

**XRTriggerToolHandler**
- Handler pour connecter ITriggerTool au système XR
- Fonctionnalités :
  - Détection automatique de la main tenant l'outil
  - Lecture de la valeur du trigger via XRInputRouter
  - Gestion des états pressé/relâché
  - Support pour les seuils de pression
- Propriétés :
  - `grab` : XRGrabInteractable
  - `pressTreshold` : Seuil de pression (défaut: 0.1)
- Events gérés :
  - `selectEntered` : Outil saisi
  - `selectExited` : Outil lâché
- Ignore les sockets pour le trigger

---

### 🎚️ Système d'Anchors (Utils/Anchors)

Système de points d'ancrage pour le grab précis.

#### Classes principales

**XRAnchor**
- Point d'ancrage pour le grab
- Propriétés :
  - `role` : Rôle de l'anchor (Grab, Inventory, SocketOverride, Any)
  - `handside` : Main compatible (None, Left, Right, Both)
  - `priority` : Priorité pour la sélection (défaut: 1.0)
  - `anchorTag` : Tag personnalisé
  - Gizmos :
    - `gizmoSphereRadius` : Rayon de la sphère (défaut: 0.01m)
    - `gizmoForwardLength` : Longueur du forward (défaut: 0.08m)
    - `gizmoUpLength` : Longueur du up (défaut: 0.05m)
    - `debugSnapRadius` : Rayon de snap visualisé (défaut: 0.12m)
- Visualisation en éditeur :
  - Couleur par main (Cyan=Left, Magenta=Right, Yellow=Both, Gray=None)
  - Sphère au centre
  - Ligne forward pour l'orientation
  - Ligne up pour le twist
  - Sphère wireframe pour la zone de snap

**AnchorsUtils**
- Utilitaires pour la sélection d'anchors
- Méthodes statiques :
  - `SelectBestAnchor()` : Sélection du meilleur anchor selon distance, angle, main
  - `SelectBestAnchorInventory()` : Sélection de l'anchor d'inventaire
- Critères de sélection :
  - Distance à l'interactor
  - Angle avec l'interactor (optionnel)
  - Compatibilité de main
  - Priorité de l'anchor
  - Rôle de l'anchor

**AnchorRole** (Enum)
- `Grab` : Pour le grab normal
- `Inventory` : Pour le placement en inventaire
- `SocketOverride` : Override pour socket
- `Any` : N'importe quel rôle

**HandUsage** (Enum)
- `None` : Pas de restriction
- `Left` : Main gauche uniquement
- `Right` : Main droite uniquement
- `Both` : Les deux mains

---

### 🐛 Debug XR (Utils/Debug)

Outils de débogage pour VR.

#### Classes principales

**XRDebugLogger**
- Activation du debugger en VR
- Fonctionnalités :
  - Toggle du debugger par combinaison de boutons
  - Maintien requis pendant une durée définie
  - Choix de la main pour l'activation
- Propriétés :
  - `holdDuration` : Durée de maintien requise (défaut: 2s)
  - `hand` : Main à utiliser (défaut: Left)
- Combinaison : Primary + Secondary buttons simultanément

---

## Samples Inclus

### 📋 Rig
- Prefab de rig XR complet
- Configuration pré-établie
- Mains animées
- Input routing configuré

### 🚪 Door
- Prefabs de portes interactives
- Exemples d'utilisation des Openables
- Physique configurée

### 🎯 Teleportation
- Prefabs de téléportation
- Zones de téléportation
- Configuration du système

### 📦 Inventory
- Prefab d'étagère d'inventaire (InventoryShelf)
- Slots d'inventaire avec sockets XR
- Intégration avec InventoryBehaviour de com.louis.xr.core
- Exemple de système complet d'inventaire VR
- Affichage des noms d'items (TextMeshPro)

## Structure du Package

```
com.louis.xr.interactions/
├── Runtime/
│   ├── Grab/              # Système de grab modulaire
│   ├── Hands/             # Animation des mains
│   ├── Input/             # Routage des inputs
│   ├── Inventory/         # Inventaire XR
│   ├── Keyboard/          # Clavier virtuel XR
│   ├── Teleport/          # Locomotion/téléportation
│   ├── Tools/             # Outils XR
│   └── Utils/
│       ├── Anchors/       # Système d'anchors
│       └── Debug/         # Debug XR
├── Editor/
│   └── Grab/              # Éditeurs custom pour grab
├── Documentation/
└── Samples~/
    ├── Rig/
    ├── Door/
    └── Teleportation/
```

## Dépendances

- **[com.louis.xr.core](../com.louis.xr.core/README.md)** : 0.1.0
  - Fournit les systèmes fondamentaux : Inventory, Keyboard, Tools, Openable, Debug
  - Utilisé par : InventoryShelfSlot, XRKeyboardManager, XRTriggerToolHandler
- **com.unity.xr.interaction.toolkit** : 3.2.2
  - Framework officiel Unity pour interactions VR
  - [Documentation Unity](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/manual/index.html)

## Utilisation

### Exemple : Configuration d'un objet grabbable

```csharp
using Louis.XR.Interactions.Grab;
using UnityEngine;

public class GrabbableSetup : MonoBehaviour
{
    void Start()
    {
        var interactable = gameObject.AddComponent<XRGenericInteractable>();
        
        // Configuration du grab direct
        interactable.directLogic = new DirectAnchorLogic
        {
            maxSnapDistance = 0.12f,
            useAngle = true,
            maxAngle = 60f
        };
        
        // Configuration du grab à distance
        interactable.remoteLogic = new RemoteHLALogic
        {
            travelTime = 0.35f,
            isAutoGrabbed = true,
            useArc = true
        };
    }
}
```

### Exemple : Utilisation de l'Input Router

```csharp
using Louis.XR.Interactions.Input;
using UnityEngine;

public class InputExample : MonoBehaviour
{
    void Update()
    {
        if (XRInputRouter.Instance == null) return;
        
        // Lire le trigger de la main droite
        float triggerValue = XRInputRouter.Instance.TriggerValue(XRHandSide.Right);
        
        // Vérifier le bouton primaire
        if (XRInputRouter.Instance.PrimaryPressed(XRHandSide.Left))
        {
            Debug.Log("Bouton primaire gauche pressé!");
        }
        
        // Lire le joystick
        Vector2 stick = XRInputRouter.Instance.Thumbstick(XRHandSide.Right);
    }
}
```

### Exemple : Création d'un anchor

```csharp
using Louis.XR.Interactions.Utils.Anchors;
using UnityEngine;

public class AnchorSetup : MonoBehaviour
{
    void Start()
    {
        var anchor = gameObject.AddComponent<XRAnchor>();
        anchor.role = AnchorRole.Grab;
        anchor.handside = HandUsage.Right;
        anchor.priority = 1.5f;
        anchor.debugSnapRadius = 0.15f;
    }
}
```

## Best Practices

1. **Anchors** : Toujours placer au moins un anchor sur les objets grabbables pour un contrôle précis
2. **Remote Grab** : Utiliser `RemoteHLALogic` pour une expérience plus immersive que `RemoteInstantLogic`
3. **Input** : Utiliser `XRInputRouter` pour centraliser les inputs plutôt que de lire directement les actions
4. **Debug** : Activer `XRDebugLogger` pendant le développement pour voir les logs en VR
5. **Performance** : Les anchors utilisent des Gizmos - désactiver en production si nécessaire

## Notes Techniques

- Le système utilise `SerializeReference` pour les logiques de grab, permettant la sérialisation polymorphique
- Les éditeurs custom dans le dossier Editor facilitent la configuration
- Le système d'anchors supporte la visualisation en temps réel dans l'éditeur
- Compatible avec le nouveau Input System de Unity

## Troubleshooting

**Problème** : Les inputs ne fonctionnent pas
- Vérifier que `XRInputRouter` est présent dans la scène
- Vérifier que `InputActionManager` est configuré
- S'assurer que les noms des maps et actions correspondent

**Problème** : Le grab ne fonctionne pas
- Vérifier la présence d'un Rigidbody sur l'objet
- Vérifier la présence de Colliders
- S'assurer qu'au moins une logique (direct/remote) est assignée

**Problème** : Les anchors ne sont pas détectés
- Vérifier que les anchors sont enfants de l'objet grabbable
- Vérifier la compatibilité de main (handside)
- Vérifier la distance de snap (maxSnapDistance)

## License

MIT

## Auteur

Louis

## Changelog

Voir [CHANGELOG.md](CHANGELOG.md)