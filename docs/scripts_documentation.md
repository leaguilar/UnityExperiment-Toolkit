# Scripts Documentation

This document describes every C# script in `Assets/Scripts/` and what it does.

Scripts are organized into subdirectories that mirror the sections below:

```
Assets/Scripts/
├── ExperimentFlow/      — UI flow and trial coordination
├── Practice/            — Control tutorials and tests
├── DataModel/           — Data structures and static Database
├── DataRecording/       — Recorders and HTTP uploaders
├── SceneLoading/        — Scene transition helpers
├── Targets/             — Trial goal objects
├── Configuration/       — Platform utilities and admin tools
├── Helpers/             — Small reusable utilities
├── Multiplayer/         — Ubiq lobby and trial sync
├── Virtual Reality/     — Cognitive walkthrough scripts
├── Agent-Based Modelling/ — Simulation scripts
└── Editor/              — Editor-only tools
```

---

## Experiment Flow & UI

### `SequentialVisibleElement.cs`
Abstract base class for all UI panels that appear in a sequence. When activated, it automatically deactivates its follow-up element, ensuring only one panel is visible at a time. Subclasses must implement `GetFollowup()` to declare what comes next in the chain.

### `SetupPage.cs`
Abstract base for any UI page that has an "Apply / Continue" button. Wires the button's `onClick` event to validate and advance the flow. Subclasses implement `CanApplyPage()` (validation) and `OnApplyPage()` (side effects on confirm). Declares a `nextPage` reference that gets activated on confirm.

### `WaitForButton.cs`
Concrete `SetupPage` that does nothing except wait for the user to click the button. Optionally loads a new scene by name (`NextSceneName`) when confirmed. Includes a short `minWaitTime` guard to prevent accidental double-clicks. Shows the cursor while active.

### `ParticipantInfoPage.cs`
Concrete `SetupPage` that collects participant demographics (ID, age, gender). On enable, reads URL query parameters (`group`, `ExpID`, `assignmentId`, `workerId`) via `WebGLTools` and populates `Database`. Also fetches `config.json` from the server. Validates all fields with live color feedback before allowing progression. On confirm, writes demographics to `Database` and calls `Database.SendParticipantInfo()`.

### `PracticeScenePage.cs`
Concrete `SetupPage` shown before the practice environment. On confirm, enables the `FirstPersonController` and logs `"Started practice."` to the server via `Database.SendMetaData`.

### `TrialOverview.cs`
The central coordinator for running trials within an experimental scene. On enable:
- Ends the previous trial via `Database.EndTrial()`.
- If all tasks are exhausted, loads `NextSceneName`.
- Otherwise, picks the next `Target` + material, shows task description in the UI, and disables the FPS controller while waiting.

On confirm (Apply button):
- Updates the HUD hint text and image with the target description and color.
- Re-enables `FirstPersonController` and `ParticipantRecorder`.
- Calls `Database.StartNewTrial()`.

Shuffles targets and materials using the participant ID as a random seed so the trial order is reproducible per participant.

### `UiFader.cs`
Attached to a UI `Image`. On enable, cross-fades the image from `StartColor`/`StartAlpha` to `TargetColor`/`TargetAlpha` over `FadeDuration` seconds. Used for scene transition fades.

### `HideWhileVideoLoading.cs`
Hides a UI element until a `VideoPlayer` has finished preparing its video, then shows it.

---

## Practice / Controls Tests

### `ControlTest.cs`
Abstract base class for interactive control tutorials. Each frame calls `TestRequirements()`. When it returns `true`, the next element in the sequence is activated and the current one deactivates. Subclasses implement `TestRequirements()` and optionally `OnTestFinished()`.

### `TestKeyboardMovement.cs`
`ControlTest` that requires the participant to hold a specific key for `minPressTime` seconds (default 3 s). Logs completion to the server.

### `TestMouseMovement.cs`
`ControlTest` that requires the participant to rotate the camera at least `RequiredRotationAngle` degrees (default 45°) both left and right before passing.

### `FindGoalTest.cs`
`ControlTest` that requires the participant to physically walk to a `ProximityTrigger` zone. Places the target object at a given `Spawnpoint` and shows a `Hint` GameObject. Completes when the trigger fires. Loads `nextSceneName` and logs completion.

---

## Data Model

