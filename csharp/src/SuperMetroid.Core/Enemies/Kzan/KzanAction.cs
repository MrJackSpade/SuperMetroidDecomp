namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// The behaviour Kzan's top part runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum KzanAction
{
    /// <summary>Counts down the population-defined top wait before resetting fall acceleration.</summary>
    WaitingToFall = 1,
    /// <summary>Accelerates downward, carries a touching rider, and clamps to the lower target on landing.</summary>
    Falling,
    /// <summary>Waits at the lower target until the post-landing countdown expires.</summary>
    WaitingToRise,
    /// <summary>Returns upward at one-half pixel per NTSC actor update and resumes the top wait.</summary>
    Rising,
}
