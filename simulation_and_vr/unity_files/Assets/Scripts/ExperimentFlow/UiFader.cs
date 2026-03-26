using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[ExecuteInEditMode]
public class UiFader : MonoBehaviour
{
    public float FadeDuration = 1f;

    public bool FadeColor = true;

    public Color StartColor;

    public Color TargetColor = Color.black;

    public bool FadeAlpha = true;

    public float StartAlpha;

    public float TargetAlpha = 0f;

    [SerializeField]
    [HideInInspector]
    private bool initialized;

    private void OnEnable()
    {
        var image = this.gameObject.GetComponent<Image>();

        if (!initialized)
        {
            StartColor = image.color;
            StartAlpha = image.color.a;
            initialized = true;
        }

        if (!Application.isPlaying)
        {
            return;
        }

        if (FadeColor)
        {
            image.color = new Color(StartColor.r, StartColor.g, StartColor.b, image.color.a);
            image.CrossFadeColor(TargetColor, FadeDuration, true, false);
        }

        if (FadeAlpha)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, StartAlpha);
            image.CrossFadeAlpha(TargetAlpha, FadeDuration, true);
        }
    }
}
