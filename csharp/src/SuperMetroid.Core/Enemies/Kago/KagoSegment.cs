namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// Kago's slow and post-hit animation loops (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="KagoAnimations"/>.
/// </summary>
internal enum KagoSegment
{
    /// <summary>The hive at rest, swelling and swollen, then easing back, slowly, repeating.</summary>
    Slow,
    /// <summary>The hive at rest, swelling and swollen, then easing back, quickly after a hit, repeating.</summary>
    Fast,
}
