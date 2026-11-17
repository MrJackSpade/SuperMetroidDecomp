namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain's fake death's descent (#2208), from her first phase's end to her tubes' collapse. Its values are arbitrary.</summary>
public enum MotherBrainDescentPhase
{
    /// <summary>Starts the wait before the lock and immediately runs its first update.</summary>
    InitialPause,

    /// <summary>Waits out the pause, then locks Samus and closes the scroll to the room's second screen.</summary>
    PauseBeforeLock,

    /// <summary>Waits out the pause, queues music stop/data load, and starts the wait before Samus is unlocked.</summary>
    PauseBeforeMusic,

    /// <summary>Releases Samus input after the music pause and begins the wait before the screen flashes.</summary>
    PauseBeforeUnlock,

    /// <summary>Starts room flashing, selects lowered-acid FX, clears the ceiling block, and arms tube collapse.</summary>
    PauseBeforeFlash,

    /// <summary>Advances the fake-death grey fade each time its step's wait runs out while also running tube collapse and explosions.</summary>
    FadeToGray,

    /// <summary>Runs the independent tube sequence and dust/explosion cadence until the falling main tube starts ascent.</summary>
    CollapseTubes,
}
