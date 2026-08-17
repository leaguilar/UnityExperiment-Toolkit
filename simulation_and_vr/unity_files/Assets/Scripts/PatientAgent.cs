using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class PatientAgent : MonoBehaviour
{
    [Header("Patient Info")]
    public string patientName = "John Doe";
    public int patientAge = 45;
    public string patientCondition = "Chest Pain";
    public string patientBed = "Bed 05";

    [Header("UI")]
    public GameObject infoPanel;
    public TextMeshProUGUI infoNameText;
    public TextMeshProUGUI infoAgeText;
    public TextMeshProUGUI infoConditionText;
    public TextMeshProUGUI infoBedText;

    [Header("Movement")]
    public NavMeshAgent agent;
    public Transform destination;

    [Header("Interaction")]
    public float interactDistance = 3f;
    public TextMeshProUGUI promptText;

    public bool HasReachedDestination { get; private set; }
    public bool HasStartedMoving { get; private set; }

    private bool playerNear;

    void Start()
    {
        if (infoPanel != null) infoPanel.SetActive(false);
        if (agent != null) agent.enabled = false;
        RefreshInfoPanel();
    }

    void Update()
    {
        if (Camera.main == null) return;

        float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
        bool wasNear = playerNear;
        playerNear = dist <= interactDistance;

        if (promptText != null)
        {
            if (playerNear && !wasNear)
                promptText.gameObject.SetActive(true);
            else if (!playerNear && wasNear)
                promptText.gameObject.SetActive(false);
        }

        if (HasReachedDestination) return;

        if (HasStartedMoving && agent != null && agent.enabled && !agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance + 0.5f)
            {
                HasReachedDestination = true;
                agent.enabled = false;
                Debug.Log($"[PatientAgent] {patientName} reached {destination?.name}");
            }
        }
    }

    public void Interact()
    {
        Debug.Log($"[PatientAgent] Interact() called. HasStartedMoving={HasStartedMoving} agent={agent?.name ?? "NULL"} dest={destination?.name ?? "NULL"}");
        if (!HasStartedMoving)
        {
            ShowInfo();
            if (agent != null && destination != null)
            {
                agent.enabled = true;
                agent.SetDestination(destination.position);
                Debug.Log($"[PatientAgent] {patientName} moving to {destination.name} at {destination.position}");
            }
            else
            {
                Debug.LogWarning($"[PatientAgent] Cannot move: agent={agent != null} destination={destination != null}");
            }
            HasStartedMoving = true;
        }
    }

    public void ShowInfo()
    {
        if (infoPanel != null) infoPanel.SetActive(true);
        RefreshInfoPanel();
    }

    public void HideInfo()
    {
        if (infoPanel != null) infoPanel.SetActive(false);
    }

    private void RefreshInfoPanel()
    {
        if (infoNameText != null) infoNameText.text = patientName;
        if (infoAgeText != null) infoAgeText.text = $"Age: {patientAge}";
        if (infoConditionText != null) infoConditionText.text = $"Condition: {patientCondition}";
        if (infoBedText != null) infoBedText.text = $"Bed: {patientBed}";
    }
}
