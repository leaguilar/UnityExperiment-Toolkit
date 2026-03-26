using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace Assets.Scripts
{
    public class PracticeScenePage : SetupPage
    {
        public float minWaitTime = 0.5f;

        public FirstPersonController FPSController;

        private float startTime;

        protected override void OnApplyPage()
        {
            FPSController.enabled = true;
            Database.SendMetaData("Practice", "Started practice.");
        }

        protected override bool CanApplyPage()
        {
            // To prevent accidental double click to skip control page, add a short wait time
            return Time.time > startTime + minWaitTime;
        }

        protected new void OnEnable()
        {
            base.OnEnable();
            startTime = Time.time;
            FPSController.enabled = false;
        }
    }
}
