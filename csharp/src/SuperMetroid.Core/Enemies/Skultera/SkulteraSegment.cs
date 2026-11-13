namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// Skultera's swimming and turning animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="SkulteraAnimations"/>.
/// </summary>
internal enum SkulteraSegment
{
    /// <summary>The swimming left animation.</summary>
    SwimmingLeft,
    /// <summary>The loop.</summary>
    SwimmingLeftLoop,
    /// <summary>The turning right animation.</summary>
    TurningRight,
    /// <summary>The swimming right animation.</summary>
    SwimmingRight,
    /// <summary>The loop.</summary>
    SwimmingRightLoop,
    /// <summary>The turning left animation.</summary>
    TurningLeft,
}
