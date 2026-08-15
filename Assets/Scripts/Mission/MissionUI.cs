using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MissionUI : MonoBehaviour
{
    [SerializeField] private GameObject missionMenu;
    [SerializeField] private GameObject prevButton;
    [SerializeField] private GameObject nextButton;

    [SerializeField] private List<MissionSlotUI> slots;

    private InputAction missionMenuAction;
    private bool isOpen = false;
    private int page = 0;
    private const int ItemsPerPage = 3;

    private void Awake()
    {
        missionMenuAction = InputSystem.actions.FindAction("Mission");
    }

    private void OnEnable() => missionMenuAction.performed += OnMissionMenuAction;
    private void OnDisable() => missionMenuAction.performed -= OnMissionMenuAction;

    private void OnMissionMenuAction(InputAction.CallbackContext context)
    {
        if (!isOpen && PlayerState.IsAnyUIOpen) return;
        isOpen = !isOpen;
        if (isOpen) OpenMenu(); else CloseMenu();
    }

    public void OpenMenu()
    {
        PlayerState.IsAnyUIOpen = true;
        PlayerState.CanControl = false;
        missionMenu.SetActive(true);
        UpdatePage();
    }

    public void CloseMenu()
    {
        missionMenu.SetActive(false);
        PlayerState.CanControl = true;
        PlayerState.IsAnyUIOpen = false;
    }

    private void UpdatePage()
    {
        List<Mission> discovered = MissionManager.Instance.GetDiscoveredMissions();

        int startIndex = page * ItemsPerPage;

        int count = Mathf.Min(ItemsPerPage, discovered.Count - startIndex);
        List<Mission> pageMissions = (count > 0) ? discovered.GetRange(startIndex, count) : new List<Mission>();

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < pageMissions.Count)
            {
                slots[i].SetupSlot(pageMissions[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }

        if (prevButton) prevButton.SetActive(page > 0);
        if (nextButton) nextButton.SetActive(startIndex + ItemsPerPage < discovered.Count);
    }

    public void NextPage()
    {
        page++;
        UpdatePage();
    }

    public void PrevPage()
    {
        page = Mathf.Max(0, page - 1);
        UpdatePage();
    }
}