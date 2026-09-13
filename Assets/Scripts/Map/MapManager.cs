using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [System.Serializable]
    public struct MapUIRecord
    {
        public string roomID; // Matches the RoomZone's roomID string
        public RectTransform mapStartTransform;
        public RectTransform mapEndTransform;
    }

    [Header("UI References")]
    [SerializeField] private GameObject mapMenu;
    [SerializeField] private RectTransform playerPin;

    [Header("Map UI Layout")]
    [SerializeField] private List<MapUIRecord> mapUIRecords;

    private Dictionary<string, MapUIRecord> uiLookup = new Dictionary<string, MapUIRecord>();

    private InputAction mapAction;
    private bool isOpen = false;
    private RoomZone currentRoom;
    private Transform playerTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        mapAction = InputSystem.actions.FindAction("Map");

        // Build the fast lookup dictionary for our UI records
        foreach (var record in mapUIRecords)
        {
            if (!uiLookup.ContainsKey(record.roomID))
            {
                uiLookup.Add(record.roomID, record);
            }
        }
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        if (mapMenu != null)
        {
            mapMenu.SetActive(false);
        }
    }

    private void OnEnable() => mapAction.performed += OnMapMenuAction;
    private void OnDisable() => mapAction.performed -= OnMapMenuAction;

    private void OnMapMenuAction(InputAction.CallbackContext context)
    {
        if (!isOpen && PlayerState.IsAnyUIOpen) return;
        isOpen = !isOpen;
        if (isOpen) OpenMenu(); else CloseMenu();
    }

    private void DebugMapUIRecords()
    {
        Debug.Log($"=== Map UI Records ({mapUIRecords.Count}) ===");

        for (int i = 0; i < mapUIRecords.Count; i++)
        {
            MapUIRecord record = mapUIRecords[i];

            Debug.Log(
                $"[{i}] RoomID: {record.roomID}, " +
                $"Start: {(record.mapStartTransform != null ? record.mapStartTransform.name : "NULL")}, " +
                $"End: {(record.mapEndTransform != null ? record.mapEndTransform.name : "NULL")}"
            );
        }

        Debug.Log("=== End Map UI Records ===");
    }

    public void OpenMenu()
    {
        PlayerState.IsAnyUIOpen = true;
        mapMenu.SetActive(true);
        UpdatePlayerPinPosition();
        DebugMapUIRecords();
    }

    public void CloseMenu()
    {
        mapMenu.SetActive(false);
        PlayerState.IsAnyUIOpen = false;
    }

    private void Update()
    {
        if (isOpen)
        {
            UpdatePlayerPinPosition();
        }
    }

    public void SetActiveRoom(RoomZone newRoom)
    {
        currentRoom = newRoom;
    }

    private void UpdatePlayerPinPosition()
    {
        if (playerTransform == null || playerPin == null) return;

        // If there's no current room or the Room ID isn't in our UI lookup, hide the pin if it's currently active
        if (currentRoom == null || !uiLookup.TryGetValue(currentRoom.RoomID, out MapUIRecord uiRecord))
        {
            if (playerPin.gameObject.activeSelf)
            {
                playerPin.gameObject.SetActive(false);
            }
            return;
        }

        if (uiRecord.mapStartTransform == null || uiRecord.mapEndTransform == null) return;

        // Make sure the pin is active only if it isn't already
        if (!playerPin.gameObject.activeSelf)
        {
            playerPin.gameObject.SetActive(true);
        }

        float minX = currentRoom.PlayerMinX;
        float maxX = currentRoom.PlayerMaxX;

        if (Mathf.Approximately(minX, maxX)) return;

        float progress = Mathf.InverseLerp(minX, maxX, playerTransform.position.x);

        playerPin.anchoredPosition = Vector2.Lerp(
            uiRecord.mapStartTransform.anchoredPosition,
            uiRecord.mapEndTransform.anchoredPosition,
            progress
        );
    }
}