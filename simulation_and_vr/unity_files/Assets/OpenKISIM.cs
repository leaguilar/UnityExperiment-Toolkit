using UnityEngine;
using TMPro;
using System.Collections;
using VoltstroStudios.UnityWebBrowser;

public class OpenKISIM : MonoBehaviour
{
    [Header("Website Settings")]
    public string url = "http://ec2-3-70-245-125.eu-central-1.compute.amazonaws.com/";

    [Header("Interaction Settings")]
    public float interactionDistance = 3.0f;
    public float viewAngle = 0.7f; // ~45 degrees. Higher is stricter.
    public KeyCode interactionKey = KeyCode.E;

    [Header("UI Reference")]
    public TextMeshProUGUI promptText; 
    public string promptMessage = "Press [E] to interact with Computer";

    [Header("Internal Browser Reference")]
    public WebBrowserUIBasic internalBrowser;

    private bool _isBrowserOpen = false;

    private void OnEnable()
    {
        // Force reset states whenever a task enables this script
        _isBrowserOpen = false;
        if (promptText != null) promptText.gameObject.SetActive(false);
        Debug.Log($"[OpenKISIM] {gameObject.name} Reset on Enable.");
    }

    void Update()
    {
        if (Camera.main == null) return;
        
        // If browser is currently open, we don't process interaction logic
        if (_isBrowserOpen) return;

        // 1. Calculate Eligibility
        Vector3 playerPos = Camera.main.transform.position;
        Vector3 dirToComputer = (transform.position - playerPos).normalized;
        float dist = Vector3.Distance(transform.position, playerPos);
        float dot = Vector3.Dot(Camera.main.transform.forward, dirToComputer);
        
        bool isEligible = (dist < interactionDistance) && (dot > viewAngle);

        // 2. Toggle Prompt (Direct Check)
        if (promptText != null)
        {
            if (isEligible && !promptText.gameObject.activeSelf)
            {
                promptText.text = promptMessage;
                promptText.gameObject.SetActive(true);
            }
            else if (!isEligible && promptText.gameObject.activeSelf)
            {
                promptText.gameObject.SetActive(false);
            }
        }

        // 3. Handle Key Press
        if (isEligible && Input.GetKeyDown(interactionKey))
        {
            Debug.Log($"[OpenKISIM] Interaction Key ({interactionKey}) pressed while eligible.");
            OpenSystem();
        }
        else if (Input.GetKeyDown(interactionKey))
        {
            // This will tell us if E is pressed but the distance/angle check failed
            // Debug.Log($"[OpenKISIM] Key pressed but NOT eligible. Dist: {Vector3.Distance(transform.position, Camera.main.transform.position)}, Angle OK: {Vector3.Dot(Camera.main.transform.forward, (transform.position - Camera.main.transform.position).normalized) > viewAngle}");
        }
    }

    private void OpenSystem()
    {
        _isBrowserOpen = true; // Lock the script from re-triggering
        if (promptText != null) promptText.gameObject.SetActive(false);

        if (internalBrowser == null)
        {
            Debug.LogWarning("[OpenKISIM] No internal browser assigned! Opening in external browser.");
            Application.OpenURL(url);
            NotifyTasks();
            return;
        }

        // Show browser via CanvasGroup
        CanvasGroup cg = internalBrowser.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        else
        {
            internalBrowser.gameObject.SetActive(true);
        }

        // Handle URL loading and readiness
        StopCoroutine("WaitAndNavigate");
        StartCoroutine(WaitAndNavigate(url));

        // Unlock cursor for mouse interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator WaitAndNavigate(string targetUrl)
    {
        // Wait for UWB engine to be ready if it's still starting
        while (internalBrowser != null && !internalBrowser.browserClient.ReadySignalReceived)
        {
            yield return new WaitForSeconds(0.2f);
        }
        
        if (internalBrowser != null)
        {
            internalBrowser.NavigateUrl(targetUrl);
            Debug.Log($"[OpenKISIM] Browser ready. Navigating to: {targetUrl}");
        }
    }

    public void CloseBrowser()
    {
        _isBrowserOpen = false; // Allow interaction logic to resume

        if (internalBrowser != null)
        {
            CanvasGroup cg = internalBrowser.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
            // We do NOT set internalBrowser.gameObject.SetActive(false) 
            // to keep the background process alive and avoid port conflicts.
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        NotifyTasks();
    }

    private void NotifyTasks()
    {
        Debug.Log("[OpenKISIM] Notifying tasks...");
        
        // Notify ClickComputerTest (using modern robust search)
        ClickComputerTest[] clickTests = Object.FindObjectsByType<ClickComputerTest>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var t in clickTests)
        {
            Debug.Log($"[OpenKISIM] Completing task on: {t.gameObject.name}");
            t.CompleteTask();
        }

        // Notify RegisterTest
        if (RegisterTest.Instance != null && RegisterTest.Instance.gameObject.activeInHierarchy)
        {
            Debug.Log("[OpenKISIM] Notifying RegisterTest Instance.");
            RegisterTest.Instance.OnRegisteredAtComputer();
        }
    }

    private void OnDisable()
    {
        if (promptText != null) promptText.gameObject.SetActive(false);
    }
}