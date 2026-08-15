using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour, IBaseEventListener
{
    [SerializeField] private GameEvent gameEvent;
    [SerializeField] private UnityEvent response;

    private void OnEnable() { if (gameEvent != null) gameEvent.RegisterListener(this); }
    private void OnDisable() { if (gameEvent != null) gameEvent.UnregisterListener(this); }

    public void OnEventRaised() => response?.Invoke();
}