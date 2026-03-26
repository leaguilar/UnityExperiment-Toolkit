using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class SetupPage : SequentialVisibleElement
{
    public GameObject nextPage;

    public Button applyButton;

    protected abstract void OnApplyPage();

    protected abstract bool CanApplyPage();

    protected new void OnEnable()
    {
        base.OnEnable();

        if (applyButton != null)
        {
            applyButton.onClick.AddListener(OnApplyClicked);
        }
    }

    protected void OnDisable()
    {
        if (applyButton != null)
        {
            applyButton.onClick.RemoveListener(OnApplyClicked);
        }
    }

    protected override SequentialVisibleElement GetFollowup()
    {
        if (nextPage == null)
        {
            return null;
        }

        return nextPage.GetComponent<SequentialVisibleElement>();
    }

    protected void OnApplyClicked()
    {
        if (!CanApplyPage())
        {
            return;
        }

        if (nextPage != null)
        {
            nextPage.SetActive(true);
        }
        
        OnApplyPage();

        this.gameObject.SetActive(false);
    }
}
