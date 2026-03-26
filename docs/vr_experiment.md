# Building a Structured VR Experiment

This guide explains how to wire the toolkit's scripts into a full experiment with consent, participant info collection, practice, trials, and data upload. This is distinct from the unstructured [Virtual Walkthrough](virtual_walkthrough.md), which has no trials or server upload.

---

## Experiment Architecture

A full experiment consists of several Unity **scenes** that load sequentially. The participant's state is preserved across scenes by the static `Database` class (never destroyed).

**Single-participant:**
```
Setup  →  Practice  →  LoadTrial  →  Experimental  →  End
```

**Multi-participant (collaborative):**
```
Setup  →  Practice  →  Lobby  →  LoadTrial  →  Experimental  →  End
```

The **Lobby** scene is where participants wait until the configured number of people have connected. Once all are present a countdown runs, then all clients transition simultaneously.

The experimental scene contains all `Target` objects and the `TrialOverview` panel, which loops until all trials are exhausted, then transitions to the End scene.

---

## Data Flow

```
ParticipantInfoPage
    └─► Database (static, survives scene loads)
            ├─► DataUploadHandler (HTTP POST to server)
            │       ├─ WriteUserData()      ← demographics
            │       ├─ WriteTrialHeader()   ← per trial start
            │       ├─ WriteTrialData()     ← streaming tracking rows
            │       └─ WriteTrialTail()     ← end time + MD5 checksum
            └─► TrialData (in-memory list of TrackingEntry)

ParticipantRecorder (MonoBehaviour, per-frame)
    └─► Database.CurrentTrial.AddTrackingData()
            └─► event → Database → DataUploadHandler.WriteTrialData()

Target (trigger collider)
    └─► Database.EndTrial()
            └─► TrialOverview.OnEnable()  (next trial or End scene)
```

---

## Scene-by-Scene Setup

### 1. Setup Scene

**Purpose:** Collect consent and participant demographics before anything else.

**Scene objects needed:**

| GameObject | Component | Notes |
|---|---|---|
| `Canvas` | — | Screen-space overlay |
| `ConsentPanel` | `WaitForButton` | `nextPage` → `InfoPanel` |
| `InfoPanel` | `ParticipantInfoPage` | `nextPage` → `ProceedPanel` |
| `ProceedPanel` | `WaitForButton` | `NextSceneName` = `"Practice"` |

**Wiring:**
1. Set `ConsentPanel` active, `InfoPanel` and `ProceedPanel` inactive at start.
2. `ConsentPanel.nextPage` → `InfoPanel` GameObject.
3. `InfoPanel.nextPage` → `ProceedPanel` GameObject.
4. `ProceedPanel.NextSceneName` = `"Practice"`.
5. Connect each panel's `applyButton` in the Inspector.

**Server configuration:**
`ParticipantInfoPage` reads (`simulation_and_vr/vr_experiments/experiment_1_config.json`): from the server on enable. The JSON must contain:
```json
{ "dataAssemblyUrl": "https://your-server.com/collect" }
```
This URL is stored in `Database.DataCollectionServerURL` and used by `DataUploadHandler` for all subsequent uploads.

**URL parameters (WebGL build):**
Pass these in the page URL; `ParticipantInfoPage` reads them automatically:
- `ExpID` — experiment identifier
- `group` — participant group (integer, used to select trial scene in `LoadTrial`)
- `assignmentId` — session ID (e.g. MTurk assignment)
- `workerId` — participant ID (pre-fills the ID field)

---

### 2. Practice Scene

**Purpose:** Teach participants how to move and look before the real experiment.

**Scene objects needed:**

| GameObject | Component | Notes |
|---|---|---|
| `FPSController` | `FirstPersonController` | Standard Unity FPS rig |
| `Canvas` | — | |
| `InstructionsPanel` | `WaitForButton` | `nextPage` → `PracticePanel` |
| `PracticePanel` | `PracticeScenePage` | `FPSController` → FPS rig |
| `KeyboardTestW` | `TestKeyboardMovement` | `key` = W, `minPressTime` = 3 |
| `KeyboardTestS` | `TestKeyboardMovement` | `key` = S, chains to next |
| `MouseTest` | `TestMouseMovement` | `Camera` → FPS camera |
| `FindGoalTest` | `FindGoalTest` | `nextSceneName` = `"LoadTrial"` |
| `Spawnpoint` | Empty transform | Where the practice target spawns |
| `GoalHint` | UI or world GameObject | Shown during `FindGoalTest` |
| `ProximityZone` | `ProximityTrigger` | Trigger collider at the goal |
| `AdminHacks` | `AdminHacks` | Optional; add to any scene |

