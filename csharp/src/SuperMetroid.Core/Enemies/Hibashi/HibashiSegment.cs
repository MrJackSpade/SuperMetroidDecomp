namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The Hibashi fire pillar's erupting flame and its collision part as typed animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="HibashiAnimations"/>.
/// </summary>
internal enum HibashiSegment
{
    /// <summary>The visible flame's eruption.</summary>
    Flame,
    /// <summary>The hitbox animation.</summary>
    Hitbox,
}
