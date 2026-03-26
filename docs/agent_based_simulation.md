# Agent-Based Simulation

This guide covers how to set up a simulation scene in Unity, author JSON configuration files, and run large batches of simulations headlessly using the provided scripts.

---

## How It Works

Each simulation runs a set of **tasks**. A task defines a group of autonomous agents that share the same behaviour: where they spawn (`start`), where they go (`pointsOfInterest`), and where they exit (`end`). Agents navigate the environment using Unity's NavMesh. The engine records each agent's full trajectory and collision counts, then writes a CSV file on exit.

**Boot sequence:**

```
BootstrapBatchSimulation scene
    └─ ConfigLoader (reads -config=<path>.json from CLI or Inspector)
            └─ ConfigManager (singleton, survives scene load)
                    └─ loads Scene named in config
                            └─ EngineScript (overrides all TaskScript values from config)
                                    └─ spawns agents → runs until all agents finish → Application.Quit()
```

---

## 1. Prepare the Unity Scene

### Environment requirements

- Import your floor plan / 3D model into the scene.
- Mark all walkable geometry as **Static**.
- Open `Window > AI > Navigation` and **Bake** the NavMesh. Tune the agent radius to match your corridors.

### Scene GameObjects

| GameObject | Components | Purpose |
|---|---|---|
| `Engine` | `EngineScript`, one or more `TaskScript` | Simulation controller |
| `ConfigLoader` | `ConfigLoader`, `ConfigManager` | Reads JSON config and passes it to the engine |
| `<POI group>` | Empty parent with child empties | Defines spawn, exit, and interest locations |

**POI naming convention:** `EngineScript.PopulateObjectArray` resolves names from the JSON config by calling `GameObject.Find()`. If a named object has children, all children are used as individual POI locations. Name your parent objects descriptively (e.g. `Room`, `Circulation`, `Stopby`).

### EngineScript Inspector fields

| Field | Description |
|---|---|
| `dataFolder` | Output directory (overridden by config in batch mode) |
| `fileName` | Output file base name (overridden by config in batch mode) |
| `sampleInterval` | Seconds between position samples (overridden by config) |
| `visualizePOIs` | Show cylinder + label above each POI |
| `visualizeTrajectories` | Draw agent trails with `LineRenderer` |
| `visualizePaths` | Draw planned NavMesh paths |
| `fontSize` / `resize` / `offset` | POI label appearance |
| `traceLength` | Number of past positions shown in trajectory visualization |

> In batch mode all IO fields (`dataFolder`, `fileName`, `sampleInterval`) are overridden from the JSON config. The visualization flags are respected but are irrelevant for headless runs.

### TaskScript Inspector fields (set in Unity or via JSON config)

| Field | Type | Description |
|---|---|---|
| `taskName` | string | Label for this group of agents |
| `agentType` | string | Category string (used in collision output columns) |
| `numberOfAgents` | int | Total agents to spawn |
| `spawnInterval` | float | Seconds between successive agent spawns |
| `start` | GameObject[] | Spawn locations (children of named parent) |
| `end` | GameObject[] | Exit locations |
| `pointsOfInterest` | GameObject[] | Locations the agent visits before exiting |
| `numberOfNeeds` | int | How many POIs each agent visits |
| `revisit` | bool | Whether an agent can return to an already-visited POI |
| `chooseNonDeterministically` | bool | Random POI selection vs sequential |
| `poiTime` | float | Seconds the agent spends at each POI |
| `agentSpeed` | float | NavMesh movement speed (m/s) |
| `agentSize` | float | Visual capsule scale |
| `agentRadius` | float | NavMesh avoidance radius |
| `privacyRadius` | float | Distance threshold for counting a "collision" with another agent |
| `taskColor` | Color | Agent colour for visualization |

**Constraint:** if `revisit = false` and `chooseNonDeterministically = false`, you need `#POIs ≥ numberOfNeeds`.

---

## 2. Write a JSON Config File

The JSON maps directly onto `ConfigData` and a list of `TaskData` objects.