**Wiring the control test chain:**
Each test activates the next via its `next` field:
```
PracticePanel (button) → KeyboardTestW → KeyboardTestS → MouseTest → FindGoalTest
```
`FindGoalTest.nextSceneName` = `"Lobby"` (multiplayer) or `"LoadTrial"` (single-player) loads the next scene when the participant reaches the goal.

**Practice events** are automatically logged to the server via `Database.SendMetaData("Practice", ...)`.

---

### 3. Lobby Scene *(multiplayer only)*

**Purpose:** Wait for all participants to connect before starting the experiment. Reads `requiredParticipants` from the experiment config, shows a live count, then transitions all clients simultaneously via a countdown.

**Scene objects needed:**

| GameObject | Component | Notes |
|---|---|---|
| `NetworkScene` | Ubiq `NetworkScene` | Must be present; connects all clients to the same room |
| `LobbyObject` | `LobbyManager` | `ConfigUrl` = relative or absolute URL of the config JSON; `StatusText` → TMP_Text |
| `Canvas` | — | Screen-space overlay |
| `StatusText` | `TMP_Text` | Displays connection count and countdown |

**`LobbyManager` flow:**
1. On `Start`, fetches `ConfigUrl` via `UnityWebRequest`. Writes `ExperimentId` and `DataCollectionServerURL` into `Database`.
2. Subscribes to Ubiq `OnPeerAdded` / `OnPeerRemoved` events and updates the status text.
3. When `Peers.Count >= requiredParticipants`, the first client to notice broadcasts a `"countdown_start"` signal over Ubiq.
4. All clients receive the signal (or trigger it locally if no peers) and start the countdown coroutine.
5. After `countdownSeconds`, loads `nextSceneName` (`"LoadTrial"` by default).

**Experiment config file** (`simulation_and_vr/vr_experiments/experiment_1_config.json`):

```json
{
  "experimentId": "experiment_1",
  "requiredParticipants": 2,
  "countdownSeconds": 5,
  "nextSceneName": "LoadTrial",
  "dataAssemblyUrl": "https://your-server.com/collect",
  "buildFolder": "WebGLBuild"
}
```

| Field | Description |
|---|---|
| `experimentId` | Written to `Database.ExperimentId`. Used as the shared random seed for trial ordering across all clients. |
| `requiredParticipants` | Minimum connected peers (including self) before the countdown starts. Set to `1` to skip waiting. |
| `countdownSeconds` | Seconds between "all connected" and loading the next scene. Gives latecomers a buffer. |
| `nextSceneName` | Scene to load after the countdown. |
| `dataAssemblyUrl` | Overrides `Database.DataCollectionServerURL`. Same as `dataAssemblyUrl` in `config.json`; can be set here alone if the Lobby is the first scene to run. |
| `buildFolder` | Name of the local WebGL build folder served by `serve_experiment.go`. Not used by the Unity build itself. |

**Deploying the config file:**
- **WebGL:** place `experiment_1_config.json` at the same URL root as the build (e.g. alongside `index.html`). `LobbyManager` resolves relative URLs against `Application.absoluteURL`.
- **Standalone:** place the file in `StreamingAssets/`. `LobbyManager` uses `Application.streamingAssetsPath` for relative paths.
- Absolute `https://` URLs work in both platforms.

**Single-participant fallback:** if no `NetworkScene` is found in the scene, `LobbyManager` skips peer counting and goes straight to the countdown. `requiredParticipants = 1` is the recommended value when not using Ubiq.

---

### 4. LoadTrial Scene

**Purpose:** Select and additively load the correct experimental scene based on participant group, while showing a loading progress bar.

**Scene objects needed:**

| GameObject | Component | Notes |
|---|---|---|
| `Loader` | `LoadTrial` | |
| `ProgressText` | `TMP_Text` | Optional; shows `"##0.0%"` |

**Wiring:**
- `LoadTrial.Trials` — list of scene names, one per group (e.g. `["Experiment_A", "Experiment_B"]`).
- `LoadTrial.Text` → the `TMP_Text` component (optional).

`LoadTrial` reads `Database.ParticipantGroup - 1` as the index into `Trials`. Group 0 (unassigned) defaults to index 0. Once loaded additively, it unloads itself.

