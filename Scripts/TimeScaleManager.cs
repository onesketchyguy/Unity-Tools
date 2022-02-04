using UnityEngine;

public static class TimeScaleManager
{
    private static float timeScale = -1;
    private static float fixedTimeScale = -1;

    private static float timeScaleMultiplier;

    private static void Initialize()
    {
        fixedTimeScale = Time.fixedDeltaTime;
        timeScale = Time.timeScale;

        timeScaleMultiplier = 1;
    }

    public static float GetTimeScale() => timeScaleMultiplier;

    public static void SetTimeScale(float scale)
    {
        if (fixedTimeScale == -1) Initialize();

        timeScaleMultiplier = scale;

        Time.fixedDeltaTime = fixedTimeScale * timeScaleMultiplier;
        Time.timeScale = timeScale * timeScaleMultiplier;
    }
}