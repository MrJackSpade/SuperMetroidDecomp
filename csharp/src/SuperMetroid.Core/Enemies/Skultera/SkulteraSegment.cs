namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// Skultera's swimming and turning animations (#641).
/// Their steps are <see cref="SkulteraAnimations"/>.
/// </summary>
internal enum SkulteraSegment
{
    /// <summary>The swim to the left, looped.</summary>
    SwimmingLeft,
    /// <summary>
    /// Turning around toward the right, half a turn in steps that slow at either end, before it swims on.
    /// </summary>
    TurningRight,
    /// <summary>The swim to the right, looped.</summary>
    SwimmingRight,
    /// <summary>
    /// Turning around toward the left, half a turn in steps that slow at either end, before it swims on.
    /// </summary>
    TurningLeft,
}
