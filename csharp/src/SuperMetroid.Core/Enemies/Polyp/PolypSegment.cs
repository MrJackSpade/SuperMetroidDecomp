namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Polyp's stationary pose as a typed animation (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="PolypAnimations"/>.
/// </summary>
internal enum PolypSegment
{
    /// <summary>The Polyp's rock, held where it lies in the lava.</summary>
    Stationary,
}
