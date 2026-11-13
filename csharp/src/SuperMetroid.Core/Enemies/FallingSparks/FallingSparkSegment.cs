namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>
/// The falling spark's animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="FallingSparkAnimations"/>.
/// </summary>
internal enum FallingSparkSegment
{
    /// <summary>The falling animation.</summary>
    Falling,
    /// <summary>The hit floor animation.</summary>
    HitFloor,
}
