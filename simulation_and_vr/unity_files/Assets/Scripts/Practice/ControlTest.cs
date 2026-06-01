using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // 必须引用 UI 命名空间

public abstract class ControlTest : SequentialVisibleElement
{
    [Header("UI Display Settings")]
    public TextMeshProUGUI instructionDisplay; // Link your TMP object here
    [TextArea]
    public string instructionText; // The message for this specific task

    public GameObject next; // The next task in the sequence

    [Header("UI Fading")]
    public float fadeDuration = 0.5f;

    [Header("Player Reset")]
    public Transform playerResetPoint; // Optional: Reset player to this point on finish
    public GameObject playerObject;    // Optional: Manual player reference

    [Header("Sequence Settings")]
    public bool showFinishFeedback = false; // If true, shows feedback and waits 2s
    public string finishMessage = "Task Finished!";
    public Color finishColor = Color.green;
    [Tooltip("Font size for the finish message. Set to 0 to use the UI's default size.")]
    public float finishFontSize = 0f; 

    private bool isFinishing = false;

    protected abstract bool TestRequirements();

    protected virtual void OnTestFinished() { }

    protected virtual void OnEnable()
    {
        base.OnEnable();
        isFinishing = false;

        // Auto-find player if not assigned
        if (playerObject == null)
        {
            var movement = FindObjectOfType<PlayerMovement>();
            if (movement != null) playerObject = movement.gameObject;
        }

        // 1. 记录任务已激活
        Debug.Log($"[{this.gameObject.name}] OnEnable 被调用. 准备更新 UI.");

        // Automatically show the instruction when the task starts
        if (instructionDisplay != null)
        {
            instructionDisplay.text = instructionText;

            // Start with alpha 0 and fade in
            instructionDisplay.canvasRenderer.SetAlpha(0);
            instructionDisplay.CrossFadeAlpha(1, fadeDuration, false);

            // 2. 记录 UI 赋值成功以及赋了什么值
            Debug.Log($"[{this.gameObject.name}] UI 更新成功，当前文字: {instructionText}");
        }
        else
        {
            // 3. 如果找不到 UI 组件，抛出醒目的红色错误
            Debug.LogError($"[{this.gameObject.name}] 的 instructionDisplay 引用为空！请检查 Inspector！");
        }
    }

    protected void Update()
    {
        if (!isFinishing && TestRequirements())
        {
            StartCoroutine(FinishRoutine());
        }
    }

    private IEnumerator FinishRoutine()
    {
        isFinishing = true;

        if (showFinishFeedback)
        {
            // 1. Show "Task Finished!" message with custom color and size
            if (instructionDisplay != null)
            {
                string hexColor = ColorUtility.ToHtmlStringRGB(finishColor);
                string styledText = $"<color=#{hexColor}>{finishMessage}</color>";
                
                if (finishFontSize > 0)
                {
                    styledText = $"<size={finishFontSize}>{styledText}</size>";
                }

                instructionDisplay.text = styledText;
                instructionDisplay.canvasRenderer.SetAlpha(1);
            }

            // 2. Wait for 2 seconds
            yield return new WaitForSeconds(2.0f);

            // 3. Fade out the text before teleporting
            if (instructionDisplay != null)
            {
                instructionDisplay.CrossFadeAlpha(0, 0.5f, false);
                yield return new WaitForSeconds(0.5f);
                instructionDisplay.text = "";
            }
        }
        else
        {
            // Original immediate fade out for "small" tasks
            if (instructionDisplay != null)
            {
                instructionDisplay.CrossFadeAlpha(0, fadeDuration, false);
                yield return new WaitForSeconds(fadeDuration);
                instructionDisplay.text = "";
            }
        }

        // 4. Perform player reset if requested
        if (playerObject != null && playerResetPoint != null)
        {
            TeleportPlayer(playerObject, playerResetPoint);
        }

        // 5. Perform any final logic
        OnTestFinished();

        // 6. Activate the next task
        if (next != null)
        {
            next.SetActive(true);
        }

        this.gameObject.SetActive(false);
    }

    protected void TeleportPlayer(GameObject player, Transform destination)
    {
        GameObject rootPlayer = player.transform.root.gameObject;
        
        var cc = rootPlayer.GetComponentInChildren<CharacterController>();
        var movement = rootPlayer.GetComponentInChildren<PlayerMovement>();

        if (cc != null) cc.enabled = false;
        if (movement != null) movement.ResetVelocity();

        rootPlayer.transform.position = destination.position;
        rootPlayer.transform.rotation = destination.rotation;

        Physics.SyncTransforms();

        if (cc != null) cc.enabled = true;
    }

    protected override SequentialVisibleElement GetFollowup()
    {
        if (next == null) return null;
        return next.GetComponent<SequentialVisibleElement>();
    }
}