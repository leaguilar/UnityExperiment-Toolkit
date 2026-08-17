using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider), typeof(MeshRenderer))]
public class Target : MonoBehaviour
{
    public int Number;
    public string Description;

    [Tooltip("When true, player must press E near the target to complete. When false, walking in completes.")]
    public bool requireInteraction = false;

    [Tooltip("Text to show when E interaction is required. Leave empty for auto-generated.")]
    public string interactionHint;

    [Tooltip("Optional event fired when this target is completed.")]
    public UnityEvent onTargetCompleted;

    [Tooltip("When true, the target's mesh is NOT hidden when not the current target. Use for patient/tangible objects.")]
    public bool keepMeshVisible = false;

    private Collider col;
    private MeshRenderer meshRenderer;
    private bool playerInTrigger;

    private void Start()
    {
        col = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
        Database.NextTrialStarted += OnNextTrialStarted;
    }

    private void OnDestroy()
    {
        Database.NextTrialStarted -= OnNextTrialStarted;
    }

    private void Update()
    {
        if (requireInteraction && playerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"[Target] E pressed on Target #{Number}");
            CompleteTrial();
        }
    }

    private void OnNextTrialStarted(int targetNumber)
    {
        var isTarget = targetNumber == Number;
        col.enabled = isTarget;
        if (!keepMeshVisible && meshRenderer != null)
            meshRenderer.enabled = isTarget;
        playerInTrigger = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Target] 检测到碰撞：物体 '{other.gameObject.name}' 进入了目标 #{Number} 的范围。");

        if (Database.CurrentTrial.TargetId != Number)
        {
            Debug.LogWarning($"[Target] 目标编号不匹配！当前需要找的是 #{Database.CurrentTrial.TargetId}，而你撞到的是 #{Number}。");
            return;
        }

        var go = other.gameObject;
        if (go.GetComponent<PlayerMovement>() != null || go.GetComponent<CharacterController>() != null)
        {
            if (requireInteraction)
            {
                playerInTrigger = true;
                var hint = string.IsNullOrWhiteSpace(interactionHint)
                    ? $"Press E: {Description}"
                    : interactionHint;
                ShowInteractionPrompt(hint);
            }
            else
            {
                CompleteTrial();
            }
        }
        else
        {
            Debug.LogWarning($"[Target] 碰撞物体 '{go.name}' 没有 PlayerMovement 或 CharacterController 组件，忽略触发。");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerMovement>() != null || other.gameObject.GetComponent<CharacterController>() != null)
        {
            playerInTrigger = false;
            HideInteractionPrompt();
        }
    }

    private void CompleteTrial()
    {
        Debug.Log("[Target] 确认玩家到达！正在触发任务结束...");
        HideInteractionPrompt();
        onTargetCompleted.Invoke();
        var syncManager = FindObjectOfType<TrialSyncManager>();
        if (syncManager != null)
        {
            syncManager.BroadcastTrialEnd();
        }
        else
        {
            Database.EndTrial();
            var trialOverview = Resources.FindObjectsOfTypeAll<TrialOverview>().FirstOrDefault();
            if (trialOverview != null)
            {
                if (trialOverview.showPanelBetweenTasks)
                    trialOverview.gameObject.SetActive(true);
                else
                    trialOverview.AutoStartNextTrial();
            }
        }
    }

    private void ShowInteractionPrompt(string hint)
    {
        var hintText = FindObjectOfType<TMPro.TMP_Text>(); // fallback
        var trialOverview = Resources.FindObjectsOfTypeAll<TrialOverview>().FirstOrDefault();
        if (trialOverview != null && trialOverview.HintText != null)
        {
            trialOverview.HintText.text = hint;
            trialOverview.HintText.gameObject.SetActive(true);
        }
    }

    private void HideInteractionPrompt()
    {
        var trialOverview = Resources.FindObjectsOfTypeAll<TrialOverview>().FirstOrDefault();
        if (trialOverview != null && trialOverview.HintText != null)
        {
            trialOverview.HintText.gameObject.SetActive(false);
        }
    }
}
