using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClickComputerTest : ControlTest
{
    [Header("Computer Reference")]
    public OpenKISIM computerScript; // Drag the computer model with OpenKISIM here

    [Header("Transition")]
    public string nextSceneName; // Optional: Load this scene when task finishes

    protected override void OnEnable()
    {
        base.OnEnable(); // Shows: "Go to the coordinator's desk and click the screen"
    }

    private bool _isInternalComplete = false;

    protected override bool TestRequirements()
    {
        // We will manually set this to true when the website is closed
        return _isInternalComplete;
    }

    // Call this method from your OpenKISIM script once the link is clicked
    public void CompleteTask()
    {
        _isInternalComplete = true;
    }

    protected override void OnTestFinished()
    {
        // Handle scene transition here if specified
        if (!string.IsNullOrWhiteSpace(nextSceneName) && next == null)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}