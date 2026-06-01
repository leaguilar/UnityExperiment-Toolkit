using UnityEngine;
using UnityEngine.UI;

public class ReticleFeedback : MonoBehaviour
{
    [Header("Settings")]
    public Image reticleImage;
    public Color normalColor = Color.white;
    public Color interactColor = Color.green;
    public float interactionDistance = 3.0f;
    public string targetTag = "Computer"; // 你可以把电脑屏幕的 Tag 设为这个

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (reticleImage == null) reticleImage = GetComponent<Image>();
        if (reticleImage != null) reticleImage.color = normalColor;
    }

    void Update()
    {
        if (mainCamera == null) return;

        // 从屏幕中心发出射线
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // 如果碰到了带有特定 Tag 的物体（比如电脑屏幕）
            if (hit.collider.CompareTag(targetTag))
            {
                reticleImage.color = interactColor;
            }
            else
            {
                reticleImage.color = normalColor;
            }
        }
        else
        {
            reticleImage.color = normalColor;
        }
    }
}
