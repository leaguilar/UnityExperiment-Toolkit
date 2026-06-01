using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

namespace Assets.Scripts
{
    public class PracticeScenePage : SetupPage
    {
        public float minWaitTime = 0.5f;

        [Header("Controller Setup")]
        public MonoBehaviour FPSController;
        public MouseTracker mouseTracker;
        public ControlTest firstTest;

        private float startTime;

        protected new void OnEnable()
        {
            base.OnEnable();
            startTime = Time.time;
            
            // 自动查找玩家控制器 (优先匹配新版 PlayerMovement)
            if (FPSController == null)
            {
                FPSController = (MonoBehaviour)FindObjectOfType<PlayerMovement>() ?? 
                                (MonoBehaviour)FindObjectOfType<FirstPersonController>();
            }

            if (mouseTracker == null && FPSController != null)
            {
                mouseTracker = FPSController.GetComponentInChildren<MouseTracker>();
            }

            SetPlayerState(false);

            // 按钮补救 (仅保留最基础的)
            if (applyButton == null)
            {
                var btnGo = GameObject.Find("Let's Go") ?? GameObject.Find("StartButton");
                if (btnGo != null) applyButton = btnGo.GetComponent<Button>();
            }

            if (applyButton != null) applyButton.onClick.AddListener(OnApplyClicked);
        }

        protected override void OnApplyPage()
        {
            SetPlayerState(true);

            if (firstTest != null)
            {
                firstTest.gameObject.SetActive(true);
            }
        }

        private void SetPlayerState(bool active)
        {
            if (FPSController != null) 
            {
                FPSController.enabled = active;
                // 同时处理 CharacterController 确保重力/移动物理生效
                var cc = FPSController.GetComponent<CharacterController>() ?? FPSController.GetComponentInParent<CharacterController>();
                if (cc != null) cc.enabled = active;
            }

            if (mouseTracker != null) mouseTracker.enabled = active;

            // 处理光标
            Cursor.lockState = active ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !active;
        }

        protected override bool CanApplyPage() => Time.time > startTime + minWaitTime;
    }
}
