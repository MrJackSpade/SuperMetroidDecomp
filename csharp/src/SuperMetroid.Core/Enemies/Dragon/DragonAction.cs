namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// The behaviour a Dragon body's main AI runs each frame; its cosmetic wing has none.
/// Its values are arbitrary.
/// </summary>
public enum DragonAction
{
    /// <summary>Waits below the lava, then selects facing and begins the rise (<see cref="DragonTimingDefinitions.RiseOrSinkDuration"/>).</summary>
    WaitToRise,

    /// <summary>Moves the body and cosmetic wing upward one pixel per update.</summary>
    Rising,

    /// <summary>Waits for each attack animation and emits the three-fireball volley.</summary>
    Attacking,

    /// <summary>Holds the exposed idle pose (<see cref="DragonTimingDefinitions.WaitBeforeSink"/>) before sinking.</summary>
    WaitToSink,

    /// <summary>Moves the body and wing downward before restoring the hidden wait.</summary>
    Sinking,
}
