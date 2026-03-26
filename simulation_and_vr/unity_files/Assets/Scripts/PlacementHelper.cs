using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace Assets.Scripts
{
    public static class PlacementHelper
    {
        public static void PlaceObject(this GameObject go, Vector3 position, Quaternion rotation)
        {
            var charController = go.GetComponent<CharacterController>();
            charController.enabled = false;

            var camera = go.GetComponentInChildren<Camera>();
            camera.transform.localRotation = Quaternion.identity;

            go.transform.position = position;
            go.transform.rotation = rotation;

            var fpsController = go.GetComponent<FirstPersonController>();
            if (fpsController != null)
            {
                fpsController.MouseLook.Init(go.transform, camera.transform);
            }

            charController.enabled = true;
        }
    }
}
