using UnityEngine;

public class RoomZone : MonoBehaviour
{
    [Header("Camera Horizontal Bounds")]
    [SerializeField] private float cameraMinX;
    [SerializeField] private float cameraMaxX;

    [Header("Player Horizontal Bounds")]
    [SerializeField] private float playerMinX;
    [SerializeField] private float playerMaxX;

    [Header("Visual Editor Settings")]
    [SerializeField] private Color cameraLineColor = Color.green;
    [SerializeField] private Color playerLineColor = Color.cyan;
    [SerializeField] private float lineVisualHeight = 10f; // Visual height for the lines in editor

    private CameraFollow mainCamera;
    private PlayerController2D playerController2D;
    private float camWidth;

    void Start()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float camHeight = cam.orthographicSize;
        camWidth = camHeight * cam.aspect;
        mainCamera = cam.GetComponent<CameraFollow>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerController2D = playerObj.GetComponent<PlayerController2D>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (mainCamera != null)
            {
                mainCamera.minX = cameraMinX + camWidth;
                mainCamera.maxX = cameraMaxX - camWidth;
            }

            if (playerController2D != null)
            {
                playerController2D.minX = playerMinX + 1;
                playerController2D.maxX = playerMaxX - 1;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        float currentY = transform.position.y;

        // Draw Camera Bounds (Green Walls)
        Gizmos.color = cameraLineColor;
        DrawBoundaryLine(cameraMinX, currentY);
        DrawBoundaryLine(cameraMaxX, currentY);

        // Draw Player Bounds (Cyan/Blue Walls)
        Gizmos.color = playerLineColor;
        DrawBoundaryLine(playerMinX, currentY);
        DrawBoundaryLine(playerMaxX, currentY);
    }

    private void DrawBoundaryLine(float xPos, float centerY)
    {
        Vector3 top = new Vector3(xPos, centerY + (lineVisualHeight / 2f), 0f);
        Vector3 bottom = new Vector3(xPos, centerY - (lineVisualHeight / 2f), 0f);
        Gizmos.DrawLine(top, bottom);
    }
}