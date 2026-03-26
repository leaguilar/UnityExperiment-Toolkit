using UnityTools.Core;
using UnityEngine;

namespace Assets.Scripts
{
    public class ParticipantRecorder : MonoBehaviour
    {
        public Camera Camera;
         
        public float Azimuth;

        public float Elevation;

        private float startTime;

        private bool isRecording;

        public void StartRecording()
        {
            startTime = Time.time;
            isRecording = true;
        }

        public void StopRecording()
        {
            isRecording = false;
        }

        private void Update()
        {
            if (!isRecording)
            {
                return;
            }

            var ct = Camera.transform;
            var ea = Camera.transform.rotation * Vector3.forward;

            Math3D.CartesianToSpherical(ea, out Azimuth, out Elevation, out _);
            
            Database.CurrentTrial.AddTrackingData(new TrackingEntry
            {
                Time = Time.time - startTime,
                Position = ct.position,
                ViewAzimuth = Azimuth,
                ViewElevation = Elevation
            });
        }
    }
}
