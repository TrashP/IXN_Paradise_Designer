using UnityEngine;

public class WaterBoxCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform waterTank;
    public Camera cam;

    void Start()
    {
        // Check necessary components
        if (waterTank == null)
        {
            Debug.LogError("Water Tank Transform is not set! Please set Water Tank in Inspector.");
            return;
        }

        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("Main camera not found! Please set Camera in Inspector.");
                return;
            }
        }

        // Get the boundary of the water tank
        Renderer waterRenderer = waterTank.GetComponent<Renderer>();
        if (waterRenderer == null)
        {
            Debug.LogError("Water tank object does not have Renderer component!");
            return;
        }

        var bounds = waterRenderer.bounds;
        Vector3 size = bounds.size;
        Vector3 center = bounds.center;

        // Set the camera to orthographic projection
        cam.orthographic = true;

        // Ensure the water tank fills the screen horizontally
        float aspect = (float)Screen.width / Screen.height;
        cam.orthographicSize = size.y / 2f;

        // If you want to fill horizontally instead of vertically
        // cam.orthographicSize = size.x / (2f * aspect);

        // Put the camera in front of the water tank
        cam.transform.position = center + new Vector3(0, 0, -size.z / 2 - 0.5f);
        cam.transform.rotation = Quaternion.Euler(0, 0, 0);

        Debug.Log($"Camera has been set. Position: {cam.transform.position}, Orthographic Size: {cam.orthographicSize}");
    }

    /// <summary>
    /// Manually reset camera position and size
    /// </summary>
    [ContextMenu("Reset Camera")]
    public void ResetCamera()
    {
        Start();
    }

    /// <summary>
    /// Draw the camera settings in the Scene view
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (waterTank != null && cam != null)
        {
            Renderer waterRenderer = waterTank.GetComponent<Renderer>();
            if (waterRenderer != null)
            {
                // Draw the water tank boundary
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(waterRenderer.bounds.center, waterRenderer.bounds.size);

                // Draw the camera position
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(cam.transform.position, 0.5f);
                Gizmos.DrawLine(cam.transform.position, waterRenderer.bounds.center);
            }
        }
    }
}
