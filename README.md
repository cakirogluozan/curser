# Unity 2D Platformer

A 2D platformer game built with Unity, featuring player movement, animations, and jump mechanics.

## Features

- **Player Movement**
  - Horizontal movement (A/D or Arrow Keys)
  - Sprinting (Hold Shift while moving)
  - Crouching (C key)
  - Jump and double jump mechanics

- **Animation System**
  - Idle, Walking, Running animations
  - Crouching and walk-crouching animations
  - Jumping, double jumping, and falling animations
  - Configurable animation triggers
  - Option to use running animation for walking

- **3D Model Support**
  - Support for 3D models in 2D gameplay
  - Automatic Z-position locking
  - Model flipping based on movement direction
  - Scale preservation options

## Controls

- **A / Left Arrow**: Move left
- **D / Right Arrow**: Move right
- **Shift**: Sprint (while moving)
- **Space**: Jump / Double Jump
- **C**: Crouch

## Setup

1. Clone the repository
2. Open the project in Unity (recommended version: 2022.3 LTS or newer)
3. Open the scene: `Assets/Scenes/SampleScene.unity`
4. Ensure the Player GameObject has:
   - Rigidbody2D component
   - Player script attached
   - Animator component (on the player or child model)
   - Ground check setup

## Project Structure

```
Assets/
├── Animations/          # Animation clips
├── Models/              # 3D models and materials
├── Scripts/             # C# scripts
│   └── Player.cs        # Main player controller
├── Scenes/              # Unity scenes
└── Settings/             # Project settings
```

## Requirements

- Unity 2022.3 LTS or newer
- Input System package (included)
- Universal Render Pipeline (URP)

## Scripts

### Player.cs
Main player controller script that handles:
- Input processing
- Movement and physics
- Animation state management
- Jump state tracking
- Model rotation and scaling

## License

[Add your license here]

