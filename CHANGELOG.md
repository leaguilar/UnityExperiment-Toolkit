# Changelog

## [2026-08-11] - Single-player pipeline → Multiplayer → 8-step Medical Workflow

### Milestone 1: Single-player full pipeline working
- All 6 scenes (Setup → Practice → Lobby → LoadTrial → Experimental → End) run end-to-end
- Tracking data written in real time to the Flask backend (`results/` folder)
- Fixed: Canvas renderMode WorldSpace → Screen Space Overlay
- Fixed: mouse look — playerBody was pointing to the wrong mesh node, preventing yaw
- Fixed: PlayerMovement groundCheck auto-find + layerMask set to Everything to prevent sinking

### Milestone 2: Multiplayer networking
- Connected to Ubiq RoomServer (127.0.0.1:8009)
- Fixed shared room GUID so all clients join the same room
- Fixed Build issues: stale Unity Services project ID, 3 compile errors, Lobby countdown blocked by `context.Send()` NullRef
- Added PeerManager + RemoteAvatar for automatic remote avatar spawning

### Milestone 3: 8-step medical workflow
- Patient flow: reception → bed placement → registration → diagnosis → treatment → PCS update → final visit → discharge
- Added PatientAgent.cs (name/age/condition, NavMesh walking, info panel)
- Target.cs: added requireInteraction + keepMeshVisible + onTargetCompleted
- TrialOverview: randomizeOrder=false for linear execution + step HUD hints

### Documentation
- progress_summary.md / progress_summary_EN.md
- progress_summary_2.md / progress_summary_2_EN.md
- experimental_scene_setup_log.md / _EN.md
- CHANGELOG.md