```json
{
  "EngineScript_sampleInterval": 1,
  "Scene": "MyFloorPlan",
  "simId": 1,
  "sampleNum": 0,
  "Scenario": "Baseline",
  "Purpose": "PilotStudy",
  "tasks": [
    {
      "taskName": "Staff",
      "agentType": "Staff",
      "numberOfAgents": 20,
      "spawnInterval": 2,
      "start": "StaffEntrance",
      "end": "StaffEntrance",
      "pointsOfInterest": "NurseStation, PatientRoom",
      "numberOfNeeds": 2,
      "revisit": false,
      "chooseNonDeterministically": false,
      "poiTime": 5,
      "agentSpeed": 1.4,
      "agentSize": 0.8,
      "agentRadius": 0.3,
      "privacyRadius": 1.0,
      "taskColor": "blue"
    },
    {
      "taskName": "Visitor",
      "agentType": "Visitor",
      "numberOfAgents": 50,
      "spawnInterval": 1,
      "start": "MainEntrance",
      "end": "MainEntrance",
      "pointsOfInterest": "PatientRoom",
      "numberOfNeeds": 1,
      "revisit": true,
      "chooseNonDeterministically": true,
      "poiTime": 10,
      "agentSpeed": 1.0,
      "agentSize": 0.8,
      "agentRadius": 0.3,
      "privacyRadius": 1.2,
      "taskColor": "orange"
    }
  ]
}
```

**Field notes:**
- `Scene` — must exactly match the Unity scene name in Build Settings.
- `simId` — used as the random seed (`Random.InitState(simId)`), so two runs with the same `simId` produce identical results.
- `sampleNum` — a second index for repeated runs with different seeds; appears in the output filename.
- `start`, `end`, `pointsOfInterest` — comma-separated GameObject names. Each name resolves to the parent's children if it has any.
- `taskColor` — accepts named colors (`red`, `blue`, `black`, `orange`, `purple`, `pink`, `brown`, `gray`, `cyan`, `magenta`, `yellow`, `white`), hex (`#FF5733`), or RGB (`255,128,0`).

---

## 3. Output

The engine writes one CSV per simulation run to:

```
data_abm_batch/<purpose>/<scene>/<scenario>/<purpose>_batch_simId_<simId>_sampleNum_<sampleNum>.csv
```

All names are lowercased and spaces replaced with underscores.

**CSV structure (semicolon-separated):**

| Column | Description |
|---|---|
| `AgentType` | From `agentType` field |
| `TaskName` | From `taskName` field |
| `Distance` | Total path length (sum of step distances) |
| `Collisions` | Total privacy-radius violations with agents of a different type |
| `Collisions_<TaskName>` | Per-task collision counts (one column per task) |
| `Duration` | Time from spawn until exit |
| `pos0x, pos0y, pos0z, pos1x, ...` | Position samples at each `sampleInterval` |

Rows with fewer samples than the longest trajectory are padded with empty fields.

---

## 4. Batch Config Generation

The Jupyter notebooks in `simulation_and_vr/simulation_experiments/` generate many JSON configs from a spreadsheet.

### Spreadsheet format

Create a CSV with one row per simulation condition (or one row per task if a simulation has multiple task types). Required columns:

| Column | Maps to |
|---|---|
| `Scene` | `ConfigData.Scene` |
| `Scenario` | `ConfigData.Scenario` |
| `Purpose` | `ConfigData.Purpose` |
| `EngineScript_sampleInterval` | `ConfigData.EngineScript_sampleInterval` |
| `simId` | `ConfigData.simId` |
| `totalSimSamples` | Number of repeated runs (becomes separate JSON files with `sampleNum` 0…N-1) |
| `TaskScript_taskName` | `TaskData.taskName` |
| `TaskScript_agentType` | `TaskData.agentType` |
| `TaskScript_numberOfAgents` | `TaskData.numberOfAgents` |
| `TaskScript_spawnInterval` | `TaskData.spawnInterval` |
| `TaskScript_start` | `TaskData.start` |
| `TaskScript_end` | `TaskData.end` |
| `TaskScript_pointsOfInterest` | `TaskData.pointsOfInterest` |
| `TaskScript_numberOfNeeds` | `TaskData.numberOfNeeds` |
| `TaskScript_agentSpeed` | `TaskData.agentSpeed` |
| `TaskScript_agentSize` | `TaskData.agentSize` |
| `TaskScript_agentRadius` | `TaskData.agentRadius` |
| `TaskScript_privacyRadius` | `TaskData.privacyRadius` |
| `TaskScript_poiTime` | `TaskData.poiTime` |
| `TaskScript_revisit` | `TaskData.revisit` |
| `TaskScript_chooseNonDeterministically` | `TaskData.chooseNonDeterministically` |

Rows with the **same `simId`** are grouped into a single simulation with multiple tasks.

