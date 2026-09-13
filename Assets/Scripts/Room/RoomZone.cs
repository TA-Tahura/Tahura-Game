using UnityEngine;

public class RoomZone : MonoBehaviour
{
    [SerializeField] private string roomID; // e.g., "Room_A", "Boss_Room"
    [Header("Camera Horizontal Bounds")]
    [SerializeField] private float cameraMinX;
    [SerializeField] private float cameraMaxX;

    [Header("Wall Horizontal Bounds")]
    [SerializeField] private float playerMinX;
    [SerializeField] private float playerMaxX;

    [Header("Visual Editor Settings")]
    [SerializeField] private Color cameraLineColor = Color.green;
    [SerializeField] private Color playerLineColor = Color.cyan;
    [SerializeField] private float lineVisualHeight = 10f; // Visual height for the lines in editor
    public string RoomID => roomID;
    public float PlayerMinX => playerMinX;
    public float PlayerMaxX => playerMaxX;
    private CameraFollow mainCamera;
    private PlayerController2D playerController2D;
    private BoxCollider2D roomCollider;
    private float camWidth;

    void Start()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            float camHeight = cam.orthographicSize;
            camWidth = camHeight * cam.aspect;
            mainCamera = cam.GetComponent<CameraFollow>();
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerController2D = playerObj.GetComponent<PlayerController2D>();
        }

        roomCollider = GetComponent<BoxCollider2D>();

        CheckIfPlayerAlreadyInside();
    }

    private void CheckIfPlayerAlreadyInside()
    {
        if (playerController2D == null) return;

        BoxCollider2D roomCollider = GetComponent<BoxCollider2D>();
        if (roomCollider == null) return;

        Vector3 playerPosition = playerController2D.transform.position;
        Bounds bounds = roomCollider.bounds;

        if (bounds.Contains(playerPosition))
        {
            ApplyRoomBounds();
        }
    }
    
    private void ApplyRoomBounds()
    {
        Debug.Log("set current room " + roomID);
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

        MapManager.Instance?.SetActiveRoom(this);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyRoomBounds();
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