---

### 4. Experimental Scene

**Purpose:** Run all trials. The participant navigates to targets; data is recorded automatically.

**Scene objects needed:**

| GameObject | Component | Notes |
|---|---|---|
| `FPSController` | `FirstPersonController` | Starts disabled |
| `Recorder` | `ParticipantRecorder` | `Camera` → FPS camera |
| `Spawnpoint` | Empty transform | Starting position for each trial |
| `TrialOverviewPanel` | `TrialOverview` | The between-trial UI hub |
| `Target_1` | `Target` | `Number` = 1, `Description` = "..." |
| `Target_2` | `Target` | `Number` = 2, `Description` = "..." |
| ... | | |
| `AdminHacks` | `AdminHacks` | Optional |

**`TrialOverview` Inspector fields:**

| Field | Value |
|---|---|
| `Repetitions` | How many times each target appears (e.g. 3) |
| `HeaderText` | UI Text for "Task Goal #N of M" |
| `DescriptionText` | UI Text for task instructions |
| `DescriptionImage` | Image showing target color |
| `HintText` | TMP_Text HUD hint (visible during trial) |
| `HintImage` | Image showing color during trial |
| `FPSController` | → FPS rig |
| `Recorder` | → ParticipantRecorder |
| `Spawnpoint` | → spawn transform |
| `NextSceneName` | `"End"` |

**Target materials:**
`TrialOverview` loads materials from `Resources/TargetMaterials/`. Create a folder at that path and add at least one material per target. The material name and color are shown to the participant as the visual cue.

**Trial loop:**
1. Scene loads → `TrialOverviewPanel.OnEnable()` fires → shows task description.
2. Participant reads and clicks "Start" → `OnApplyPage()` → FPS enabled, recording starts, `Database.StartNewTrial()` called.
3. Participant walks to the correct `Target` → `Target.OnTriggerEnter()` → `Database.EndTrial()` → `TrialOverviewPanel` re-activates.
4. Repeat until `tasks` list is empty → `LoadScene("End")`.

**Data streamed per trial:**
- Demographics (once per session, on first `StartNewTrial`)
- Trial header (target ID, material, start time)
- Tracking rows (position + azimuth/elevation, every frame `ParticipantRecorder` is recording)
- Trial tail (end time, total times, MD5 checksum)

---

### 5. End Scene

**Purpose:** Show a completion/redeem code and thank the participant.

**Scene objects needed:**

| GameObject | Component | Notes |
|---|---|---|
| `Canvas` | — | |
| `CodeField` | `InputField` + `GenerateRedeemCode` | `VerificationOutput` → self |
| `ThankYouText` | `Text` / `TMP_Text` | Static message |

`GenerateRedeemCode` auto-runs on `Start`, computes `MD5(ExperimentId + ParticipantGroup + "Science is cool!!")` and displays the first 4 bytes as hex in the `InputField`. The code is also copied to the clipboard.

---

## Server-Side Requirements

Your data collection server must accept HTTP POST requests at the URL specified in `config.json`. Each request body is a JSON object with one of the following shapes:

| Type | Key fields |
|---|---|
| User data | `id`, `sid`, `eid`, `age`, `ge`, `gr`, `da` |
| Trial header | `id`, `sid`, `tnum`, `td`, `tm`, `st` |
| Trial data | `id`, `sid`, `tnum`, `pid`, `DATA` (CSV rows) |
| Trial tail | `id`, `sid`, `tnum`, `pnum`, `checksum`, `et`, `tspan`, `tsofar`, `tsstart` |
| Meta data | `id`, `sid`, `eid`, `mid`, `cat`, `meta`, `date` |

Field definitions: `id`=participantId, `sid`=sessionId, `eid`=experimentId, `tnum`=trialIndex, `pid`=dataPacketIndex, `ge`=gender (m/f/o), `gr`=group, `da`=date.

---

## Build & Deployment Checklist

1. Add all scenes to **Build Settings** in order: Setup, Practice, Lobby *(multiplayer only)*, LoadTrial, Experimental scenes, End.
2. Place `config.json` at the root of your web server (same origin as the WebGL build). The file must contain `{ "dataAssemblyUrl": "..." }`.
3. Pass experiment parameters via URL: `?ExpID=MY_EXP&group=1&assignmentId=SESSION_ID&workerId=P001`.
4. To deploy multiple groups, add one experimental scene per group and list them in `LoadTrial.Trials` in order (group 1 → index 0, group 2 → index 1, etc.).
5. Place target materials in `Assets/Resources/TargetMaterials/`.
6. For the admin backdoor, type `6`, `4`, `8`, `5`, `9`, `9`, `7`, `2` in sequence during any scene.