### `TrackingEntry.cs`
Plain struct holding a single movement sample: `Time`, `Position` (Vector3), `ViewAzimuth`, `ViewElevation`.

### `TrialData.cs`
Holds all data for one trial: `TargetId`, `TargetMaterialName`, `StartTime`, `EndTime`, and a list of `TrackingEntry`. Fires `TrackingDataAdded` event each time a sample is recorded, which `Database` listens to in order to stream data immediately.

### `Database.cs` (static)
The global in-memory store for the current session. Holds:
- Participant metadata: `ParticipantId`, `ParticipantGroup`, `ParticipantAge`, `ParticipantGender`, `ExperimentId`, `SessionId`, `DataCollectionServerURL`.
- `CurrentTrial` — the active `TrialData`. When replaced, unsubscribes/resubscribes the streaming hook.
- `TrialResults` — list of completed `TrialData`.
- `StartNewTrial(targetId, materialName)` — initialises a new `TrialData`, creates the `DataUploadHandler` if not yet done, and fires `NextTrialStarted` event.
- `EndTrial()` — stamps `EndTime`, appends to `TrialResults`, sends tail packet.
- `SendParticipantInfo()` / `SendMetaData(category, data)` — convenience wrappers over `DataUploadHandler`.

### `Gender.cs`
Enum: `Male`, `Female`, `Other`.

---

## Data Recording

### `ParticipantRecorder.cs`
MonoBehaviour that must be placed on (or near) the participant camera. Each frame while `isRecording`, reads the camera's world-space forward direction, converts it to spherical coordinates (azimuth, elevation), and pushes a `TrackingEntry` into `Database.CurrentTrial`. Call `StartRecording()` / `StopRecording()` to control it.

### `IDataWriter.cs`
Interface: `Write`, `WriteLine`, `Close`. Abstracts the destination of raw CSV data (file, network stream, or null).

### `EmptyWriter.cs`
No-op implementation of `IDataWriter`. Used as a default when no writer is configured.

### `DataWebStream.cs`
`IDataWriter` that batches written characters into a queue and POSTs them to a URL via `UnityWebRequest` on every Unity Update tick. Retries on network/HTTP errors. Used for raw streaming (legacy; `DataUploadHandler` is the newer structured approach).

### `DataUploadHandler.cs`
Structured HTTP data uploader. Sends distinct JSON packet types to the server:
- **UserData** — participant demographics.
- **TrialHeader** — target ID, material, start time.
- **TrialData** — batched tracking rows (flushed every ~4 KB).
- **TrialTail** — end time, durations, MD5 checksum of all trial data.
- **MetaData** — free-form key-value annotations (e.g. admin events, practice events).

All packets are sent as HTTP POST with `Content-Type: application/json`. Multiple requests can be in-flight simultaneously; completed/failed requests are cleaned up each Update.

---

## Scene Loading

### `LoadTrial.cs`
MonoBehaviour for a loading screen. On `Awake`, reads `Database.ParticipantGroup` to pick which scene from the `Trials` list to load additively. Once loaded, unloads the origin scene. Shows load progress percentage via an optional `TMP_Text`.

---

## Targets

### `Target.cs`
Placed in the experimental scene on each goal object. Fields: `Number` (int, must match the ID passed to `StartNewTrial`) and `Description` (human-readable string). On start, hides itself. Listens to `Database.NextTrialStarted`; shows/enables its collider only when its `Number` matches the active target. On trigger enter with the `FirstPersonController`, calls `Database.EndTrial()` and re-activates the `TrialOverview` panel.

### `ProximityTrigger.cs`
Generic trigger wrapper. Exposes `TriggerEnter` / `TriggerExit` C# events and a `triggered` bool. Filters to a specific `TargetObject`. Used by `FindGoalTest` for practice.

---

## Configuration & Platform

### `WebGLTools.cs`
Static utility for WebGL builds. Parses URL query parameters (`GetParameter(key)`) so experiment config can be passed via the URL (e.g. `?ExpID=123&group=2&workerId=XYZ`). In the Editor, uses a hardcoded debug URL. Also provides `ValidateDeployment()` (checks the build is running from the expected URL) and `FetchConfigJsonData()` (downloads `config.json` from the server and populates `ConfigData.dataAssemblyUrl`).

