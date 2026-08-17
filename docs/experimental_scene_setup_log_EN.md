# ExperimentZhuo Scene Setup Log

## Scene Flow

```
SetupZhuo -> PracticeZhuo -> LobbyZhuo -> LoadTrial -> ExperimentalZhuo -> EndZhuo
```

| Scene | Purpose |
|-------|---------|
| SetupZhuo | Collect participant info from WebGL URL params (group/ExpID/sessionId) |
| PracticeZhuo | Mouse/keyboard/find-goal practice tasks |
| LobbyZhuo | Wait for all peers, countdown, transition to experiment |
| LoadTrial | Load trial scene additively by ParticipantGroup |
| ExperimentalZhuo | Loop all Targets, record tracking data per frame |
| EndZhuo | Thank-you screen + redeem code |

---

## Server Config

| Server | Address | Port | Role |
|--------|---------|------|------|
| Data collection (Flask) | localhost | 8080 | Receive experiment data via HTTP POST |
| Ubiq RoomServer | 127.0.0.1 | 8009 | Relay player positions in real-time |
| WebGL deployment | eth-cog.s3... | — | Public experiment URL |

**Start commands:**

```bash
cd vr_experiments\data_collection_backend\python
python server.py                       # Data server

npx @ucl-vr/ubiq-server                # Ubiq room server
```

**Configure Ubiq in Unity:**
1. Project -> Create -> Ubiq -> Connection Definition
2. Set: Type=TcpClient, SendToIp=127.0.0.1, SendToPort=8009
3. Drag into RoomClient -> Servers array

---

## New Files

| File | Description |
|------|-------------|
| `PeerManager.cs` | Spawns/destroys remote avatar prefabs when peers join/leave |
| `RemoteAvatar.prefab` | Lightweight remote player: UbiqNetworkedPlayer+PlayerMovement+MouseTracker (all isLocalPlayer=false) |

### PeerManager Core Logic

- Subscribes to `RoomClient.OnPeerAdded` and `OnPeerRemoved`
- On peer added: skips self (Me), instantiates `remoteAvatarPrefab`, calls `UbiqNetworkedPlayer.SetOwnership(false)` to disable local input and camera
- On peer removed: destroys the corresponding instance by UUID

---

## Code Changes

### MouseTracker.cs

| Change | Why |
|--------|-----|
| `internal bool isLocalPlayer;` -> `public bool isLocalPlayer = true;` | Prevent remote avatars from responding to local mouse |
| Added `if (!isLocalPlayer) return;` at top of Update() | Same as above |
| Cursor lock in Start() guarded with `isLocalPlayer` | Prevent remote avatars from stealing cursor |

### TrialOverview.cs

| Change | Why |
|--------|-----|
| Added `showPanelBetweenTasks = false` | Skip panel display between tasks |
| Added `AutoStartNextTrial()` method | Auto-continue to next task without showing panel |

### TrialSyncManager.cs

| Change | Why |
|--------|-----|
| Added `Database.EndTrial()` in `ApplyTrialEnd()` | EndTrial was missing in multiplayer path |
| Check `showPanelBetweenTasks` in ApplyTrialEnd | Skip panel in auto-advance mode |

### Target.cs

| Change | Why |
|--------|-----|
| Check `showPanelBetweenTasks` in single-player path | Skip panel between tasks |

---

## Bugs Fixed

### 1. Head cannot yaw (left/right)

**Cause:** MouseTracker.playerBody was pointing to the "Body" mesh node instead of the root. Rotating Body only spins the capsule, not the camera.

**Fix:** Drag FPSControllerZhuo root into MouseTracker's Player Body field in Inspector.

### 2. TrialOverview panel invisible

**Cause:** Canvas Render Mode was "World Space" — UI rendered in 3D space, not on screen.

**Fix:** Change Canvas render mode to Screen Space - Overlay.

### 3. Cursor permanently locked

**Cause:** OnEnable() runs before Start(). TrialOverview.OnEnable() unlocks cursor, then MouseTracker.Start() locks it again.

**Fix:** Added isLocalPlayer guard in MouseTracker.Start().

### 4. .meta file GUID conflict

**Cause:** Manually creating .meta files with fake GUIDs that don't match Unity's auto-generated ones.

**Fix:** Delete fake .meta files and let Unity regenerate them.
.meta files are Unity's "ID cards" for each asset. Internally, assets are referenced by GUID, not filename — so renaming or moving files never breaks references.

---

## Knowledge

### FPS Hierarchy & playerBody

```
FPSControllerZhuo            <- Root (CharacterController, movement)
  FirstPersonCharacter 1     <- Pivot (empty Transform, yaw axis)
    FieldOfView               <- Camera (MouseTracker)
    Body                      <- Visual capsule mesh
    groundCheck
```

**playerBody rule:** Drag in a node that is an ancestor of the camera. Rotating an ancestor rotates all descendants.

| playerBody | Effect |
|-----------|--------|
| FPSControllerZhuo (Root) | Look and walk same direction |
| FirstPersonCharacter 1 (Middle) | Look freely, W always points north |

---

## Scene Checklist

- [x] Canvas — Screen Space - Overlay
- [x] TrialOverviewPanel — wired to FPSController, SyncManager, Spawnpoint
- [x] FPSControllerZhuo — PlayerMovement + UbiqNetworkedPlayer(isLocalPlayer=true) + CharacterController
- [x] FieldOfView — Camera + MouseTracker(playerBody=FPSControllerZhuo)
- [x] Targets (3 spheres) — Target component with Number + Description
- [x] Spawnpoint / Recorder / TrialSync / PeerManagerObject / HUD
- [x] Ubiq Network Scene — RoomClient configured with RoomServerConnection
- [x] EventSystem
- [x] TargetMaterials (6 mats in Resources/TargetMaterials/)
