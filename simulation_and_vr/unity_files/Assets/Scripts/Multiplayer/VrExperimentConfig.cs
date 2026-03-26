/*
DesignMind2: A Toolkit for Evidence-Based, Cognitively-Informed and Human-Centered Architectural Design
Copyright (C) 2023-2026  michal Gath-Morad, Christoph Hölscher, Raphaël Baur, Leonel Aguilar

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
*/

using System;

namespace Assets.Scripts
{
    /// <summary>
    /// Mirrors the JSON structure of experiment_N_config.json.
    /// Loaded at runtime by LobbyManager via UnityWebRequest.
    /// </summary>
    [Serializable]
    public class VrExperimentConfig
    {
        /// <summary>Unique identifier for this experiment. Written to Database.ExperimentId
        /// and used as the shared random seed for trial ordering in multiplayer.</summary>
        public string experimentId;

        /// <summary>Number of participants that must be connected before the lobby
        /// transitions to the experiment. Set to 1 for single-participant runs.</summary>
        public int requiredParticipants;

        /// <summary>Seconds to count down after all participants have joined, giving
        /// latecomers a moment to finish loading before the trial scene opens.</summary>
        public float countdownSeconds;

        /// <summary>Unity scene name to load after the countdown. Typically "LoadTrial".</summary>
        public string nextSceneName;

        /// <summary>URL of the data collection server endpoint. Stored in
        /// Database.DataCollectionServerURL so DataUploadHandler can POST to it.</summary>
        public string dataAssemblyUrl;
    }
}
