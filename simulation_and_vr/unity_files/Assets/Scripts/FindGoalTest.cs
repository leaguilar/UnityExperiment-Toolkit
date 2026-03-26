using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FindGoalTest : ControlTest
{
    public ProximityTrigger Trigger;

    public GameObject Hint;

    public string nextSceneName;

    public GameObject Spawnpoint;

    protected override void OnTestFinished()
    {
        base.OnTestFinished();

        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }

        Database.SendMetaData("Practice", $"Finished practice for {this.Hint.name}.");
    }

    protected override bool TestRequirements()
    {
        return Trigger.triggered;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        Trigger.TargetObject.PlaceObject(Spawnpoint.transform.position, Spawnpoint.transform.rotation);

        Hint.SetActive(true);
    }

    private void OnDisable()
    {
        Hint.SetActive(false);
    }
}
