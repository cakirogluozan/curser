# Ozzie and the Curser

A 2D platformer game built with Unity, featuring Ozzie (the main character) and the Curser (a fairy companion that follows Ozzie). The game features comprehensive player movement, animations, dash mechanics, jump systems, and combat with enemies.

## Game Overview

**Ozzie and the Curser** is a 2D platformer where you play as Ozzie, a character navigating through challenging levels. The Curser is a magical fairy companion that follows Ozzie and assists in combat by attacking enemies.

### Characters
- **Ozzie**: The main playable character with full movement and combat capabilities
- **The Curser**: A fairy companion that follows Ozzie and attacks enemies automatically

### 3D Model Creation
The fairy (Curser) model was created using text-to-3D AI model generation tools, then rigged and animated using Blender. This allows for smooth 3D animations while maintaining 2D gameplay mechanics.

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
- **Player Animation States**:
  - Idle
  - Walking
  - Running
  - Crouching
  - Walk-Crouching
  - Jumping
  - Double Jumping
  - Falling
  - Dashing
- **Fairy Animation States**:
  - Idle
  - Attacking
- **Configurable Animation Triggers**: All animation triggers can be customized
- **Animation Options**: Option to use running animation for walking
- **Smart State Management**: Animation states prioritize correctly (dash > jump > movement > idle)
- **Automatic State Transitions**: Fairy automatically transitions between idle and attacking states

### Combat System
- **Enemy Health System**: Enemies have 100 health points
- **Damage System**: Enemies take damage when hit by attacks
- **Enemy Death**: Enemies are destroyed when health reaches 0
- **Damage Numbers**: Visual damage numbers display above enemies when hit
- **Fairy Attacks**: The Curser (fairy) automatically attacks nearby enemies

### 3D Model Support
- Support for 3D models in 2D gameplay
- Automatic Z-position locking to maintain 2D gameplay
- Model flipping based on movement direction
- Scale preservation options
- Automatic animator detection on child models
- **Fairy Model**: Created using text-to-3D AI generation and rigged in Blender

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
│   ├── girl.fbx            # Player model (Ozzie)
│   ├── fairy.fbx           # Fairy model (The Curser) - AI generated and Blender rigged
│   └── Materials/          # Model materials
├── Scripts/                 # C# scripts
│   ├── Player.cs           # Main player controller (Ozzie)
│   ├── AttackHelper.cs    # Fairy attack system (The Curser)
│   ├── EnemyAI.cs          # Enemy AI and health system
│   ├── DamageNumber.cs     # Damage number display system
│   ├── Projectile.cs       # Projectile system for attacks
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

Main player controller script for Ozzie that handles all player mechanics.

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
- **Fairy Model Creation**: The Curser model was generated using text-to-3D AI tools, then imported to Blender for rigging and animation setup
- **Enemy System**: Enemies have 100 health and are destroyed when health reaches 0
- **Damage System**: Damage numbers appear above enemies when they take damage
- **UI Templates**: Template main menu and pause menu canvas have been added
- **First Build**: First build test completed successfully

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

## TODO / Development Roadmap

### 🎨 Animation System
- [x] **Fairy Animations**: Idle and attacking animations for The Curser ✅
- [x] **Enemy Animations**: Enemy movement and rotation based on direction ✅
- [ ] **Dying Animation**: Death state and animation when player/enemy health reaches zero
- [ ] **Shooting Animation**: Attack/shooting animations for player
- [ ] **Emotes**: Player emote animations (wave, dance, taunt, etc.)
- [ ] **Enemy Death Animations**: Death animations for enemies
- [ ] **Boss Animations**: Special animations for boss characters
- [ ] **Environmental Animations**: Animated props, background elements

### 🎮 Gameplay Features
- [x] **Enemy Health System**: Enemies have health and take damage ✅
- [x] **Damage System**: Visual damage numbers and health tracking ✅
- [x] **Fairy Combat**: The Curser attacks enemies automatically ✅
- [ ] **Power-ups**: Temporary boosts (speed, damage, invincibility, etc.)
- [ ] **Skills System**: Unlockable abilities and skill trees
- [ ] **Inventory System**: Item collection and management
- [ ] **Item Types**: Weapons, consumables, equipment, collectibles
- [ ] **Curse System**: Different types of curses with unique effects
- [ ] **Stage Design**: Multiple levels with unique layouts and challenges
- [ ] **Boss Fights**: Boss encounters with unique mechanics and patterns
- [ ] **Boss Models**: 3D models and animations for boss characters

