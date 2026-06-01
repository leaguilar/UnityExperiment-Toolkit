using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class TrialOverview : SetupPage
    {
        public int Repetitions = 3;
        
        public TMP_Text HeaderText;
        public TMP_Text DescriptionText;
        public Image DescriptionImage;
        public TMP_Text HintText;
        public Image HintImage;

        [Header("Controller & Sync")]
        public PlayerMovement FPSController;
        public MouseTracker mouseTracker;
        public GameObject interactionDot;
        public ParticipantRecorder Recorder;
        public Transform Spawnpoint;
        public TrialSyncManager SyncManager;

        public string NextSceneName;

        private Vector3 manualStartPosition;
        private Quaternion manualStartRotation;
        private bool hasSavedManualPos = false;

        private List<Target> tasks;
        
        private List<Material> materials;

        private int totalTasks;

        private Target currentTarget;

        private Material currentMaterial;

        private void Awake()
        {
            // In single-player mode seed by participant id so each participant gets
            // their own shuffled order. In multiplayer mode seed by the shared
            // ExperimentId so every client generates the identical trial sequence.
            var seed = SyncManager != null
                ? (Database.ExperimentId ?? string.Empty).GetHashCode()
                : Database.ParticipantId?.GetHashCode() ?? 0;
            Random.InitState(seed / 2 + SceneManager.GetActiveScene().name.GetHashCode() / 2);
            
            var allTargets = FindObjectsOfType<Target>();
            Debug.Log($"[TrialOverview] 场景体检：共发现 {allTargets.Length} 个目标球。");
            foreach(var t in allTargets) Debug.Log($" - 发现目标 #{t.Number}: {t.Description}");

            tasks = new List<Target>(allTargets.Length * Repetitions);
            for (var i = 0; i < Repetitions; i++)
            {
                RandomizeOrder(allTargets);
                tasks.AddRange(allTargets);
            }

            this.totalTasks = this.tasks.Count;
            
            var allMaterials = Resources.LoadAll<Material>("TargetMaterials/");
            materials = new List<Material>(allTargets.Length * Repetitions);

            if (allMaterials.Length == 0)
            {
                Debug.LogError("No target materials available.");
                this.enabled = false;
                Application.Quit(666);
                return;
            }

            while (materials.Count < tasks.Count)
            {
                RandomizeOrder(allMaterials);

                foreach (var material in allMaterials)
                {
                    materials.Add(material);

                    if (materials.Count >= tasks.Count)
                    {
                        break;
                    }
                }
            }
        }

        protected new void OnEnable()
        {
            // 确保文字和逻辑在面板出现时是隐藏的
            if (HintText != null) HintText.gameObject.SetActive(false);
            if (HintImage != null) HintImage.gameObject.SetActive(false);

            // 终极修复：如果场景缺了 EventSystem，自动造一个
            if (UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem_AutoCreated");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            base.OnEnable();

            // 锁定控制
            if (FPSController != null) FPSController.enabled = false;
            if (mouseTracker != null) mouseTracker.enabled = false;
            if (interactionDot != null) interactionDot.SetActive(false);

            Database.EndTrial();

            if (tasks.Count == 0)
            {
                SceneManager.LoadScene(NextSceneName, LoadSceneMode.Single);
                return;
            }

            if (Recorder != null) Recorder.StopRecording();
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            currentTarget = tasks[0];
            tasks.RemoveAt(0);

            currentMaterial = materials[0];
            materials.RemoveAt(0);

            // 修正：确保球能被看见
            if (currentTarget != null)
            {
                var renderer = currentTarget.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = currentMaterial;
                    renderer.enabled = true; // 强制开启渲染器
                }
                
                // 强制开启物体及其碰撞
                currentTarget.gameObject.SetActive(true);
                var collider = currentTarget.GetComponent<Collider>();
                if (collider != null) collider.enabled = true;
            }

            HeaderText.text = $"Task Goal #{Database.TrialResults.Count + 1} of {this.totalTasks}";
            DescriptionText.text = $"Find the {currentTarget.Description}\nGo to the {currentMaterial.name.ToLower()} ball.";
            
            if (DescriptionImage != null)
            {
                DescriptionImage.color = currentMaterial.color;
            }
            else
            {
                Debug.LogWarning("[TrialOverview] 缺少 Description Image 引用，跳过颜色预览更新。");
            }
            
            Debug.Log("[TrialOverview] 面板已激活，控制已锁定，等待点击开始...");
        }

        protected override void OnApplyPage()
        {
            if (SyncManager != null)
            {
                // Multiplayer: broadcast to all peers. The actual trial setup
                // happens in OnNetworkTrialStart() on every client.
                SyncManager.BroadcastTrialStart(currentTarget.Number, currentMaterial.name);
            }
            else
            {
                // Single-player: apply directly.
                OnNetworkTrialStart(currentTarget.Number, currentMaterial.name);
            }
        }

        /// <summary>
        /// Called on every client (including the one that clicked Start) by
        /// TrialSyncManager once the start signal has been broadcast.
        /// Contains all per-trial setup that must happen on every participant's machine.
        /// </summary>
        public void OnNetworkTrialStart(int targetId, string materialName)
        {
            // 自动补救：如果开始后还没连上 HintText，尝试找一下
            if (HintText == null) HintText = GameObject.Find("HintText")?.GetComponent<TMP_Text>();

            string hintMsg = $"Target: {currentTarget.Description}\nColor: {currentMaterial.name}";

            if (HintText != null)
            {
                // 强制修复：如果 HintText 被藏在面板里，把它救出来
                if (HintText.transform.IsChildOf(this.transform))
                {
                    Debug.LogWarning("[TrialOverview] 检测到 HintText 被错误地放在了面板内部，正在将其移至 Canvas 根部以防止消失。");
                    HintText.transform.SetParent(this.transform.parent, true);
                }

                HintText.gameObject.SetActive(true);
                HintText.text = hintMsg;
            }
            else
            {
                Debug.LogWarning("[TrialOverview] 缺少 HUD HintText 引用。当前任务目标：" + hintMsg);
            }

            if (HintImage != null)
            {
                HintImage.gameObject.SetActive(true);
                HintImage.color = currentMaterial.color;
            }

            PlaceFPSController();
            
            if (FPSController != null) FPSController.enabled = true;
            if (mouseTracker != null) mouseTracker.enabled = true;
            if (interactionDot != null) interactionDot.SetActive(true);

            Recorder.StartRecording();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Database.StartNewTrial(targetId, materialName);
        }

        protected override bool CanApplyPage()
        {
            return true;
        }

        private void PlaceFPSController()
        {
            if (FPSController == null) return;

            var position = Spawnpoint != null 
                ? Spawnpoint.transform.position + Vector3.up * 0.9f 
                : transform.position; // 这里的 transform 指的是面板的位置，作为兜底
            
            var rotation = Spawnpoint != null 
                ? Spawnpoint.transform.rotation 
                : Quaternion.identity;

            // 改回最原始的赋值方式
            FPSController.transform.position = position;
            FPSController.transform.rotation = rotation;
            Debug.Log($"[TrialOverview] 玩家已复位到: {position}");
        }

        private void RandomizeOrder<T>(IList<T> items)
        {
            var cnt = items.Count;
            for (var i = 0; i < cnt; i++)
            {
                var newIndex = Random.Range(0, cnt);
                var tmp = items[i];
                items[i] = items[newIndex];
                items[newIndex] = tmp;
            }
        }
    }
}
