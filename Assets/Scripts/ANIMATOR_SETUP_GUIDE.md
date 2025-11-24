# Animator Controller Setup Guide - Trigger-Based State System

This guide explains how to set up your Unity Animator Controller to work with the trigger-based animation system implemented in `Player.cs`.

## Overview

The system uses **triggers** for all state transitions, which provides cleaner state management and prevents conflicts. Each animation state is triggered independently based on the player's current conditions.

## Required Animation Parameters (Triggers)

Create the following **Trigger** parameters in your Animator Controller:

1. **TriggerIdle** - Trigger for idle/catwalk idle state
2. **TriggerWalking** - Trigger for walking state
3. **TriggerRunning** - Trigger for running state
4. **TriggerJumping** - Trigger for jumping state
5. **TriggerCrouching** - Trigger for crouching idle state
6. **TriggerWalkCrouching** - Trigger for walking while crouching state

## Animator Controller Structure

### Recommended State Machine Layout

```
[Any State] ──(TriggerJumping)──> [Jumping State]
     │
     ├──(TriggerIdle)──> [Idle/Catwalk Idle State]
     │
     ├──(TriggerWalking)──> [Walking State]
     │
     ├──(TriggerRunning)──> [Running State]
     │
     ├──(TriggerCrouching)──> [Crouching State]
     │
     └──(TriggerWalkCrouching)──> [Walk Crouching State]
```

### State Setup Instructions

1. **Create Animation States:**
   - Create 6 animation states (one for each trigger)
   - Name them: `Idle`, `Walking`, `Running`, `Jumping`, `Crouching`, `WalkCrouching`
   - Assign your animation clips to each state

2. **Set Up Transitions:**
   - Use **"Any State"** as the source for all transitions (this allows transitions from any state)
   - Create transitions from "Any State" to each of your animation states
   - Set the condition for each transition to use the corresponding trigger

3. **Transition Settings:**
   - **Has Exit Time:** Uncheck this (we want immediate transitions)
   - **Transition Duration:** Set to 0.1-0.2 seconds for smooth blending
   - **Interruption Source:** Set to "None" or "Source" to prevent unwanted interruptions

### Example Transition Setup

For the **Jumping** state transition:
- **Source:** Any State
- **Destination:** Jumping State
- **Condition:** TriggerJumping (trigger)
- **Has Exit Time:** ❌ False
- **Transition Duration:** 0.15
- **Interruption Source:** None

Repeat this pattern for all 6 states.

## Priority System

The code determines state priority as follows (highest to lowest):
1. **Jumping** - When player is not grounded
2. **Crouching States** - When crouch key is pressed
   - WalkCrouching (if moving while crouching)
   - Crouching (if idle while crouching)
3. **Movement States** - When grounded and moving
   - Running (if speed > runSpeedThreshold, default: 4.0)
   - Walking (if speed > walkSpeedThreshold, default: 0.1)
4. **Idle** - Default state when none of the above apply

## Important Notes

### Trigger Behavior
- Triggers are automatically reset after being consumed by Unity
- The code resets all triggers before setting a new one to ensure clean transitions
- Only one trigger is set per frame, preventing conflicts

### State Transitions
- Transitions happen immediately when conditions change
- The system tracks the current state to avoid redundant triggers
- All transitions use "Any State" to allow transitions from any current state

### Speed Thresholds
You can adjust these in the Inspector:
- **walkSpeedThreshold** (default: 0.1) - Speed above which walking animation plays
- **runSpeedThreshold** (default: 4.0) - Speed above which running animation plays

## Troubleshooting

### Animations not transitioning:
- Check that all trigger parameter names match exactly (case-sensitive)
- Verify transitions are set up from "Any State" to each state
- Ensure "Has Exit Time" is unchecked on all transitions

### Animation stuck in one state:
- Check that your animation clips are set to loop (if needed)
- Verify the trigger parameters are actually triggers (not bools or floats)
- Check the console for any animator errors

### Jumping animation not playing:
- Ensure the player has a Rigidbody2D and ground detection is working
- Check that the ground layer mask is set correctly
- Verify the Jumping state transition condition uses TriggerJumping

## Testing Checklist

- [ ] All 6 trigger parameters created in Animator Controller
- [ ] All 6 animation states created and assigned clips
- [ ] Transitions from "Any State" to all states configured
- [ ] All transitions have correct trigger conditions
- [ ] "Has Exit Time" is unchecked on all transitions
- [ ] Animation clips are set to loop (where appropriate)
- [ ] Ground detection is working (check gizmos in Scene view)
- [ ] Crouch input is mapped (default: C key)

## Code Integration

The `Player.cs` script automatically:
- Detects player state based on movement, ground status, and crouch input
- Triggers the appropriate animation state
- Prevents redundant triggers by tracking current state
- Resets triggers before setting new ones for clean transitions

No additional code changes needed - just set up the Animator Controller as described above!