### Running the notebook

```bash
cd simulation_and_vr/simulation_experiments/
jupyter notebook create_batch_configs_new.ipynb
```

Edit the two variables at the top of the notebook:
```python
csv_dir = "Simulation setup 202503"   # folder containing your CSV files
output_base_dir = "output_json"        # where JSON files will be written
```

Output structure:
```
output_json/
└── <prefix>/                          # from the CSV filename before the first "-"
    ├── <prefix>_simId_0_sampleNum_0.json
    ├── <prefix>_simId_0_sampleNum_1.json
    ├── <prefix>_simId_1_sampleNum_0.json
    └── ...
```

The run scripts expect this exact folder structure and filename pattern (`*simId*sampleNum*.json`).

---

## 5. Build the Standalone Executable

Before running batch simulations you need a **headless standalone build**.

1. In Unity: `File > Build Settings`
2. Add the `BootstrapBatchSimulation` scene (and all experimental scenes referenced in your configs) to the build list.
3. Target platform: **Windows** (for `.exe`) or **Linux** (for headless server builds).
4. Build. The executable is what the run scripts invoke as `EBD-Toolkit-HS4U-20241108.exe` (rename or update the scripts to match your build name).

---

## 6. Run Batch Simulations

Both `run_parallel.sh` (Linux/macOS) and `run_parallel.ps1` (Windows) iterate over all JSON files matching `output_json/**/*_simId_*_sampleNum_*.json`, launching up to N simulations in parallel.

### Linux / macOS — `run_parallel.sh`

```bash
chmod +x run_parallel.sh

./run_parallel.sh \
  -e MyBuild.x86_64 \
  -w /path/to/build/folder \
  -j output_json \
  -l sim_logs \
  -p 8
```

**Options:**

| Flag | Default | Description |
|---|---|---|
| `-e` / `--exe` | `EBD-Toolkit-HS4U-20241108.exe` | Executable name or path |
| `-w` / `--workdir` | Current directory | Working directory (must contain the executable) |
| `-j` / `--jsondir` | `output_json` | Directory containing the generated JSON files |
| `-l` / `--logdir` | `sim_logs` | Directory for per-simulation log files |
| `-p` / `--parallel` | `nproc` (all cores) | Maximum number of simultaneous simulations |

The script waits with `while [ running_jobs -ge MAX_PARALLEL ]; do sleep 1; done` before launching each new job, then `wait` at the end.

### Windows — `run_parallel.ps1`

1. Open the script and set `$workingDirectory` at the top:
   ```powershell
   $workingDirectory = "C:\path\to\build\folder"
   ```
2. Run from PowerShell:
   ```powershell
   Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
   .\run_parallel.ps1
   ```

The script defaults to **8 parallel jobs** (`$MaxParallelJobs = 8`). Edit that variable to match your CPU count.

Log files are written to `sim_logs\` with names like:
```
sim_logs/<prefix>_loop_<i>_simId_<id>_sampleNum_<n>.txt
```

### What each simulation process receives

The executable is launched with:
```
./MyBuild.exe -config="<absolute_path_to_json>" -logFile "<log_path>"
```

`ConfigLoader` reads `-config=` from `Environment.GetCommandLineArgs()` on startup and passes the parsed `ConfigData` to `ConfigManager`. `EngineScript` then reads from `ConfigManager.Instance.Config` to override all its parameters before spawning agents.

---

## 7. Complete Workflow Summary

```
1. Design your floor plan → import FBX → bake NavMesh in Unity
2. Place POI GameObjects (parents with child empties) and name them
3. Add BootstrapBatchSimulation scene with ConfigLoader + ConfigManager + EngineScript
4. Author experiment conditions in a CSV spreadsheet
5. Run create_batch_configs_new.ipynb → generates output_json/<prefix>/
6. Build standalone executable
7. Run run_parallel.sh or run_parallel.ps1 → launches N parallel headless sims
8. Collect CSVs from data_abm_batch/ for analysis
```

---

## 8. Testing a Single Config in the Editor

Without building, you can test one config in the Unity Editor:

1. Open the `BootstrapBatchSimulation` scene.
2. Select the `ConfigLoader` GameObject.
3. Set `configFilePathEditor` in the Inspector to an absolute path to your JSON file.
4. Press **Play**. The engine will load the scene named in the config, run the simulation, and exit play mode when all agents are done.
5. The output CSV is written relative to `Application.dataPath`.
