# Simulation and Data Naming Convention: ED Layout Study

This document defines the naming strategy for the 2x2 Factorial Design experiment evaluating Visibility and Accessibility in Emergency Department (ED) layouts. These names are used in Unity Scene files and ABM JSON configurations to ensure automated data categorization.

## Experimental Matrix

| Condition ID | Experimental Condition | Unity Scene Name (`Scene`) | Data Scenario (`Scenario`) | Architectural Characteristics |
| :--- | :--- | :--- | :--- | :--- |
| **0** | **Baseline** | `ed_baseline` | `baseline` | The original ED layout used as a reference for all behavioral metrics. |
| **1** | **High Visibility + High Accessibility** | `ed_vish_acch` | `vish_acch` | Open cockpit structure (e.g., glass walls) with multiple, unobstructed entry points. |
| **2** | **High Visibility + Low Accessibility** | `ed_vish_accl` | `vish_accl` | Transparent cockpit interior, but with limited or indirect physical access paths. |
| **3** | **Low Visibility + High Accessibility** | `ed_visl_acch` | `visl_acch` | Opaque cockpit walls obstructing sightlines, but with many direct physical entry points. |
| **4** | **Low Visibility + Low Accessibility** | `ed_visl_accl` | `visl_accl` | Most enclosed state: obstructed sightlines and restricted, circuitous entry paths. |

---

## Field Definitions

### 1. Unity Scene Name (`Scene`)
- **Target**: Unity Engine
- **Logic**: Must exactly match the `.unity` file name in the project.
- **Function**: Used by the `ConfigLoader` to identify which virtual environment to load for the simulation.

### 2. Data Scenario (`Scenario`)
- **Target**: File System / Data Analysis
- **Logic**: Defines the folder structure for simulation output.
- **Function**: Output files will be stored in:  
  `data_abm_batch / [Purpose] / [Scene] / [Scenario] / ...`
- **Benefit**: Facilitates automated statistical analysis (e.g., using Python/Pandas) by grouping results by experimental condition.

## Technical Requirements
- Use **lowercase letters** only.
- Replace spaces with **underscores** (`_`).
- Maintain the same **POI (Points of Interest)** names across all scenes to ensure spatial data alignment.
