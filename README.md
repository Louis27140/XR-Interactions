# Louis - XR Interactions

Package Unity fournissant un système d'interactions XR avancé basé sur le XR Interaction Toolkit de Unity.

## Version

**0.1.0**

## Prérequis

- Unity 2022.3 ou supérieur
- XR Interaction Toolkit 3.2.2
- [com.louis.xr.core](../com.louis.xr.core/README.md) 0.1.0

## Installation

1. Installer d'abord le package `com.louis.xr.core`
2. Ouvrir le Package Manager dans Unity
3. Sélectionner "Add package from disk..."
4. Naviguer vers le fichier `package.json` de ce package

## 📚 Systèmes disponibles

| Système | Description | Documentation |
|---------|-------------|---------------|
| 🎮 **Grab** | Système modulaire de saisie (Direct, Remote HLA) | [📖 Documentation](Documentation/Grab.md) |
| 🖐️ **Hands** | Animation procédurale des mains (Grip/Trigger) | [📖 Documentation](Documentation/Hands.md) |
| 🎯 **Input** | Routeur centralisé, Context Stack et Haptique | [📖 Documentation](Documentation/Input.md) |
| 🎚️ **Anchors** | Points de snap intelligents pour le grab | [📖 Documentation](Documentation/Anchors.md) |
| 🚀 **Teleportation** | Locomotion par téléportation au joystick | [📖 Documentation](Documentation/Teleportation.md) |
| 📦 **Inventory XR** | Slots physiques (sockets) pour l'inventaire | [📖 Documentation](Documentation/Inventory.md) |
| ⌨️ **Keyboard XR** | Clavier virtuel auto-positionné | [📖 Documentation](Documentation/Keyboard.md) |
| 🔧 **Tools XR** | Bridge pour outils activables (lampe, caméra) | [📖 Documentation](Documentation/Tools.md) |
| 🐛 **Debug XR** | Activation du debugger in-VR | [📖 Documentation](Documentation/Debug.md) |


## 🏗️ Structure du Package

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
├── Documentation/         # Documentation détaillée
│   ├── Grab.md
│   ├── Hands.md
│   ├── Input.md
│   ├── Inventory.md
│   ├── Keyboard.md
│   ├── Teleportation.md
│   ├── Tools.md
│   ├── Anchors.md
│   └── Debug.md
└── Samples~/              # Exemples (Rig, Door, Teleportation)
```

## License

MIT

## Auteur

Louis

## Changelog

Voir [CHANGELOG.md](CHANGELOG.md)
