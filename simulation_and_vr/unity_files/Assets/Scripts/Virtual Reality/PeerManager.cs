// Uncomment once the Ubiq package is present in the project:
#define UBIQ_PRESENT

using System;
using System.Collections.Generic;
using UnityEngine;

#if UBIQ_PRESENT
using Ubiq.Rooms;
#endif

public class PeerManager : MonoBehaviour
{
    [Tooltip("Prefab instantiated for each remote peer joining the room.")]
    public GameObject remoteAvatarPrefab;

#if UBIQ_PRESENT
    private RoomClient roomClient;
    private readonly Dictionary<string, GameObject> peerAvatars = new Dictionary<string, GameObject>();

    private void Start()
    {
        roomClient = RoomClient.Find(this);

        if (roomClient == null)
        {
            Debug.LogError("[PeerManager] No RoomClient found in scene. Remote avatars will not be spawned.");
            return;
        }

        roomClient.timeoutBehaviour = RoomClient.TimeoutBehaviour.None;

        roomClient.Join(new Guid("4b5e1f8a-3c2d-4a9e-b1f6-7d8c0e3a9f2b"));

        roomClient.OnPeerAdded.AddListener(OnPeerAdded);
        roomClient.OnPeerRemoved.AddListener(OnPeerRemoved);
    }

    private void OnDestroy()
    {
        if (roomClient != null)
        {
            roomClient.OnPeerAdded.RemoveListener(OnPeerAdded);
            roomClient.OnPeerRemoved.RemoveListener(OnPeerRemoved);
        }

        foreach (var avatar in peerAvatars.Values)
        {
            if (avatar != null)
            {
                Destroy(avatar);
            }
        }

        peerAvatars.Clear();
    }

    private void OnPeerAdded(IPeer peer)
    {
        if (remoteAvatarPrefab == null)
        {
            Debug.LogError("[PeerManager] remoteAvatarPrefab is not assigned.");
            return;
        }

        if (peer == roomClient.Me)
        {
            return;
        }

        var uuid = peer.uuid;

        if (peerAvatars.ContainsKey(uuid))
        {
            Debug.LogWarning($"[PeerManager] Avatar for peer {uuid} already exists.");
            return;
        }

        var instance = Instantiate(remoteAvatarPrefab, transform);
        instance.name = $"RemoteAvatar_{uuid}";

        SetOwnership(instance, false);

        peerAvatars[uuid] = instance;
        Debug.Log($"[PeerManager] Spawned remote avatar for peer {uuid}.");
    }

    private void OnPeerRemoved(IPeer peer)
    {
        var uuid = peer.uuid;

        if (peerAvatars.TryGetValue(uuid, out var instance))
        {
            if (instance != null)
            {
                Destroy(instance);
            }

            peerAvatars.Remove(uuid);
            Debug.Log($"[PeerManager] Destroyed remote avatar for peer {uuid}.");
        }
    }

    private static void SetOwnership(GameObject instance, bool isLocal)
    {
        var networkedPlayer = instance.GetComponent<UbiqNetworkedPlayer>();
        if (networkedPlayer != null)
        {
            networkedPlayer.SetOwnership(isLocal);
        }

        var playerMovement = instance.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.isLocalPlayer = isLocal;
        }

        var mouseTracker = instance.GetComponentInChildren<MouseTracker>();
        if (mouseTracker != null)
        {
            mouseTracker.isLocalPlayer = isLocal;
        }

        var camera = instance.GetComponentInChildren<Camera>();
        if (camera != null)
        {
            camera.enabled = isLocal;
        }
    }

#else
    private void Start()
    {
        Debug.LogWarning("[PeerManager] Ubiq package not present, PeerManager disabled.");
    }
#endif
}
