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
using System.Collections;
using Assets.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Linq;

#if UBIQ_PRESENT
using Ubiq.Messaging;
using Ubiq.Rooms; // [FIX 1]: Updated from Ubiq.Peers to Ubiq.Rooms
#endif

/// <summary>
/// Manages the lobby scene that gates entry into the collaborative VR experiment.
/// </summary>
public class LobbyManager :
#if UBIQ_PRESENT
    NetworkedBehaviour
#else
    MonoBehaviour
#endif
{
    [Tooltip("URL of the experiment config JSON, relative to the streaming-assets root " +
             "or an absolute URL. E.g. 'experiment_1_config.json'.")]
    public string ConfigUrl = "experiment_1_config.json";

    [Tooltip("TMP_Text element used to display lobby status to the participant.")]
    public TMP_Text StatusText;

    [Tooltip("TMP_Text element used to display the big countdown numbers.")]
    public TMP_Text CountdownText;

    public string nextSceneName = "LoadTrial";
    public GameObject interactionDot;

    // -----------------------------------------------------------------
    // Internal state
    // -----------------------------------------------------------------

    private VrExperimentConfig config;

#if UBIQ_PRESENT
    private NetworkScene networkScene;
    private RoomClient roomClient; // [FIX 2]: Added RoomClient for new Ubiq API
#endif

    private bool countdownStarted;

    [Serializable]
    private struct LobbyMessage
    {
        public string type; // "countdown_start"
    }

    // -----------------------------------------------------------------
    // Unity lifecycle
    // -----------------------------------------------------------------

    private IEnumerator Start()
    {
        // 自动补救：如果引用丢失，尝试通过名字寻找
        if (StatusText == null) StatusText = GameObject.Find("StatusText")?.GetComponent<TMP_Text>();
        if (CountdownText == null) CountdownText = GameObject.Find("CountdownDisplay")?.GetComponent<TMP_Text>();

        if (interactionDot != null) interactionDot.SetActive(true);
        SetStatus("Loading experiment configuration\u2026");

        yield return FetchConfig();

        if (config == null)
        {
            SetStatus("Error: could not load experiment configuration.\nCheck the console and verify ConfigUrl.");
            yield break;
        }

        // Push shared state into the static Database so every subsequent scene
        // inherits the correct ExperimentId and server URL.
        Database.ExperimentId = config.experimentId;
        if (!string.IsNullOrWhiteSpace(config.dataAssemblyUrl))
        {
            Database.DataCollectionServerURL = config.dataAssemblyUrl;
        }

#if UBIQ_PRESENT
        networkScene = NetworkScene.Find(this);
        roomClient = RoomClient.Find(this); // Find the new RoomClient

        if (networkScene == null || roomClient == null)
        {
            Debug.LogWarning("LobbyManager: No Ubiq NetworkScene or RoomClient found in scene. " +
                             "Treating as single-participant session.");
            StartCountdown();
            yield break;
        }

        // Subscribe to peer changes using RoomClient instead of NetworkScene
        roomClient.OnPeerAdded.AddListener(OnPeerCountChanged);
        roomClient.OnPeerRemoved.AddListener(OnPeerCountChanged);
        CheckPeerCount();
#else
        // Ubiq not available: treat as single-participant, skip straight to countdown.
        StartCountdown();
#endif
    }

    // -----------------------------------------------------------------
    // Peer counting (Ubiq path)
    // -----------------------------------------------------------------

#if UBIQ_PRESENT
    private void OnPeerCountChanged(IPeer _) => CheckPeerCount();

    private void CheckPeerCount()
    {
        if (countdownStarted) return;

        var connected = roomClient.Peers.Count() + 1; 
        var required  = config?.requiredParticipants ?? 1;

        // 更新状态：加入成功 + 动态人数
        SetStatus($"Joined Room Successed!\nWaiting for participants... {connected} / {required} connected");

        if (connected >= required)
        {
            // First peer to notice broadcasts so all clients get the same signal.
            var msg = new LobbyMessage { type = "countdown_start" };
            context.Send(JsonUtility.ToJson(msg));
            StartCountdown();
        }
    }

    // -----------------------------------------------------------------
    // Ubiq message handling
    // -----------------------------------------------------------------

    public override void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = JsonUtility.FromJson<LobbyMessage>(message.ToString());
        if (msg.type == "countdown_start")
        {
            StartCountdown();
        }
    }
#endif

    // -----------------------------------------------------------------
    // Countdown and scene transition
    // -----------------------------------------------------------------

    private void StartCountdown()
    {
        if (countdownStarted) return;
        countdownStarted = true;
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        var seconds = Mathf.Max(1, Mathf.RoundToInt(config?.countdownSeconds ?? 3f));

        if (CountdownText != null)
        {
            CountdownText.gameObject.SetActive(true);
        }

        for (var i = seconds; i > 0; i--)
        {
            SetStatus($"Joined Room Successed!\nStarting in {i}\u2026");
            if (CountdownText != null)
            {
                CountdownText.text = i.ToString();
            }
            yield return new WaitForSeconds(1f);
        }

        if (CountdownText != null)
        {
            CountdownText.text = "GO!";
        }

        var scene = config?.nextSceneName;
        if (string.IsNullOrWhiteSpace(scene))
        {
            Debug.LogError("LobbyManager: nextSceneName is not set in the experiment config.");
            yield break;
        }

        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    // -----------------------------------------------------------------
    // Config fetching
    // -----------------------------------------------------------------

    private IEnumerator FetchConfig()
    {
        var url = ConfigUrl;

        // Relative URLs are resolved against StreamingAssets in standalone builds
        // and against Application.absoluteURL in WebGL builds.
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("file", StringComparison.OrdinalIgnoreCase))
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // Same origin as the WebGL page.
                var origin = Application.absoluteURL;
                var lastSlash = origin.LastIndexOf('/');
                url = (lastSlash >= 0 ? origin.Substring(0, lastSlash + 1) : origin) + url;
            }
            else
            {
                url = System.IO.Path.Combine(Application.streamingAssetsPath, url);
            }
        }

        using (var request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"LobbyManager: Failed to fetch config from '{url}'.\n{request.error}");
                yield break;
            }

            config = JsonUtility.FromJson<VrExperimentConfig>(request.downloadHandler.text);

            if (config == null)
            {
                Debug.LogError("LobbyManager: Config JSON parsed to null. Check the file format.");
            }
        }
    }

    // -----------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------

    private void SetStatus(string text)
    {
        if (StatusText != null)
            StatusText.text = text;

        Debug.Log($"[Lobby] {text}");
    }
}