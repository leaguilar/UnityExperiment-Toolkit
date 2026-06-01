using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ReticleClicker : MonoBehaviour
{
    [Tooltip("点击按键，默认是鼠标左键/VR手柄触发键")]
    public KeyCode clickKey = KeyCode.Mouse0;

    void Update()
    {
        // 只有当鼠标处于锁定状态时，才启用准星点击逻辑
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (Input.GetKeyDown(clickKey))
            {
                PerformReticleClick();
            }
        }
    }

    void PerformReticleClick()
    {
        // 1. 创建一个模拟的点击事件，位置设在屏幕正中心
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // 2. 射线检测 UI 元素
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // 3. 遍历检测到的物体，寻找第一个可以点击的按钮
        foreach (RaycastResult result in results)
        {
            // 尝试从物体本身或其父级获取 Button 组件
            Button btn = result.gameObject.GetComponentInParent<Button>();
            if (btn != null && btn.interactable)
            {
                // 模拟点击
                btn.onClick.Invoke();
                Debug.Log("[ReticleClicker] 已通过准星点击按钮: " + btn.gameObject.name);
                break; // 每次只点击最上层的一个按钮
            }
        }
    }
}
