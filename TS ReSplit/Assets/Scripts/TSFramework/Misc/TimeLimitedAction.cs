using System;
using UnityEngine;

// an action that will only get run once a delay has passed since the last time it was called
public class TimeLimitedAction
{
    public float LastRunTime = -1f;
    public float DelayTime   = 1f;
    public Action Action     = null;

    public TimeLimitedAction(float DelayTime, Action Action)
    {
        this.DelayTime = DelayTime;
        this.Action    = Action;
    }

    public void Run()
    {
        if (Time.time > (LastRunTime + DelayTime))
        {
            LastRunTime = Time.time;

            if (Action != null)
            {
                Action();
            }
        }
    }
}
