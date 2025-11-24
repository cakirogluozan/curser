# Unity 2D Platformer

A 2D platformer game built with Unity, featuring comprehensive player movement, animations, dash mechanics, and jump systems.

## Features

### Player Movement
- **Horizontal Movement**: Smooth left/right movement (A/D or Arrow Keys)
- **Sprinting**: Hold Shift while moving to sprint
- **Crouching**: Press C to crouch, can move while crouched
- **Jump Mechanics**: 
  - Single jump from ground
  - Double jump in mid-air
  - Falling state detection
- **Dash Ability**: Quick dash in movement direction (Ctrl key)
  - Configurable dash force, duration, and cooldown
  - Dashes in movement direction or facing direction
- **Restart Functionality**: Hold R for 2 seconds to restart the game

### Animation System
- **Complete Animation States**:
  - Idle
  - Walking
  - Running
  - Crouching
  - Walk-Crouching
  - Jumping
  - Double Jumping
  - Falling
  - Dashing
- **Configurable Animation Triggers**: All animation triggers can be customized
- **Animation Options**: Option to use running animation for walking
- **Smart State Management**: Animation states prioritize correctly (dash > jump > movement > idle)

### 3D Model Support
- Support for 3D models in 2D gameplay
- Automatic Z-position locking to maintain 2D gameplay
- Model flipping based on movement direction
- Scale preservation options
- Automatic animator detection on child models

### Technical Features
- Input System integration (Unity's new Input System)
- Rigidbody2D physics-based movement
- Ground detection with configurable radius and layer mask
- Animator validation and error handling
- Scene management for restart functionality

## Controls

| Input | Action |
|-------|--------|
| **A / Left Arrow** | Move left |
| **D / Right Arrow** | Move right |
| **Shift** | Sprint (while moving) |
| **Space** | Jump / Double Jump |
| **C** | Crouch |
| **Ctrl** | Dash |
| **R** (Hold 2s) | Restart game |

## Setup

1. Clone the repository
   ```bash
   git clone <repository-url>
   ```

2. Open the project in Unity
   - Recommended version: Unity 2022.3 LTS or newer
   - The project uses Universal Render Pipeline (URP)

3. Open the scene
   - Navigate to `Assets/Scenes/SampleScene.unity`

4. Configure the Player GameObject
   - Ensure the Player GameObject has:
     - `Rigidbody2D` component (required)
     - `Player` script attached
     - `Animator` component (on the player or child model)
     - Ground check setup (automatically created if missing)

5. Set up Animator Controller
   - Assign an AnimatorController to the Animator component
   - Add the following trigger parameters:
     - `TriggerIdle`
     - `TriggerWalking`
     - `TriggerRunning`
     - `TriggerCrouching`
     - `TriggerWalkCrouching`
     - `TriggerJumping`
     - `TriggerDoubleJumping`
     - `TriggerFalling`
     - `TriggerDashing`
   - See `Assets/Scripts/ANIMATOR_SETUP_GUIDE.md` for detailed setup instructions

6. Configure Player Settings (in Inspector)
   - **Movement**: Adjust move speed and sprint speed
   - **Jump Settings**: Configure jump force and double jump force
   - **Dash Settings**: Tune dash force, duration, and cooldown
   - **Restart Settings**: Adjust restart hold duration (default: 2 seconds)
   - **Animation Triggers**: Customize trigger names if needed
   - **Ground Detection**: Set ground check radius and layer mask

## Project Structure

```
Assets/
├── Animations/              # Animation clips (.fbx files)
├── Models/                  # 3D models and materials
│   ├── girl.fbx            # Player model
│   └── Materials/          # Model materials
├── Scripts/                 # C# scripts
│   ├── Player.cs           # Main player controller
│   └── ANIMATOR_SETUP_GUIDE.md  # Animator setup guide
├── Scenes/                  # Unity scenes
│   └── SampleScene.unity   # Main game scene
├── Settings/                # Project settings
└── Sprites/                 # Sprite assets
```

## Requirements

- **Unity Version**: 2022.3 LTS or newer
- **Packages**:
  - Input System (included)
  - Universal Render Pipeline (URP)
  - 2D Sprite (for sprites if used)

## Scripts Documentation

### Player.cs

Main player controller script that handles all player mechanics.

#### Key Features:
- **Input Processing**: Handles keyboard input via Unity's Input System
- **Movement & Physics**: Rigidbody2D-based movement with sprint and crouch
- **Jump System**: 
  - Ground detection
  - Single and double jump mechanics
  - Falling state tracking
- **Dash System**:
  - Direction-based dashing
  - Cooldown management
  - Duration tracking
- **Animation State Management**: 
  - State machine for animations
  - Priority-based state selection
  - Trigger-based animation system
- **Model Management**: 
  - 3D model support in 2D space
  - Automatic model flipping
  - Z-position locking
- **Scene Management**: Restart functionality

#### Configurable Parameters:

**Movement**
- `moveSpeed`: Base movement speed
- `sprintSpeed`: Sprint movement speed
- `walkSpeedThreshold`: Minimum speed to trigger walking animation

**Jump**
- `jumpForce`: Force applied for single jump
- `doubleJumpForce`: Force applied for double jump
- `groundCheckRadius`: Radius for ground detection circle
- `groundLayerMask`: Layer mask for ground detection

**Dash**
- `dashForce`: Speed/force of dash
- `dashDuration`: How long dash lasts (seconds)
- `dashCooldown`: Time before dash can be used again (seconds)

**Restart**
- `restartHoldDuration`: Time to hold R key before restart (seconds)

**Animation**
- All trigger names are configurable
- `useRunningAnimationForWalking`: Toggle to use running animation for walking

## Development Notes

- The script automatically creates a ground check GameObject if one isn't assigned
- Animator is automatically found on the player or child models
- The script validates AnimatorController assignment and logs helpful errors
- All movement is physics-based using Rigidbody2D
- Animation states have priority: Dash > Jump > Falling > Crouch > Movement > Idle

## Troubleshooting

### Animator Not Found
- Ensure an Animator component exists on the Player GameObject or a child model
- Check the console for helpful warning messages

### AnimatorController Not Assigned
- Assign an AnimatorController to the Animator component in the Inspector
- The script will log an error if the controller is missing

### Missing Animation Triggers
- Ensure all required trigger parameters exist in your AnimatorController
- Check trigger names match the configured values in the Player script

### Ground Detection Not Working
- Verify the ground check GameObject is positioned correctly
- Check that ground objects are on the correct layer (set in `groundLayerMask`)
- Adjust `groundCheckRadius` if needed

## License

[Add your license here]

## Contributing

[Add contribution guidelines here]
