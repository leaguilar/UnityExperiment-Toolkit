using UnityEngine;

[ExecuteInEditMode]
public class UIForceCenter : MonoBehaviour
{
    void Start()
    {
        ForceCenter();
    }

    // 在编辑器里修改时也会实时归位
    void OnValidate()
    {
        ForceCenter();
    }

    public void ForceCenter()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            // 1. 设置锚点到正中心
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            // 2. 坐标归零
            rect.anchoredPosition = Vector2.zero;
            rect.localPosition = Vector3.zero;

            // 3. 确保缩放是 1
            rect.localScale = Vector3.one;

            // 4. 强制设置一个可见的大小（如果之前是0的话）
            if (rect.sizeDelta.x < 1) rect.sizeDelta = new Vector2(10, 10);
        }
    }
}
