using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [SerializeField] private List<Mission> missions = new List<Mission>();
    private Dictionary<string, Mission> missionLookup = new Dictionary<string, Mission>();

    [SerializeField] private MissionEvent OnMissionDiscovered;
    [SerializeField] private MissionEvent OnMissionCompleted;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Initialize lookup dictionary for fast access
        foreach (var mission in missions)
        {
            if (!missionLookup.ContainsKey(mission.id))
                missionLookup.Add(mission.id, mission);
        }
    }

    public void DiscoverMission(string missionId)
    {
        if (missionLookup.TryGetValue(missionId, out Mission mission))
        {
            if (mission.state == MissionState.Locked)
            {
                mission.state = MissionState.Discovered;
                OnMissionDiscovered?.Raise(mission);
                Debug.Log($"[MissionManager] Discovered: {mission.title}");
            }
        }
    }

    public void AddProgress(string missionId, int amount = 1)
    {
        if (missionLookup.TryGetValue(missionId, out Mission mission))
        {
            if (mission.state == MissionState.Discovered && mission.isCountable)
            {
                mission.AddProgress(amount);
                if (mission.state == MissionState.Completed)
                {
                    OnMissionCompleted?.Raise(mission);
                }
            }
        }
    }

    public void CompleteMission(string missionId)
    {
        if (missionLookup.TryGetValue(missionId, out Mission mission))
        {
            if (mission.state == MissionState.Discovered)
            {
                mission.Complete();
                OnMissionCompleted?.Raise(mission);
            }
        }
    }

    public Mission GetMission(string missionId)
    {
        missionLookup.TryGetValue(missionId, out Mission m);
        return m;
    }

    public List<Mission> GetDiscoveredMissions()
    {
        List<Mission> res = new List<Mission>();
        foreach (var mission in missions)
        {
            if (mission.state != MissionState.Locked)
            {
                res.Add(mission);
            }
        }
        return res;
    }
}