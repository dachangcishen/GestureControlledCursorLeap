using System;
using UnityEngine;

public static class EventManager
{
    public static event Action<Trial> OnTrialBegin;
    public static event Action<int> OnTrialEnd;

    public static void BeginTrial(Trial trial)
    {
        OnTrialBegin?.Invoke(trial);
    }

    public static void EndTrial(int interceptedResult)
    {
        OnTrialEnd?.Invoke(interceptedResult);
    }
}


