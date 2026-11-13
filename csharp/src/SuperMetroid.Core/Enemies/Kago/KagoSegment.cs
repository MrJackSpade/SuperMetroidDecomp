namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// Kago's slow and post-hit animation loops (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="KagoAnimations"/>.
/// </summary>
internal enum KagoSegment
{
    /// <summary>The slow animation.</summary>
    Slow,
    /// <summary>The fast animation.</summary>
    Fast,
}
