namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The behaviour Dachora's main AI runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum DachoraAiFunction
{
    /// <summary>Probes the floor and waits for Samus to enter the activation rectangle.</summary>
    WaitingForSamus = 1,

    /// <summary>Runs the 120-update warning countdown before starting the first run animation.</summary>
    BlinkingBeforeRun,

    /// <summary>Accelerates left along the floor until the turn coordinate or a wall.</summary>
    RunningLeft,

    /// <summary>Accelerates right until the tutorial launch coordinate or a wall.</summary>
    RunningRight,

    /// <summary>Cycles the shine glow and counts down to the vertical launch.</summary>
    ChargingShinespark,

    /// <summary>Accelerates upward, publishes echo positions, and transitions on ceiling collision.</summary>
    Shinesparking,

    /// <summary>Applies capped gravity until floor collision resumes the running phase.</summary>
    Falling,

    /// <summary>Alternates one physical echo slot's visibility for its eight-update lifetime.</summary>
    Echo,
}
