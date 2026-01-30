# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

## [0.0.1] - 2026-01-26

### Added
- **Input System**: Intégration du nouveau système d'input
  - `XRInputRouter` pour le routage des inputs VR
  - Support des contextes d'input
  - Gestion des priorités et résolution de conflits
  - Editor: `XRInputRouterEditor` pour la configuration
  - Editor: `InputDefinitionDrawer` avec dropdown de sélection
  - Assets de configuration: `InputContextSettings`
- **Strategy Pattern**: Implémentation du pattern stratégie pour les différentes logiques de grab
- **Grab Logic Implementations**:
  - Direct grab logic
  - Remote grab logic
  - Socket grab logic
- **Haptics Feedback**: Retour haptique sur les événements grab, release et hover
- **Inventory Management**: Prefabs de gestion d'inventaire
- **Generic Interactable**: Logique de grab déplacée vers un interactable générique
- **Editor Tools**: Outils d'édition pour améliorer le workflow
- **Anchor-based Grabbing**: Système de grab basé sur des ancres avec considération de la main
- **Multi-grip Grabbable**: Support de saisie multi-points
- **Oriented Grabbable**: Saisie avec orientation
- **Trigger Tool Handler**: Gestionnaire d'outils déclenchés
- **Keyboard Support**: Support du clavier en VR
- **Improved Teleportation**: Amélioration du système de téléportation
- **Hands System**: Système de mains
- **Remote Grab**: Système de saisie à distance
- **Door System**: Système de portes
- **Teleportation**: Système de téléportation complet
- **Documentation**: Ajout de documentation complète
  - Input.md - Guide du système d'input VR
  - Grab.md - Système de saisie
  - Inventory.md - Gestion de l'inventaire
  - Hands.md - Système de mains
  - Keyboard.md - Support clavier VR
  - Teleportation.md - Système de téléportation
  - Tools.md - Système d'outils VR
  - Anchors.md - Système d'ancres
  - Debug.md - Outils de débogage

### Fixed
- Correction du warning lors de la non-sélection du contexte "Default" dans InputContextMaskDrawer

### Changed
- Mise à jour du README.md
- **XRGenericInteractable**: Modifications pour compatibilité avec le nouveau système d'input
- **InventoryShelfController**: Mise à jour des dépendances
- **InventoryShelfSlot**: Mise à jour des dépendances
- **TeleportationManager**: Intégration avec le système d'input
- **XRTriggerToolHandler**: Intégration avec le système d'input
- **Assembly Definitions**: Mise à jour des références
  - XR.Interactions.Grab.asmdef
  - XR.Interactions.Input.asmdef
  - XR.Interactions.inventory.asmdef
  - XR.Interactions.Teleport.asmdef
  - XR.Interactions.Tools.asmdef
  - XR.Interactions.Runtime.asmdef
- **Player Prefab**: Mise à jour de la configuration du rig
- Amélioration de la flexibilité et extensibilité des interactions XR
- Optimisations de performance
- Amélioration de la maintenabilité du code

### Removed
- **RemoteGrabWithRotationLogic**: Suppression de l'ancienne implémentation
  - Déplacé vers Runtime/Grab/Logic/Remote/
- Custom interactables (remplacés par le système générique)
