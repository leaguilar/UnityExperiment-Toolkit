/*
DesignMind2: A Toolkit for Evidence-Based, Cognitively-Informed and Human-Centered Architectural Design
Copyright (C) 2023-2026  michal Gath-Morad, Christoph Hölscher, Raphaël Baur, Leonel Aguilar

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
*/

// Uncomment once the Ubiq package is present in the project:
#define UBIQ_PRESENT

using System;
using UnityEngine;

#if UBIQ_PRESENT
using Ubiq.Messaging;
using Ubiq.Rooms;
using Ubiq.Networking;
#endif

namespace Assets.Scripts
{
    /// <summary>
    /// Synchronises trial start and end events across all Ubiq peers so that
    /// every participant in a collaborative session executes the same trial
    /// at the same time.
    ///
    /// Place this on a persistent GameObject in the experimental scene alongside
    /// the Ubiq NetworkScene. Wire TrialOverview in the Inspector.
    ///
    /// All clients must generate the same trial order. TrialOverview achieves
    /// this by seeding Unity's RNG with the shared ExperimentId rather than the
    /// per-participant ParticipantId.
    ///
    /// Flow:
    ///   1. TrialOverview shows the between-trial panel on all clients.
    ///   2. Any participant clicks Start → TrialOverview calls BroadcastTrialStart().
    ///   3. All clients (including sender) receive the signal and call
    ///      TrialOverview.OnNetworkTrialStart(), which enables movement and begins
    ///      recording independently on each client.
    ///   4. The first participant to reach the Target calls BroadcastTrialEnd().
    ///   5. All clients receive the signal, re-activate the TrialOverview panel,
    ///      and the loop continues.
    /// </summary>
    public class TrialSyncManager :
#if UBIQ_PRESENT
        NetworkedBehaviour
#else
        MonoBehaviour
#endif
    {
        [Tooltip("The TrialOverview in this scene. Set in Inspector.")]
        public TrialOverview TrialOverview;

        // Guards against duplicate start/end signals arriving from multiple peers.
        private bool isTrialActive;

        [Serializable]
        private struct SyncMessage
        {
            public string type;         // "start" or "end"
            public int    targetId;
            public string materialName;
        }

        // ---------------------------------------------------------------------
        // Public API — called by TrialOverview and Target
        // ---------------------------------------------------------------------

        /// <summary>
        /// Called by TrialOverview when the local participant clicks Start.
        /// Broadcasts to all peers, then applies locally.
        /// </summary>
        public void BroadcastTrialStart(int targetId, string materialName)
        {
#if UBIQ_PRESENT
            // 核心修复：改用 bool 标志判断
            if (isNetworkActive)
            {
                var msg = new SyncMessage { type = "start", targetId = targetId, materialName = materialName };
                context.Send(JsonUtility.ToJson(msg));
            }
            else
            {
                Debug.LogWarning("[TrialSyncManager] 网络未就绪，正在以单机模式启动任务。");
            }
#endif
            ApplyTrialStart(targetId, materialName);
        }

        public void BroadcastTrialEnd()
        {
            if (!isTrialActive) return;
#if UBIQ_PRESENT
            // 核心修复：改用 bool 标志判断
            if (isNetworkActive)
            {
                var msg = new SyncMessage { type = "end" };
                context.Send(JsonUtility.ToJson(msg));
            }
#endif
            ApplyTrialEnd();
        }

        // ---------------------------------------------------------------------
        // Ubiq message handling
        // ---------------------------------------------------------------------

#if UBIQ_PRESENT
        public override void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var msg = JsonUtility.FromJson<SyncMessage>(message.ToString());            switch (msg.type)
            {
                case "start": ApplyTrialStart(msg.targetId, msg.materialName); break;
                case "end":   ApplyTrialEnd();                                 break;
            }
        }
#endif

        // ---------------------------------------------------------------------
        // Local application (called on every client, sender and receivers alike)
        // ---------------------------------------------------------------------

        private void ApplyTrialStart(int targetId, string materialName)
        {
            if (isTrialActive) return;   // ignore duplicate start signals
            isTrialActive = true;
            TrialOverview.OnNetworkTrialStart(targetId, materialName);
        }

        private void ApplyTrialEnd()
        {
            if (!isTrialActive) return;
            isTrialActive = false;
            Database.EndTrial();
            if (TrialOverview.showPanelBetweenTasks)
                TrialOverview.gameObject.SetActive(true);
            else
                TrialOverview.AutoStartNextTrial();
        }
    }
}
