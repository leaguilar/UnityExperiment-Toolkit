using System;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ubiq.Samples
{
    public class JoinAllRoomClients : MonoBehaviour
    {
        private void Start()
        {
            var guid = new System.Guid("4b5e1f8a-3c2d-4a9e-b1f6-7d8c0e3a9f2b");
            foreach (var roomClient in FindObjectsByType<RoomClient>(FindObjectsSortMode.None))
            {
                roomClient.Join(guid);
            }
        }
    }
}