namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The phases of Mother Brain's fake death's descent (#2208), from her first phase's end to her tubes' collapse. Its values are arbitrary.</summary>
public enum MotherBrainDescentPhase
{
    /// <summary>Seeds a 64-count wait and immediately falls into its first decrement.</summary>
    InitialPause,

    /// <summary>Waits for the timer to pass zero, then locks Samus and closes the scroll to the room's second screen.</summary>
    PauseBeforeLock,

    /// <summary>Expires the 32-count pause, queues music stop/data load, and starts the 12-count unlock wait.</summary>
    PauseBeforeMusic,

    /// <summary>Releases Samus input after the music pause and begins the eight-count screen-flash wait.</summary>
    PauseBeforeUnlock,

    /// <summary>Starts room flashing, selects lowered-acid FX, clears the ceiling block, and arms tube collapse.</summary>
    PauseBeforeFlash,

    /// <summary>Advances the fake-death grey fade whenever the timer runs out while also running tube collapse and explosions.</summary>
    FadeToGray,

    /// <summary>Runs the independent tube sequence and dust/explosion cadence until the falling main tube starts ascent.</summary>
    CollapseTubes,
}
