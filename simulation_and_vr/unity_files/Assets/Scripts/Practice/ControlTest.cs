using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ControlTest : SequentialVisibleElement
{
    public GameObject next;

    protected abstract bool TestRequirements();

    protected virtual void OnTestFinished()
    {

    }
    
    protected void Update()
    {
        if (TestRequirements())
        {
            if (next != null)
            {
                next.SetActive(true);
            }

            this.gameObject.SetActive(false);

            OnTestFinished();
        }
    }

    protected override SequentialVisibleElement GetFollowup()
    {
        if (next == null)
        {
            return null;
        }

        return next.GetComponent<SequentialVisibleElement>();
    }
}
