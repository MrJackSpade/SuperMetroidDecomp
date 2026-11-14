namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Puyo's grounded loops and airborne hop poses as typed animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="PuyoAnimations"/>.
/// </summary>
internal enum PuyoSegment
{
    /// <summary>The grounded fast animation.</summary>
    GroundedFast,
    /// <summary>The grounded medium animation.</summary>
    GroundedMedium,
    /// <summary>The grounded slow animation.</summary>
    GroundedSlow,
    /// <summary>The airborne pose tilted furthest right: early in a rise to the right, late in a fall to the left.</summary>
    TiltedFarRight,
    /// <summary>The airborne pose tilted right: midway through a rise to the right or a fall to the left.</summary>
    TiltedRight,
    /// <summary>The upright airborne pose at the top of a hop.</summary>
    Upright,
    /// <summary>The airborne pose tilted left: midway through a rise to the left or a fall to the right.</summary>
    TiltedLeft,
    /// <summary>The airborne pose tilted furthest left: early in a rise to the left, late in a fall to the right.</summary>
    TiltedFarLeft,
}
