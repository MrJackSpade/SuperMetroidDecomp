namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// The boss body the room shows (#2194), as the frame-end display latch reads it: the layer, where it lies and whether it
/// shows nothing. The room's boss attaches it as it sets up, and the room keeps it for its lifetime, so the layer keeps
/// drawing after the boss's own place is gone.
/// </summary>
internal interface IBossBodyDisplay
{
    /// <summary>
    /// The body as the room shows it now (#2560), its layer, placement and whether this frame hides it, or null while the
    /// room shows none of it.
    /// </summary>
    BossBodyShowing? Showing { get; }

    /// <summary>Whether the boss hides the room's background layer this frame (#2571).</summary>
    bool HidesBackgroundLayer { get; }
}
