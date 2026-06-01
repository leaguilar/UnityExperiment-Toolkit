using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindGoalTest : ControlTest
{
    [Header("Goal Setup")]
    public ProximityTrigger Trigger; // The trigger on the sphere
    public GameObject Hint;          // The sphere itself
    public Transform Spawnpoint;     // Where the sphere should appear

    protected override void OnEnable()
    {
        // 1. Reset trigger state to prevent "instant completion"
        if (Trigger != null)
        {
            Trigger.triggered = false;
        }

        // 2. Call base to show the UI text
        base.OnEnable();

        // 3. Move the sphere to the spawn point
        if (Hint != null && Spawnpoint != null)
        {
            Hint.SetActive(false); // Briefly disable to reset physics
            Hint.transform.position = Spawnpoint.position;
            Hint.transform.rotation = Spawnpoint.rotation;
            Hint.SetActive(true);
        }
    }

    protected override bool TestRequirements()
    {
        if (Trigger == null) return false;
        return Trigger.triggered; // Task finishes when this becomes true
    }

    protected override void OnTestFinished()
    {
        if (Hint != null) Hint.SetActive(false);
        Debug.Log("Goal found successfully!");
    }
}