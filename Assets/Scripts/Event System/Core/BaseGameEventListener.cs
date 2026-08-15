using UnityEngine;
using UnityEngine.Events;

public abstract class BaseGameEventListener<T, E> : MonoBehaviour, IBaseEventListener<T>
    where E : BaseGameEvent<T>
{
    [SerializeField] private E gameEvent;
    
    [SerializeField] private UnityEvent<T> response; 

    private void OnEnable() { if (gameEvent != null) gameEvent.RegisterListener(this); }
    private void OnDisable() { if (gameEvent != null) gameEvent.UnregisterListener(this); }

    public void OnEventRaised(T value) => response?.Invoke(value);
}