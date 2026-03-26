using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TestKeyboardMovement : ControlTest
{
    public KeyCode key;

    public float minPressTime = 3;

    private float totalPressTime;

    protected override bool TestRequirements()
    {
        if (Input.GetKey(key))
        {
            totalPressTime += Time.deltaTime;
        }

        return totalPressTime > minPressTime;
    }

    /// <inheritdoc />
    protected override void OnTestFinished()
    {
        Database.SendMetaData("Practice", $"Finished key \"{this.key}\" practice.");
    }
}
