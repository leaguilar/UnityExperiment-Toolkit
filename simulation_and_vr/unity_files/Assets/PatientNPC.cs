using UnityEngine;
using UnityEngine.AI;
using TMPro; // Added for TextMeshPro support

public class PatientNPC : MonoBehaviour
{
    [Header("Patient Info")]
    public string patientBed = "Bed 05";
    public GameObject infoPanel; // 拖入挂在病人身上的 Canvas

    [Header("Movement")]
    public NavMeshAgent agent;
    public Transform bedDestination; // 拖入床位旁的空物体

    [Header("Interaction & Prompts")]
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public TextMeshProUGUI promptText; // 拖入场景中通用的 "Press E" 文本框

    private bool hasStartedMoving = false;
    private bool isPlayerNear = false;

    void Update()
    {
        if (Camera.main == null) 
        {
            // Debug.LogError($"[{gameObject.name}] Camera.main is null! Ensure your player camera has the 'MainCamera' Tag.");
            return; // 频繁报错太吵，这里静默返回
        }

        // 检查任务管理器状态
        if (RegisterTest.Instance == null)
        {
            return; // 任务还没开始
        }

        if (!RegisterTest.Instance.gameObject.activeInHierarchy)
        {
            return; // 当前还没轮到这个任务
        }

        // 检查玩家距离
        float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
        bool wasNear = isPlayerNear;
        isPlayerNear = dist <= interactDistance;

        // 处理提示文字的显示/隐藏
        if (promptText != null)
        {
            if (isPlayerNear && !wasNear)
            {
                promptText.text = "Press [E] to interact with Patient";
                promptText.gameObject.SetActive(true);
                Debug.Log($"[{gameObject.name}] Player is near. Prompt SHOWN. Distance: {dist}");
            }
            else if (!isPlayerNear && wasNear)
            {
                promptText.gameObject.SetActive(false);
                Debug.Log($"[{gameObject.name}] Player left area. Prompt HIDDEN.");
            }
        }

        // 处理按键交互
        if (isPlayerNear && Input.GetKeyDown(interactKey))
        {
            Debug.Log($"[{gameObject.name}] 'E' key pressed. Current Task State: {RegisterTest.Instance.currentState}");

            if (!hasStartedMoving)
            {
                // 阶段 1：第一次互动
                if (RegisterTest.Instance.currentState != RegisterTest.State.FindPatient) 
                {
                    Debug.Log($"[{gameObject.name}] Ignored interaction because task state is not FindPatient.");
                    return;
                }

                if (promptText != null) promptText.gameObject.SetActive(false);
                if (infoPanel != null) infoPanel.SetActive(true);
                
                if (agent != null && bedDestination != null)
                {
                    agent.SetDestination(bedDestination.position);
                    Debug.Log($"[{gameObject.name}] Starting movement to {bedDestination.name}");
                }
                else
                {
                    Debug.LogWarning($"[{gameObject.name}] NavMeshAgent or BedDestination missing!");
                }
                
                hasStartedMoving = true;
                RegisterTest.Instance.OnPatientInfoViewed();
            }
            else
            {
                // 阶段 3：在床边第二次互动
                if (RegisterTest.Instance.currentState != RegisterTest.State.FindBed) 
                {
                    Debug.Log($"[{gameObject.name}] Ignored interaction because task state is not FindBed.");
                    return;
                }

                if (bedDestination != null)
                {
                    float distToBed = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), 
                                                       new Vector2(bedDestination.position.x, bedDestination.position.z));
                    
                    if (distToBed < 2.5f) 
                    {
                        Debug.Log($"[{gameObject.name}] Final interaction near bed successful. Finishing task.");
                        if (promptText != null) promptText.gameObject.SetActive(false);
                        RegisterTest.Instance.OnFinalConfirmation();
                    }
                    else
                    {
                        Debug.Log($"[{gameObject.name}] Tried to confirm, but NPC is not at the bed yet. Distance to bed: {distToBed}");
                    }
                }
            }
        }
    }

    public void HideInfo()
    {
        if (infoPanel != null) infoPanel.SetActive(false);
    }

    void OnDisable()
    {
        // 确保脚本禁用时，提示文字也消失
        if (promptText != null) promptText.gameObject.SetActive(false);
    }
}