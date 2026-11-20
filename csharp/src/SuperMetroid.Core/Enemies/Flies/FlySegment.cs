namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>
/// The shared Mellow, Mella and Memu flight loop as a typed animation (#641): the fly's one loop, its wings beating as it
/// flies. Its steps are <see cref="FlyAnimations"/>.
/// </summary>
internal enum FlySegment
{
    /// <summary>Its wings beating, raised out, raised, level and lowered, repeating.</summary>
    Flight,
}
