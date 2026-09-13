using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    public enum AreaType { Overworld_Map, Japanese_Cave_Map, Third_Map } // Add new maps here

    [System.Serializable]
    public struct MapUIRecord
    {
        public string roomID;
        public RectTransform mapStartTransform;
        public RectTransform mapEndTransform;
    }

    [System.Serializable]
    private struct AreaPanelMapping
    {
        public AreaType areaType;
        public GameObject panelGameObject;
    }

    [Header("UI References")]
    [SerializeField] private GameObject mapMenu;
    [SerializeField] private RectTransform playerPin;

    [Header("Map Panels Registry")]
    [SerializeField] private List<AreaPanelMapping> areaPanels;

    [Header("Map UI Layout (For Room-Based Maps)")]
    [SerializeField] private List<MapUIRecord> mapUIRecords;

    [Header("Fog / Overworld Controls")]
    [SerializeField] private Image fogOverlayImage; 
    [SerializeField] private Sprite[] fogSprites; 
    private int currentFogLevel = 0;

    private Dictionary<AreaType, GameObject> panelLookup = new Dictionary<AreaType, GameObject>();
    private Dictionary<string, MapUIRecord> uiLookup = new Dictionary<string, MapUIRecord>();

    private InputAction mapAction;
    private bool isOpen = false;
    private RoomZone currentRoom;
    private Transform playerTransform;

    private AreaType currentActiveAreaType = AreaType.Japanese_Cave_Map;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        mapAction = InputSystem.actions.FindAction("Map");

        // Build panel lookup
        foreach (var mapping in areaPanels)
        {
            if (!panelLookup.ContainsKey(mapping.areaType))
            {
                panelLookup.Add(mapping.areaType, mapping.panelGameObject);
            }
        }

        // Build room UI lookup
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

    public void SetCurrentAreaType(AreaType newType)
    {
        currentActiveAreaType = newType;
    }

    public void OpenMenu()
    {
        PlayerState.IsAnyUIOpen = true;
        mapMenu.SetActive(true);

        // Activate the target panel and deactivate all others
        foreach (var kvp in panelLookup)
        {
            if (kvp.Value != null)
            {
                kvp.Value.SetActive(kvp.Key == currentActiveAreaType);
            }
        }

        // Execute area-specific initialization
        if (currentActiveAreaType == AreaType.Overworld_Map)
        {
            UpdateFogState();
        }

        UpdatePlayerPinPosition();
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

    public void AdvanceFogState()
    {
        currentFogLevel++;
        currentFogLevel = Mathf.Clamp(currentFogLevel, 0, fogSprites.Length);
    }

    public void SetFogLevel(int level)
    {
        currentFogLevel = Mathf.Clamp(level, 0, fogSprites.Length);
    }

    private void UpdateFogState()
    {
        if (fogOverlayImage == null) return;

        if (currentFogLevel >= 2 || currentFogLevel >= fogSprites.Length)
        {
            fogOverlayImage.gameObject.SetActive(false);
        }
        else
        {
            fogOverlayImage.gameObject.SetActive(true);
            if (currentFogLevel < fogSprites.Length && fogSprites[currentFogLevel] != null)
            {
                fogOverlayImage.sprite = fogSprites[currentFogLevel];
            }
        }
    }

    private void UpdatePlayerPinPosition()
    {
        if (playerTransform == null || playerPin == null) return;

        if (currentRoom == null || !uiLookup.TryGetValue(currentRoom.RoomID, out MapUIRecord uiRecord))
        {
            if (playerPin.gameObject.activeSelf)
            {
                playerPin.gameObject.SetActive(false);
            }
            return;
        }

        if (uiRecord.mapStartTransform == null || uiRecord.mapEndTransform == null) return;

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