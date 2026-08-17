using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

namespace Assets.Scripts
{
    public class WaitForButton : SetupPage
    {
        public float minWaitTime = 0.5f;
        public string NextSceneName;

        [Header("Optional Control Locking")]
        public MonoBehaviour FPSController;
        public MonoBehaviour mouseTracker;
        public GameObject interactionDot;

        private float startTime;

        protected new void OnEnable()
        {
            base.OnEnable();
            startTime = Time.time;

            // 自动查找玩家控制器
            if (FPSController == null)
            {
                FPSController = (MonoBehaviour)FindObjectOfType<PlayerMovement>() ?? 
                                (MonoBehaviour)FindObjectOfType<FirstPersonController>();
            }
            if (mouseTracker == null && FPSController != null)
            {
                mouseTracker = (MonoBehaviour)FPSController.GetComponentInChildren<MouseTracker>();
            }

            SetPlayerState(false);
        }

        protected override void OnApplyPage()
        {
            if (!string.IsNullOrWhiteSpace(NextSceneName))
            {
                SceneManager.LoadScene(NextSceneName, LoadSceneMode.Single);
            }

            SetPlayerState(true);
        }

        private void SetPlayerState(bool active)
        {
            if (FPSController != null) FPSController.enabled = active;
            if (mouseTracker != null) mouseTracker.enabled = active;
            if (interactionDot != null) interactionDot.SetActive(active);

            bool shouldLock = active && (FPSController != null || mouseTracker != null);
            Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !shouldLock;
        }

        protected override bool CanApplyPage() => Time.time > startTime + minWaitTime;
    }
}
