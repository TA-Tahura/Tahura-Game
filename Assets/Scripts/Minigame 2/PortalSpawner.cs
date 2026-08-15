using UnityEngine;

public class PortalSpawner : MonoBehaviour
{
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(-5f, 3f, 0f);
    private Transform playerTransform;

    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    public void OnAllItemCollected()
    {
        if (playerTransform == null) return;
        Vector3 spawnPosition = playerTransform.position + playerTransform.TransformDirection(spawnOffset);
        Instantiate(portalPrefab, spawnPosition, Quaternion.identity);
        MissionManager.Instance.DiscoverMission("find_portal");
    }
}