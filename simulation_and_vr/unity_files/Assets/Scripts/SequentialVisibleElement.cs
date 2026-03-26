using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class SequentialVisibleElement : MonoBehaviour
{
    protected abstract SequentialVisibleElement GetFollowup();

    protected virtual void OnEnable()
    {
        DisableFollowup();
    }

    protected void DisableFollowup()
    {
        var followup = GetFollowup();
        if (followup != null)
        {
            followup.DisableFollowup();
            followup.gameObject.SetActive(false);
        }
    }
}

