namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>
/// The falling spark's animations (#641).
/// Their steps are <see cref="FallingSparkAnimations"/>.
/// </summary>
internal enum FallingSparkSegment
{
    /// <summary>A spark tumbling, upright, diagonal and small, as it falls, repeating.</summary>
    Falling,
    /// <summary>
    /// The spark flickering on and off through its shapes where it hit the floor, before it vanishes.
    /// </summary>
    HitFloor,
}
