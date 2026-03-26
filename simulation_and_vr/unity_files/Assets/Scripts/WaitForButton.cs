using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class WaitForButton : SetupPage
    {
        public float minWaitTime = 0.5f;

        public string NextSceneName;

        private float startTime;

        protected override void OnApplyPage()
        {
            if (!string.IsNullOrWhiteSpace(NextSceneName))
            {
                SceneManager.LoadScene(NextSceneName, LoadSceneMode.Single);
            }
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

            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
}
