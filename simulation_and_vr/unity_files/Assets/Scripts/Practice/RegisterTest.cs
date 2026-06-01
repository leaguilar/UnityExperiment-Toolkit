using UnityEngine;
using UnityEngine.SceneManagement;

public class RegisterTest : ControlTest
{
    public static RegisterTest Instance;
    
    [Header("References")]
    public PatientNPC patient;
    public OpenKISIM computerScript; 

    [Header("Transition")]
    public string nextSceneName; // Optional: Load this scene when task finishes

    public enum State { FindPatient, RegisterAtComputer, FindBed }
    public State currentState = State.FindPatient;
    private bool isTaskComplete = false;

    void Awake()
    {
        Instance = this;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        currentState = State.FindPatient;
        isTaskComplete = false;
        
        if (computerScript != null) computerScript.enabled = false; 

        instructionText = "Step 1: Find the patient at the entrance and press E to view their info.";
        UpdateDisplay();
    }

    public void OnPatientInfoViewed()
    {
        if (currentState == State.FindPatient)
        {
            currentState = State.RegisterAtComputer;
            string bedInfo = patient != null ? patient.patientBed : "their bed";
            instructionText = $"Step 2: Patient is heading to {bedInfo}. Go to the coordinator's desk to register.";
            UpdateDisplay();
            if (computerScript != null) computerScript.enabled = true; 
        }
    }

    public void OnRegisteredAtComputer()
    {
        if (currentState == State.RegisterAtComputer)
        {
            currentState = State.FindBed;
            string bedInfo = patient != null ? patient.patientBed : "their bed";
            instructionText = $"Step 3: Registration done! Go to {bedInfo} and press E to confirm.";
            UpdateDisplay();
            if (patient != null) patient.HideInfo(); 
            if (computerScript != null) computerScript.enabled = false; 
        }
    }

    public void OnFinalConfirmation()
    {
        if (currentState == State.FindBed)
        {
            isTaskComplete = true; 
        }
    }

    protected override void OnTestFinished()
    {
        // Handle scene transition here if specified
        if (!string.IsNullOrWhiteSpace(nextSceneName) && next == null)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    protected override bool TestRequirements() => isTaskComplete;

    void UpdateDisplay() 
    { 
        if (instructionDisplay != null) instructionDisplay.text = instructionText; 
    }
}