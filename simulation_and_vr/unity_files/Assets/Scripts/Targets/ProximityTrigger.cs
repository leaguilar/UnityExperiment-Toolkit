using UnityEngine;
using UnityEngine.Events;

public class ProximityTrigger : MonoBehaviour
{
    [Header("Detection Settings")]
    public GameObject TargetObject; // Drag your FPSController here
    public bool triggered = false;  // FindGoalTest reads this

    [Header("Events (Optional)")]
    public UnityEvent TriggerEnter;
    public UnityEvent TriggerExit;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the thing entering is the Player (by reference, tag, or name)
        if (other.gameObject == TargetObject || 
            other.CompareTag("Player") || 
            other.name.Contains("FPS") || 
            other.name.Contains("Character"))
        {
            triggered = true; // Set to TRUE so the task finishes
            TriggerEnter?.Invoke();
            Debug.Log("Player reached the goal!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == TargetObject || 
            other.CompareTag("Player") || 
            other.name.Contains("FPS") || 
            other.name.Contains("Character"))
        {
            triggered = false;
            TriggerExit?.Invoke();
            Debug.Log("Player left the goal area.");
        }
    }
} 