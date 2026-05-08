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