### 🌍 World & Design
- [ ] **World Theme**: Consistent art style and world design
- [ ] **Enemy Design**: Visual design and variety for different enemy types
- [ ] **Environment Art**: Backgrounds, platforms, obstacles, decorations
- [ ] **Level Progression**: Difficulty curve and level unlocking system
- [ ] **Parallax Backgrounds**: Multi-layer scrolling backgrounds (partially implemented)

### 🛍️ NPCs & Economy
- [ ] **Shop NPCs**: Buy/sell items and equipment
- [ ] **Crafting NPCs**: Craft new curses and items
- [ ] **Lottery System**: Random reward system
- [ ] **Currency System**: In-game money/economy
- [ ] **Dialogue System**: NPC conversations and quests

### 🖥️ UI/UX
- [ ] **Health Bar**: Player health display
- [ ] **Mana/Energy Bar**: Resource system display
- [ ] **Item Display**: Show equipped/collected items
- [ ] **Minimap**: Level overview and navigation
- [x] **Main Menu**: Start screen, settings, credits ✅ (Template canvas added)
- [x] **Pause Menu**: In-game pause functionality ✅ (Template canvas added)
- [ ] **HUD Elements**: Score, level, timer, etc.
- [ ] **Inventory UI**: Visual inventory management interface
- [ ] **Skill Tree UI**: Visual representation of skills and upgrades

### 🎬 Media & Marketing
- [ ] **Gameplay Videos**: Record gameplay clips for trailers
- [ ] **Game Preview**: Create promotional game preview/trailer
- [ ] **Screenshots**: Capture key moments and features
- [ ] **Social Media Assets**: Images and videos for promotion

### 🔧 Technical Improvements
- [ ] **Save System**: Save/load game progress
- [ ] **Settings Menu**: Graphics, audio, controls
- [ ] **Audio System**: Sound effects and background music
- [ ] **Particle Effects**: Enhanced visual effects (partially implemented)
- [ ] **Camera System**: Dynamic camera following, zoom, shake effects
- [ ] **Performance Optimization**: Optimize for target platforms
- [ ] **Localization**: Multi-language support

### 🐛 Polish & Quality
- [ ] **Bug Fixes**: Fix known issues and edge cases
- [ ] **Balance Testing**: Tune difficulty, damage values, spawn rates
- [ ] **Playtesting**: Gather feedback and iterate
- [ ] **Accessibility**: Options for colorblind, difficulty settings
- [ ] **Tutorial**: In-game tutorial or help system

## Suggestions

### Additional Features to Consider:
- **Combo System**: Chain attacks for bonus damage
- **Achievement System**: Unlock achievements for milestones
- **Leaderboards**: Compare scores with other players
- **Daily Challenges**: Daily quests or challenges
- **Secrets & Easter Eggs**: Hidden areas and references
- **Multiple Characters**: Different playable characters with unique abilities
- **Co-op Mode**: Multiplayer support (local or online)
- **Procedural Generation**: Randomly generated levels or elements
- **Weather System**: Dynamic weather effects
- **Day/Night Cycle**: Time-based gameplay changes

### Code Organization:
- **ScriptableObjects**: Use for items, enemies, skills configuration
- **Event System**: Decouple systems using Unity Events or C# events
- **Object Pooling**: Optimize projectile and effect spawning
- **State Machine**: Formal state machine for complex behaviors
- **Data Persistence**: JSON or ScriptableObject-based save system

### Art & Assets:
- **Sprite Sheets**: Optimize sprite organization
- **Animation Controllers**: Separate controllers for different character types
- **Shader Effects**: Custom shaders for visual polish
- **Post-Processing**: Add post-processing effects for atmosphere

## License

Copyright (c) 2024 AI Developer

**Personal Use License**

This game and its source code are provided for **personal use only**. 

### Permitted Uses:
- ✅ Playing the game for personal entertainment
- ✅ Learning from the source code for educational purposes
- ✅ Modifying the code for personal use
- ✅ Sharing gameplay videos or screenshots

### Prohibited Uses:
- ❌ **Publishing** or redistributing the game
- ❌ **Commercial use** or selling the game
- ❌ **Releasing** modified versions publicly
- ❌ **Claiming ownership** of the game or code
- ❌ **Using assets** (models, animations, sprites) in other projects without permission

### Terms:
This game is developed from scratch by an AI developer. All rights are reserved. 
The game is provided "as is" without warranty of any kind.

**For any questions or permissions beyond personal use, please contact the developer.**

---

**Note**: This license allows you to enjoy and learn from the game, but protects the developer's work from unauthorized distribution or commercial use.

## Contributing

This project is currently not accepting contributions. However, feedback and suggestions are welcome!

If you'd like to report bugs or suggest features, please create an issue in the repository.
