using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameEvent<T> : ScriptableObject
{
    private readonly List<IBaseEventListener<T>> listeners = new List<IBaseEventListener<T>>();

    public void Raise(T value)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
            listeners[i].OnEventRaised(value);
    }

    public void RegisterListener(IBaseEventListener<T> listener) => listeners.Add(listener);
    public void UnregisterListener(IBaseEventListener<T> listener) => listeners.Remove(listener);
}

public interface IBaseEventListener<T>
{
    void OnEventRaised(T value);
}

public interface IBaseEventListener
{
    void OnEventRaised();
}