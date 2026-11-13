namespace SuperMetroid.Core.Enemies.FallingSparks;

/// <summary>
/// The falling spark's programs (#641), one per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="FallingSparkAnimations"/>.
/// </summary>
internal enum FallingSparkProgram
{
    /// <summary>The falling animation.</summary>
    Falling,
    /// <summary>The hit floor animation.</summary>
    HitFloor,
}