### `GenerateRedeemCode.cs`
Placed on the End scene. Computes an MD5-based verification code from `ExperimentId + ParticipantGroup + salt` and displays it in an `InputField`, pre-selected and copied to clipboard. Used to give participants a completion code (e.g. for MTurk).

### `AdminHacks.cs`
Debug tool. Listens for a secret key sequence (`64859972`) to unlock admin mode. Once unlocked, exposes keyboard commands: load any scene by number, adjust walk speed, show help. All admin actions are logged as metadata via `Database.SendMetaData`.

---

## Helpers

### `PlacementHelper.cs`
Extension method `PlaceObject(position, rotation)` on `GameObject`. Handles both `CharacterController` and standard transform teleportation.

### `ConstantRotation.cs`
Rotates a GameObject continuously around a specified axis at a given speed. Useful for decorative elements.

### `TrialOverview.cs` (also see Experiment Flow section above)
Acts as the between-trial UI hub, always present in the experimental scene but toggled visible/invisible around each trial.

---

## Multiplayer

> Scripts for coordinating multi-participant VR experiments via the Ubiq networking framework. Used in the **Lobby** scene and the collaborative experimental scene. Documented in detail in `vr_experiment.md`.

### `VrExperimentConfig.cs`
Plain serializable data class mirroring `experiment_N_config.json`. Holds `experimentId`, `requiredParticipants`, `countdownSeconds`, `nextSceneName`, and `dataAssemblyUrl`. Loaded at runtime by `LobbyManager`.

### `LobbyManager.cs`
`NetworkedBehaviour` (or `MonoBehaviour` without Ubiq) that manages the pre-experiment waiting room. On start it fetches `VrExperimentConfig` via `UnityWebRequest`, writes `ExperimentId` and `DataCollectionServerURL` into `Database`, then monitors the Ubiq peer count. The first client to reach `requiredParticipants` broadcasts a `"countdown_start"` message; all clients (including the sender) run the countdown coroutine and load `nextSceneName` simultaneously.

### `TrialSyncManager.cs`
`NetworkedBehaviour` placed in the experimental scene. Ensures all participants run the same trial at the same time:
- `BroadcastTrialStart(targetId, materialName)` — sends a `"start"` message and calls `TrialOverview.OnNetworkTrialStart()` locally.
- `BroadcastTrialEnd()` — guards with `isTrialActive` to prevent duplicates, sends `"end"`, re-activates the `TrialOverview` panel on all clients.
- `ProcessMessage()` — receives remote signals and mirrors the same local calls.

---

## Virtual Reality (Cognitive Walkthrough — separate use case)

> These scripts are for the **unstructured virtual walkthrough** mode (no trials, no server upload). They are documented separately in `virtual_walkthrough.md`.

- `PlayerMovement.cs` — WASD + gravity character controller.
- `MouseTracker.cs` — Mouse look.
- `CaptureWalkthrough.cs` — Samples position/orientation to CSV at a fixed interval.
- `ProcessWalkthrough.cs` — Post-hoc trajectory analysis and heatmap.
- `HeatMapper.cs` — Cone-raycast visual attention heatmap.
- `VisualizeTrajectory.cs` — Line renderer for trajectories.
- `ProperConeRayCast.cs` — Utility for cone visibility checks.
- `UbiqNetworkedPlayer.cs` — Multiplayer position sync via Ubiq.

---

## Agent-Based Modelling (Simulation — separate use case)

> These scripts run headless or in-editor simulations with autonomous agents. Documented in the main README.

- `EngineScript.cs` — Simulation controller.
- `AgentScript.cs` — Individual agent NavMesh behaviour.
- `TaskScript.cs` — Task definitions for agents.
- `ConfigLoader.cs` / `ConfigManager.cs` — JSON config loading.
- `CommandLineParser.cs` — Headless batch mode.
- `POIMarkerScript.cs` — Point-of-interest visualization.
- `ABMVisualizer.cs` — Density and trajectory visualizations.
- `EBDMath.cs` — Math utilities.

---

## Editor Tools

- `HiddenObjects.cs` — Menu items to show/hide objects by hidden flag.
- `ReplacerWindow.cs` — Editor window to bulk-replace GameObjects.
- `SelectionUtil.cs` — Menu items to select objects by component type.
- `SetPositionWindow.cs` — Editor window to set object positions numerically.
