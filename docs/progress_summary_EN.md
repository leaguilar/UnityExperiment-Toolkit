# Progress Summary Part 1

## Completed

### 1. Single-player Test

- 6 scenes full runthrough successful
- Tracking data written to `results/` folder with correct format
- 3 targets x 1 repetition = 3 trials per participant
- Participant data: ID, session, experiment ID, age, gender, group, timestamp
- Tracking data per frame: Time, X, Y, Z, ViewAzimuth, ViewElevation
- Trial tail with MD5 checksum for data integrity

### 2. Multiplayer Networking

- Ubiq RoomServer connected (127.0.0.1:8009)
- Fixed room GUID "4b5e1f8a..." so all clients join same room
- LobbyManager.cs calls `roomClient.Join(fixedGuid)` on startup
- RoomClient timeout disabled to prevent disconnects during scene transition
- Both Editor and Build can connect to the same lobby

### 3. Bug Fixes

- **Mouse look broken:** playerBody was pointing to Body mesh node instead of FPSControllerZhuo root
- **Canvas invisible:** renderMode was WorldSpace, changed to Screen Space - Overlay
- **Cursor locked:** MouseTracker.Start() re-locked cursor after TrialOverview unlocked it
- **Build errors:** 3 compile errors fixed (corrupt JoinAllRoomClients.cs, Editor API references in CaptureWalkthrough.cs and POIMarkerScript.cs)
- **PlayerMovement groundCheck:** auto-find logic added to Awake()
- **Lobby room joining:** LobbyManager now calls roomClient.Join() directly

### 4. New Scripts

- **PeerManager.cs:** spawns/destroys remote avatars on peer join/leave via Ubiq RoomClient events
- **RemoteAvatar.prefab:** lightweight remote player prefab with disabled local input and camera

### 5. Flow Improvements

- **TrialOverview:** optional `showPanelBetweenTasks` to skip between-task panels
- **AutoStartNextTrial():** continues task loop without UI panel interruption
- **LobbyManager:** explicitly joins shared room GUID + timeoutBehaviour disabled

---

## Server Config

| Service | Address | Port |
|---------|---------|------|
| Data collection (Flask) | localhost | 8080 |
| Ubiq RoomServer | 127.0.0.1 | 8009 |

**Start commands:**

```bash
python server.py                       # Data collection
npx @ucl-vr/ubiq-server                # Ubiq multiplayer
```

---

## Key Files Changed

| File | Change |
|------|--------|
| MouseTracker.cs | isLocalPlayer guard + playerBody fix |
| TrialOverview.cs | showPanelBetweenTasks + AutoStartNextTrial |
| TrialSyncManager.cs | Database.EndTrial() call + panel skip logic |
| Target.cs | showPanelBetweenTasks check |
| LobbyManager.cs | roomClient.Join(fixedGUID) + timeoutBehaviour=None |
| PeerManager.cs | timeoutBehaviour=None |
| PlayerMovement.cs | groundCheck auto-find |
| JoinAllRoomClients.cs | Fixed corrupt first line (stray "wo'") |
| CaptureWalkthrough.cs | UNITY_EDITOR guard for EditorApplication |
| POIMarkerScript.cs | UNITY_EDITOR guard for SceneView |
