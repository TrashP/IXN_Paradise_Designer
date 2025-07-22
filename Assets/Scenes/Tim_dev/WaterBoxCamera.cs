using UnityEngine;

public class WaterBoxCamera : MonoBehaviour
{
    [Header("摄像机设置")]
    public Transform waterTank;
    public Camera cam;

    void Start()
    {
        // 检查必要组件
        if (waterTank == null)
        {
            Debug.LogError("未设置水缸Transform！请在Inspector中设置Water Tank。");
            return;
        }

        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("未找到主摄像机！请在Inspector中设置Camera。");
                return;
            }
        }

        // 获取水缸的边界
        Renderer waterRenderer = waterTank.GetComponent<Renderer>();
        if (waterRenderer == null)
        {
            Debug.LogError("水缸对象没有Renderer组件！");
            return;
        }

        var bounds = waterRenderer.bounds;
        Vector3 size = bounds.size;
        Vector3 center = bounds.center;

        // 设置摄像机为正交投影
        cam.orthographic = true;

        // 保证水缸横向刚好填满画面
        float aspect = (float)Screen.width / Screen.height;
        cam.orthographicSize = size.y / 2f;

        // 如果你想"横向"填满而不是"纵向"
        // cam.orthographicSize = size.x / (2f * aspect);

        // 将摄像机放到水缸前方
        cam.transform.position = center + new Vector3(0, 0, -size.z / 2 - 0.5f);
        cam.transform.rotation = Quaternion.Euler(0, 0, 0);

        Debug.Log($"摄像机已设置完成。位置: {cam.transform.position}, 正交大小: {cam.orthographicSize}");
    }

    /// <summary>
    /// 手动重新设置摄像机位置和大小
    /// </summary>
    [ContextMenu("重新设置摄像机")]
    public void ResetCamera()
    {
        Start();
    }

    /// <summary>
    /// 在Scene视图中绘制摄像机设置的可视化
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (waterTank != null && cam != null)
        {
            Renderer waterRenderer = waterTank.GetComponent<Renderer>();
            if (waterRenderer != null)
            {
                // 绘制水缸边界
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(waterRenderer.bounds.center, waterRenderer.bounds.size);

                // 绘制摄像机位置
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(cam.transform.position, 0.5f);
                Gizmos.DrawLine(cam.transform.position, waterRenderer.bounds.center);
            }
        }
    }
}
