using UnityEngine;

public class MissionCollectible : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandleCollected();
        }
    }

    void HandleCollected()
    {
        MissionManager.Instance.AddProgress("get_mining_tool");
        Destroy(gameObject);
    }
}
