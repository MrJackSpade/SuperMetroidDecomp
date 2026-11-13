namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The behaviour Alcoon's main AI runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum AlcoonEnemyFunction
{
    /// <summary>Starts the emergence jump when Samus is strictly within 80 horizontal pixels and 32 pixels of the previously discovered landing Y.</summary>
    WaitingForSamus = 1,
    /// <summary>Integrates upward vertical motion with half-pixel acceleration until the whole velocity becomes nonnegative, then selects the falling pose.</summary>
    EmergingRising,
    /// <summary>Descends through the terrain collision mover, then faces Samus and begins walking with one walking cycle remaining.</summary>
    EmergingFalling,
    /// <summary>Follows the floor, hides after travelling 112 pixels from spawn, or requests a volley when the step counter is zero and Samus is ahead; animation commands perform horizontal movement.</summary>
    WalkingAndFiring,
    /// <summary>Performs no main-AI movement while the fire-volley animation runs, until its command restores walking.</summary>
    WaitingForFireAnimation,
    /// <summary>Integrates the retreat jump's upward motion until acceleration changes its whole velocity to nonnegative.</summary>
    HidingRising,
    /// <summary>Falls without terrain collision until reaching spawn Y, then restores the whole spawn coordinates and proximity-wait state while preserving subpositions.</summary>
    HidingFalling,
}
