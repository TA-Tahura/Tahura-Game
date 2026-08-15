using System;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public enum MissionState
{
    Locked,
    Discovered,
    Completed
}

[System.Serializable]
public class Mission
{
    public string id;
    public string title;
    
    [Header("State")]
    public MissionState state = MissionState.Locked;
    
    [Header("Countable Target")]
    public bool isCountable;
    public int targetCount = 1;
    public int currentCount;

    public GameEvent onCompleted;

    public void AddProgress(int amount = 1)
    {
        if (state != MissionState.Discovered) return;

        currentCount += amount;
        if (currentCount >= targetCount)
        {
            currentCount = targetCount;
            Complete();
        }
    }

    public void Complete()
    {
        if (state == MissionState.Completed) return;

        state = MissionState.Completed;
        onCompleted?.Raise();
    }
}