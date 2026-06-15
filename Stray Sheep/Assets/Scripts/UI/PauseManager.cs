using UnityEngine;

public static class PauseManager
{
    public static bool IsPaused { get; private set; }

    public static void SetPaused(bool paused)
    {
        IsPaused = paused;
    }
}
