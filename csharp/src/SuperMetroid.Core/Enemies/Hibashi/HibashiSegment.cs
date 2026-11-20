namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The Hibashi fire pillar's erupting flame and its collision part as typed animations (#641).
/// Their steps are <see cref="HibashiAnimations"/>.
/// </summary>
internal enum HibashiSegment
{
    /// <summary>The visible flame's eruption.</summary>
    Flame,
    /// <summary>The pillar's collision part, held as an ember.</summary>
    Hitbox,
}