---

## Serving the WebGL Build Locally

`simulation_and_vr/vr_experiments/serve_experiment.go` is a minimal Go file server for testing a WebGL build on your local machine.

**Prerequisites:** Go 1.20+ installed.

**Setup:**

1. Export a WebGL build from Unity and place the output folder next to `serve_experiment.go`.
2. Set `"buildFolder"` in `experiment_1_config.json` to the name of that folder.
3. Place `experiment_1_config.json` inside the build folder so the build can fetch it at runtime.

**Run:**

```bash
cd simulation_and_vr/vr_experiments
go run serve_experiment.go
```

The server listens on `http://localhost:8080`. Open the experiment with URL parameters:

```
http://localhost:8080/index.html?ExpID=AABBCC&group=1
```

This is equivalent to a production deployment — all URL parameters (`ExpID`, `group`, `assignmentId`, `workerId`) are parsed by `ParticipantInfoPage` exactly as they would be on a remote host.

> The file server is for **local testing only**. For deployed studies use a proper web host (nginx, S3 static hosting, etc.) and pair it with the Go data collection server described below.

---

## Minimal Working Example (single group, two targets)

```
Scenes: Setup → Practice → LoadTrial → Experiment_A → End

Experiment_A contains:
  - FPSController (disabled at start)
  - ParticipantRecorder (Camera = FPS camera)
  - Spawnpoint (empty transform at start location)
  - TrialOverviewPanel (TrialOverview, Repetitions=2, NextSceneName="End")
  - Target_1 (Target, Number=1, Description="the red room")
  - Target_2 (Target, Number=2, Description="the blue corridor")

Resources/TargetMaterials/ contains:
  - Red.mat
  - Blue.mat
  - Green.mat

LoadTrial.Trials = ["Experiment_A"]
```

With this setup, each participant will see 4 trials (2 targets × 2 repetitions), in a shuffled order seeded by their participant ID, and all tracking data will be uploaded to your server in real time.

---

## Multiplayer Collaborative VR Experiment with Ubiq

All participants share the same virtual space and **collaborate on the same trial simultaneously**. When any participant reaches the target, the trial ends for the entire group and all move to the next task together. Each client independently records and uploads its own movement data.

### Architecture

```
Client A                                  Client B
──────────────────────────────            ──────────────────────────────
Setup → Practice → LoadTrial              Setup → Practice → LoadTrial
    → Experimental scene                      → Experimental scene
         │                                         │
         ├─ TrialSyncManager ◄─── Ubiq ───► TrialSyncManager
         │    (same trial, same time)              │
         ├─ remote avatar B                  ├─ remote avatar A
         └─ Database A → server              └─ Database B → server
```

`TrialSyncManager` is a new `NetworkedBehaviour` that broadcasts two signals:

| Signal | Sent when | Effect on all clients |
|---|---|---|
| `"start"` | Any participant clicks the Start button | Enables movement, starts `ParticipantRecorder`, calls `Database.StartNewTrial()` |
| `"end"` | The first participant to reach the `Target` | Re-activates the `TrialOverview` panel, which calls `Database.EndTrial()` and loads the next task |

`Database` remains per-process. Ubiq carries only trial coordination signals and avatar transforms — no experiment data crosses the wire.

### Prerequisites

