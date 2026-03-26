using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class ProximityTrigger : MonoBehaviour
    {
        public event Action TriggerEnter;

        public event Action TriggerExit;

        public GameObject TargetObject;

        public bool triggered;

        private void OnTriggerEnter(Collider other)
        {
            triggered = true;

            if (other.gameObject == TargetObject)
            {
                TriggerEnter?.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            triggered = false;

            if (other.gameObject == TargetObject)
            {
                TriggerExit?.Invoke();
            }
        }
    }
}
