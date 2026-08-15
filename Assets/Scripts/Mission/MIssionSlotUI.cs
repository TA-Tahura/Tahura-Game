using TMPro;
using UnityEngine;

public class MissionSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private GameObject checkmark;

    public void SetupSlot(Mission mission)
    {
        gameObject.SetActive(true);

        if (mission.isCountable)
        {
            label.text = $"{mission.title} ({mission.currentCount}/{mission.targetCount})";
        }
        else
        {
            label.text = mission.title;
        }

        checkmark.SetActive(mission.state == MissionState.Completed);
    }

    public void ClearSlot()
    {
        gameObject.SetActive(false);
    }
}