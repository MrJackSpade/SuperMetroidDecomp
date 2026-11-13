namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The Hibashi fire pillar's eruption graphics and its collision part as typed animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="HibashiAnimations"/>.
/// </summary>
internal enum HibashiSegment
{
    /// <summary>The graphics animation.</summary>
    Graphics,
    /// <summary>The hitbox animation.</summary>
    Hitbox,
}
