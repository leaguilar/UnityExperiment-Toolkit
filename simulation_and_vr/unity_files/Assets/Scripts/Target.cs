using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(Collider), typeof(MeshRenderer))]
public class Target : MonoBehaviour
{
    public int Number;

    public string Description;

    private Collider collider;

    private MeshRenderer meshRenderer;

    private void Start()
    {
        this.collider = GetComponent<Collider>();
        this.meshRenderer = GetComponent<MeshRenderer>();
        this.meshRenderer.enabled = false;
        Database.NextTrialStarted += OnNextTrialStarted;
    }

    private void OnDestroy()
    {
        Database.NextTrialStarted -= OnNextTrialStarted;
    }

    private void OnNextTrialStarted(int targetNumber)
    {
        var isTarget = targetNumber == Number;
        collider.enabled = isTarget;
        meshRenderer.enabled = isTarget;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (Database.CurrentTrial.TargetId != Number)
        {
            return;
        }

        var go = collider.gameObject;
        if (go.GetComponent<FirstPersonController>() != null)
        {
            Database.EndTrial();

            var trialOverview = Resources.FindObjectsOfTypeAll<TrialOverview>().First();
            trialOverview.gameObject.SetActive(true);
        }
    }
}
