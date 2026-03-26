using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    /// <summary>
    /// Stores data for a single task
    /// </summary>
    public class TrialData
    {
        private readonly List<TrackingEntry> trackingData;

        public readonly IReadOnlyList<TrackingEntry> TrackingData;

        public int TargetId;

        public string TargetMaterialName;

        public float StartTime;

        public float EndTime;

        public event Action<TrackingEntry> TrackingDataAdded;

        public TrialData()
        {
            trackingData = new List<TrackingEntry>();
            TrackingData = trackingData.AsReadOnly();
            TargetId = -1;
        }

        public void AddTrackingData(TrackingEntry entry)
        {
            trackingData.Add(entry);
            OnTrackingDataAdded(entry);
        }

        protected virtual void OnTrackingDataAdded(TrackingEntry obj)
        {
            TrackingDataAdded?.Invoke(obj);
        }
    }
}