Install Ubiq before proceeding (see [Virtual Walkthrough §4.1](virtual_walkthrough.md#41-install-ubiq)).

### New and Modified Scripts

Three files were changed and one new script was added. All changes are backward-compatible: a scene without `TrialSyncManager` wired up falls back to single-player behaviour.

#### New: `Assets/Scripts/TrialSyncManager.cs`

Extends Ubiq's `NetworkedBehaviour`. Place it on a persistent GameObject in the experimental scene and wire `TrialOverview` in the Inspector.

- **`BroadcastTrialStart(targetId, materialName)`** — called by `TrialOverview.OnApplyPage()`. Sends a `"start"` message to all peers, then applies locally. The `isTrialActive` guard ensures duplicate signals (e.g. if two participants click simultaneously) are ignored.
- **`BroadcastTrialEnd()`** — called by `Target.OnTriggerEnter`. Sends a `"end"` message to all peers, then applies locally. The first call per trial wins; all subsequent ones are no-ops.
- **`ProcessMessage()`** — receives signals from remote peers and calls `ApplyTrialStart` / `ApplyTrialEnd` locally.

#### Modified: `Assets/Scripts/TrialOverview.cs`

| Change | Detail |
|---|---|
| `FirstPersonController` → `PlayerMovement` | Removes Standard Assets dependency; `enabled = true/false` is identical |
| `+` `public TrialSyncManager SyncManager` | Wire in Inspector for multiplayer; leave null for single-player |
| Seed changed | Uses `Database.ExperimentId` (shared) when `SyncManager != null`, falls back to `Database.ParticipantId` (per-participant) otherwise |
| `OnApplyPage()` | Calls `SyncManager.BroadcastTrialStart()` if wired, else applies directly |
| `+` `public void OnNetworkTrialStart(int, string)` | New method called on every client by `TrialSyncManager`. Contains all per-trial setup: hint UI update, spawn, enable movement, start recorder, `Database.StartNewTrial()` |

#### Modified: `Assets/Scripts/Target.cs`

| Change | Detail |
|---|---|
| `FirstPersonController` → `PlayerMovement` | Trigger check uses `GetComponent<PlayerMovement>()` |
| End logic | Calls `syncManager.BroadcastTrialEnd()` if a `TrialSyncManager` is in the scene; otherwise original `Database.EndTrial()` + panel activation |

#### Modified: `Assets/Scripts/PracticeScenePage.cs`

Same `FirstPersonController` → `PlayerMovement` swap as above. No other changes.

### Why the Shared Seed Matters

`TrialOverview.Awake()` shuffles the target list using a random seed. In single-player mode the seed is the participant's ID, giving each person their own order. In multiplayer mode it must be the same on every client so they all pick the same target in the same round.

When `SyncManager` is wired, the seed becomes `Database.ExperimentId.GetHashCode() / 2 + sceneName.GetHashCode() / 2`. Since `ExperimentId` is passed via URL parameter and is the same for all participants in a session, the shuffled order is identical on every machine.

### Local Player Setup

Replace the `FPSController` object in both the **Practice** and **Experimental** scenes with a new player GameObject. The root object needs:

| Component | Notes |
|---|---|
| `CharacterController` | Set Radius/Height to match your environment |
| `PlayerMovement` | `controller` → `CharacterController`; `groundCheck` → child empty at foot level; `layerMask` → floor layer |
| `UbiqNetworkedPlayer` | `isLocalPlayer = true` |
| `ParticipantRecorder` | `Camera` → the child Camera |

**Required children:**

| Child | Component | Notes |
|---|---|---|
| `FieldOfView` | `Camera` + `MouseTracker` | `MouseTracker.playerBody` → parent transform; `isLocalPlayer = true` |
| `GroundCheck` | Empty transform | At foot level |

Re-wire these Inspector fields to point at the new objects:
- `TrialOverview.FPSController` → `PlayerMovement`
- `TrialOverview.SyncManager` → `TrialSyncManager` ← **new**
- `PracticeScenePage.FPSController` → `PlayerMovement`
- `TestMouseMovement.Camera` → child Camera

### Remote Avatar Prefab

A lightweight prefab representing other participants' avatars. Same component stack as the local player but all `isLocalPlayer` flags set to `false`, and no experiment logic:

| Component | `isLocalPlayer` | Notes |
|---|---|---|
| `CharacterController` | — | Provides collider volume |
| `PlayerMovement` | `false` | Input disabled |
| `MouseTracker` | `false` | Input disabled; cursor unchanged |
| `UbiqNetworkedPlayer` | `false` | Driven by incoming network state |

Do **not** add `ParticipantRecorder`, `TrialSyncManager`, or `TrialOverview` to this prefab. Remote avatars cannot trigger `Target.OnTriggerEnter` because their `PlayerMovement` component is disabled and does not move under physics — only the local player's capsule is driven by the `CharacterController`.

### Add NetworkScene and TrialSyncManager to the Experimental Scene

1. Search for the `NetworkScene` prefab (installed by Ubiq) and drag it into the scene.
2. Create an empty GameObject named `TrialSync`. Add the `TrialSyncManager` component. Wire `TrialOverview` → the `TrialOverviewPanel` object.
3. For a private lab deployment, set `ServerUrl` and `Port` on the `NetworkScene` component.

The `NetworkScene` and `TrialSyncManager` are only needed in the experimental scene. Setup, Practice, LoadTrial, and End scenes do not need them.

### Peer Manager: Spawning Remote Avatars

Create a `PeerManager` MonoBehaviour on a new GameObject in the experimental scene:

```csharp
using UnityEngine;
using Ubiq.Peers;
using Ubiq.Messaging;
using System.Collections.Generic;

public class PeerManager : MonoBehaviour
{
    public GameObject remoteAvatarPrefab;

    private NetworkScene networkScene;
    private readonly Dictionary<string, GameObject> remoteAvatars = new();

    private void Start()
    {
        networkScene = NetworkScene.Find(this);
        networkScene.OnPeerAdded   += OnPeerAdded;
        networkScene.OnPeerRemoved += OnPeerRemoved;
    }

    private void OnPeerAdded(IPeer peer)
    {
        if (peer == networkScene.Me) return;
        var avatar = Instantiate(remoteAvatarPrefab);
        avatar.GetComponent<UbiqNetworkedPlayer>().SetOwnership(false);
        remoteAvatars[peer.UUID] = avatar;
    }

    private void OnPeerRemoved(IPeer peer)
    {
        if (remoteAvatars.TryGetValue(peer.UUID, out var avatar))
        {
            Destroy(avatar);
            remoteAvatars.Remove(peer.UUID);
        }
    }
}
```

> Ubiq's peer API (`OnPeerAdded`, `IPeer.UUID`) may differ slightly between package versions. Consult the installed Ubiq documentation if the above does not compile.

### Updated Experimental Scene Checklist

| Object | Components | Key Inspector wiring |
|---|---|---|
| `NetworkScene` | Ubiq `NetworkScene` | `ServerUrl` / `Port` if using private server |
| `LocalPlayer` | `CharacterController`, `PlayerMovement`, `UbiqNetworkedPlayer` (`isLocalPlayer=true`), `ParticipantRecorder` | — |
| `LocalPlayer/FieldOfView` | `Camera`, `MouseTracker` (`isLocalPlayer=true`) | `playerBody` → `LocalPlayer` |
| `LocalPlayer/GroundCheck` | Empty transform | — |
| `TrialSync` | `TrialSyncManager` | `TrialOverview` → `TrialOverviewPanel` |
| `PeerManagerObject` | `PeerManager` | `remoteAvatarPrefab` → remote avatar prefab |
| `Spawnpoint` | Empty transform | — |
| `TrialOverviewPanel` | `TrialOverview` | `FPSController` → `LocalPlayer.PlayerMovement`; `SyncManager` → `TrialSync.TrialSyncManager` |
| `Target_1 … N` | `Target` | Unchanged |

### Collaborative Trial Flow (end-to-end)

```
All clients load experimental scene
    └─► TrialOverviewPanel.OnEnable() on all clients
            ├─ Database.EndTrial() [no-op on first call]
            ├─ FPSController disabled, recorder stopped
            └─ Same target + material picked (shared seed)

Any participant clicks "Start"
    └─► TrialOverview.OnApplyPage()
            └─► TrialSyncManager.BroadcastTrialStart(targetId, materialName)
                    ├─► context.Send("start") to all peers
                    └─► ApplyTrialStart() locally
                              └─► OnNetworkTrialStart() on ALL clients
                                      ├─ FPSController enabled
                                      ├─ ParticipantRecorder.StartRecording()
                                      └─ Database.StartNewTrial()

First participant reaches Target
    └─► Target.OnTriggerEnter() [local player only]
            └─► TrialSyncManager.BroadcastTrialEnd()
                    ├─► context.Send("end") to all peers
                    └─► ApplyTrialEnd() locally
                              └─► TrialOverviewPanel.SetActive(true) on ALL clients
                                      └─► TrialOverview.OnEnable() on all clients
                                              └─ [repeat from top]
```

### Notes

- **Second participant reaching the target:** `BroadcastTrialEnd()` checks `isTrialActive` before sending. Once the first "end" signal sets `isTrialActive = false`, any subsequent trigger (another participant arriving late, or a duplicate network message) is silently dropped.
- **Practice and Setup scenes:** these do not include a `NetworkScene`, so avatar sync and trial sync only begin when the experimental scene loads. If earlier visibility is needed, add a `NetworkScene` and `PeerManager` to those scenes, but omit `TrialSyncManager` (trials only exist in the experimental scene).
- **Data per participant:** each client calls `Database.StartNewTrial()` and `Database.EndTrial()` independently with its own timestamps. Movement tracking (`ParticipantRecorder`) runs independently on each machine. All uploads go to the same server endpoint but are tagged with each client's `ParticipantId` and `SessionId`.

---

## Data Collection Backend

Two server implementations are provided in `simulation_and_vr/vr_experiments/data_collection_backend/`. Both expose the same HTTP endpoint that `DataUploadHandler` POSTs to, so they are interchangeable from Unity's perspective.

### Connecting Unity to the Server

`LobbyManager` reads `dataAssemblyUrl` from the experiment config JSON and writes it to `Database.DataCollectionServerURL`. `DataUploadHandler` (created by `Database.StartNewTrial`) reads that URL at runtime, so every participant in the session automatically POSTs to the correct server without any hard-coded URLs.

For WebGL single-participant runs without a lobby, `WebGLTools.FetchConfigJsonData()` can also set `Database.DataCollectionServerURL` from a `config.json` served alongside the build.

---

### Packet Format

`DataUploadHandler` sends five distinct JSON packet types. The server routes them by the presence of specific keys:

| Packet type | Routing key | Typical content |
|---|---|---|
| **UserData** | `ge` field present | Participant demographics |
| **TrialHeader** | `tnum` + `pid` present | Target ID, material, start time |
| **TrialData** | `tnum` + `st` present | Batched tracking rows (flushed ~4 KB) |
| **TrialTail** | `checksum` present | End time, durations, MD5 checksum |
| **MetaData** | `cat` present | Free-form annotations |

Files are written to `results/<participantId>/<sessionId>/` with one file per packet type per trial.

---

### Python Server (simple, disk-only)

**Location:** `data_collection_backend/python/`

**Requirements:** Python 3, Flask (`pip install flask`)

**Run:**
```bash
cd data_collection_backend/python
python server.py
# Listens on http://0.0.0.0:8080
```

The Flask app registers a single `POST /` handler. `packageHandler.py` inspects the JSON body, creates the directory tree `results/<id>/<sid>/`, and appends each packet to the appropriate file.

Use this for **local development** or small studies where disk-only storage is sufficient.

---

### Go Server (production, S3-backed)

**Location:** `data_collection_backend/go/`

**Requirements:** Go 1.20+, AWS credentials configured if S3 upload is enabled.

**Build and run:**
```bash
cd data_collection_backend/go
go build -o dataserver .
./dataserver                    # HTTP on :8080
./dataserver -secure            # HTTP :8080 + HTTPS :8443 (requires cert.pem / key.pem)
./dataserver -testUpload        # Run a synthetic upload test and exit
```

**Features beyond the Python server:**

| Feature | Detail |
|---|---|
| **MD5 validation** | On receipt of a `TrialTail` packet, the server recomputes the MD5 hash of all stored trial data and compares it against the `checksum` field. Mismatches are logged. |
| **Idempotent writes** | If a file already exists for a packet, the write is skipped — safe for retries without duplicating data. |
| **S3 archival** | After a tail packet passes validation, files are uploaded to S3 via the AWS SDK (`go/storage/storage.go`). The bucket and prefix are configurable via environment variables. |
| **Disk threshold** | A background goroutine monitors `results/` every 5 minutes. When total usage exceeds 100 MB, data is moved to S3 and local copies are removed. |
| **Concurrent sessions** | Uses a command channel for serialised writes, allowing multiple simultaneous participant connections without data corruption. |
| **HTTPS** | Pass `-secure` and place `cert.pem` / `key.pem` in the working directory. HTTP stays available on :8080 for internal health checks. |

**Alphanumeric sanitisation:** participant IDs and session IDs are stripped of all non-alphanumeric characters before use in file paths, preventing directory traversal attacks.

---

### Choosing a Backend

| | Python | Go |
|---|---|---|
| Setup effort | Minimal | Requires Go toolchain |
| Storage | Disk only | Disk + S3 |
| Data integrity | None | MD5 checksum |
| Production use | Not recommended | Yes |
| HTTPS | No | Yes (`-secure`) |

For **pilot studies and local testing**, use the Python server.  
For **deployed experiments** (MTurk, remote participants, long studies), use the Go server with S3 configured.
