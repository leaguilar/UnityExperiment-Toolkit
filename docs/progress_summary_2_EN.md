# Progress Summary Part 2

## Build Fixes

### Lobby to Experiment Transition Broken

**Root cause:** `context.Send()` threw NullReferenceException in the Build because `NetworkedBehaviour.context` was null when `CheckPeerCount()` ran with 2 peers. This silently killed `StartCountdown()`.

**Fix:** Wrapped `context.Send()` in try-catch so the countdown always starts even if the network message fails to send.

### Config Issues

- `LoadTrial` scene was missing from Build Settings — removed dependency, Lobby now transitions directly to `ExperimentalZhuo`
- StreamingAssets config `nextSceneName` was misaligned between Editor and Build

---

## New 8-Step Medical Workflow

### Target.cs Rewrite

| Feature | Description |
|---------|-------------|
| `requireInteraction` | `true` = player must press E to complete; `false` = auto-complete on walk-in |
| `keepMeshVisible` | `true` = mesh stays visible even when not the active target (used for patient NPCs) |
| `onTargetCompleted` | UnityEvent fired on completion, wired to `PatientAgent.Interact()` in Inspector |

### TrialOverview.cs Changes

| Feature | Description |
|---------|-------------|
| `randomizeOrder` | `false` = targets execute in Number order (1,2,3...8) instead of shuffled |
| HUD hint | Shows `Step X/8` with description and `[Press E to complete]` for interaction steps |

### PatientAgent.cs (New)

Standalone patient NPC script with configurable Name, Age, Condition, and Bed fields. On E-interact (triggered by `onTargetCompleted`), it shows an info panel and then walks to a destination via Unity NavMeshAgent.

### Scene Structure for Patient Targets

```
Patient_NPC                       PatientAgent + NavMeshAgent + Capsule + CapsuleCollider
  Capsule (body)                   Visual model
  TriggerSphere                    SphereCollider(isTrigger) + Target(Number, requireInteraction, keepMeshVisible)
                                    Target.onTargetCompleted -> Patient_NPC.PatientAgent.Interact()
```

Steps 1 (Reception), 5 (Deliver Diagnosis), and 7 (Final Visit) use this structure.

Steps 2, 3, 4, 6, 8 use plain Target spheres at locations (walk-to or E-interact at stations).

### Bug Fixes

- `HideInteractionPrompt()` was called AFTER `AutoStartNextTrial()`, hiding the fresh task hint
- View/capsule separation fixed by removing the static RemoteAvatar instance from the scene
- PlayerMovement `groundCheck` auto-find added to Awake()
- PlayerMovement `layerMask` set to Everything to prevent sinking into ground

---

## Server Config (unchanged)

| Service | Address | Port |
|---------|---------|------|
| Data collection (Flask) | localhost | 8080 |
| Ubiq RoomServer | 127.0.0.1 | 8009 |
