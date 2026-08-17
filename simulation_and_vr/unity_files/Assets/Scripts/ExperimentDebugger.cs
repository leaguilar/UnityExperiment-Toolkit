using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts;

public class ExperimentDebugger : MonoBehaviour
{
    public TrialOverview trialOverview;
    public PlayerMovement fpsController;
    public MouseTracker mouseTracker;
    public Button startButton;

    private bool hasLogged;

    void Update()
    {
        if (hasLogged) return;
        hasLogged = true;

        Debug.Log("========== Experiment State Check ==========");

        var es = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        Debug.Log($"[1] EventSystem: {(es != null ? "OK" : "MISSING!")}");

        if (trialOverview != null)
        {
            Debug.Log($"[2] TrialOverview GO active: {trialOverview.gameObject.activeSelf}");
            Debug.Log($"[3] TrialOverview component enabled: {trialOverview.enabled}");
            Debug.Log($"[4] FPSController ref: {(trialOverview.FPSController != null ? "OK" : "NULL!")}");
            Debug.Log($"[5] mouseTracker ref: {(trialOverview.mouseTracker != null ? "OK" : "NULL!")}");
            Debug.Log($"[6] applyButton ref: {(trialOverview.applyButton != null ? "OK" : "NULL!")}");
            Debug.Log($"[7] Recorder ref: {(trialOverview.Recorder != null ? "OK" : "NULL!")}");
        }
        else
        {
            Debug.LogError("[2] TrialOverview is NULL!");
        }

        if (fpsController != null)
        {
            Debug.Log($"[8] FPSController enabled: {fpsController.enabled}");
            Debug.Log($"[9] FPSController isLocalPlayer: {fpsController.isLocalPlayer}");
        }
        else
        {
            Debug.Log("[8] FPSController is NULL! (can't walk?)");
        }

        if (mouseTracker != null)
        {
            Debug.Log($"[10] MouseTracker enabled: {mouseTracker.enabled}");
            Debug.Log($"[11] MouseTracker isLocalPlayer: {mouseTracker.isLocalPlayer}");
        }
        else
        {
            Debug.Log("[10] MouseTracker is NULL! (can't look?)");
        }

        Debug.Log($"[12] Cursor.lockState: {Cursor.lockState}");
        Debug.Log($"[13] Cursor.visible: {Cursor.visible}");

        if (startButton != null)
        {
            Debug.Log($"[14] StartButton interactable: {startButton.interactable}");
            Debug.Log($"[15] StartButton GO active: {startButton.gameObject.activeSelf}");
            Debug.Log($"[16] StartButton GO activeInHierarchy: {startButton.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.Log("[14] StartButton is NULL!");
        }

        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"[17] Canvas renderMode: {canvas.renderMode}");
            Debug.Log($"[18] Canvas sortingOrder: {canvas.sortingOrder}");
            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            Debug.Log($"[19] GraphicRaycaster enabled: {(raycaster != null && raycaster.enabled ? "OK" : "MISSING!")}");
        }
        else
        {
            Debug.Log("[17] Canvas is NULL!");
        }

        var networkedPlayer = fpsController != null ? fpsController.GetComponent<UbiqNetworkedPlayer>() : null;
        if (networkedPlayer != null)
        {
            Debug.Log($"[20] UbiqNetworkedPlayer isLocalPlayer: {networkedPlayer.isLocalPlayer}");
        }
        else
        {
            Debug.Log("[20] UbiqNetworkedPlayer is NULL!");
        }

        Debug.Log("=============================================");
    }
}
