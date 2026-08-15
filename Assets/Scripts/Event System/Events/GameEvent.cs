using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameEvent", menuName = "Events/Game Event")]
public class GameEvent : ScriptableObject
{
    private readonly List<IBaseEventListener> listeners = new List<IBaseEventListener>();

    public void Raise()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
            listeners[i].OnEventRaised();
    }

    public void RegisterListener(IBaseEventListener listener) => listeners.Add(listener);
    public void UnregisterListener(IBaseEventListener listener) => listeners.Remove(listener);
}