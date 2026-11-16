namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// Skultera's swimming and turning animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="SkulteraAnimations"/>.
/// </summary>
internal enum SkulteraSegment
{
    /// <summary>The swim to the left, looped.</summary>
    SwimmingLeft,
    /// <summary>The turning right animation.</summary>
    TurningRight,
    /// <summary>The swim to the right, looped.</summary>
    SwimmingRight,
    /// <summary>The turning left animation.</summary>
    TurningLeft,
}
