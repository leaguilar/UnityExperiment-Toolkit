using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;
[RequireComponent(typeof(Collider), typeof(MeshRenderer))]
public class Target : MonoBehaviour
{
    public int Number;

    public string Description;

    private Collider collider;

    private MeshRenderer meshRenderer;

    private void Start()
    {
        this.collider = GetComponent<Collider>();
        this.meshRenderer = GetComponent<MeshRenderer>();
        this.meshRenderer.enabled = false;
        Database.NextTrialStarted += OnNextTrialStarted;
    }

    private void OnDestroy()
    {
        Database.NextTrialStarted -= OnNextTrialStarted;
    }

    private void OnNextTrialStarted(int targetNumber)
    {
        var isTarget = targetNumber == Number;
        collider.enabled = isTarget;
        meshRenderer.enabled = isTarget;
    }

    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log($"[Target] 检测到碰撞：物体 '{collider.gameObject.name}' 进入了目标 #{Number} 的范围。");

        if (Database.CurrentTrial.TargetId != Number)
        {
            Debug.LogWarning($"[Target] 目标编号不匹配！当前需要找的是 #{Database.CurrentTrial.TargetId}，而你撞到的是 #{Number}。");
            return;
        }

        var go = collider.gameObject;
        // 兼容性检查：检查是否有 PlayerMovement 或 CharacterController
        if (go.GetComponent<PlayerMovement>() != null || go.GetComponent<CharacterController>() != null)
        {
            Debug.Log("[Target] 确认玩家到达！正在触发任务结束...");
            var syncManager = FindObjectOfType<Assets.Scripts.TrialSyncManager>();
            if (syncManager != null)
            {
                syncManager.BroadcastTrialEnd();
            }
            else
            {
                // Single-player fallback: original behaviour.
                Database.EndTrial();
                var trialOverview = Resources.FindObjectsOfTypeAll<TrialOverview>().FirstOrDefault();
                if (trialOverview != null) trialOverview.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning($"[Target] 碰撞物体 '{go.name}' 没有 PlayerMovement 或 CharacterController 组件，忽略触发。");
        }
    }
}
