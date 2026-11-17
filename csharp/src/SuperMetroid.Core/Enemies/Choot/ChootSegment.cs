namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// Choot's idle, jumping and falling animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="ChootAnimations"/>.
/// </summary>
internal enum ChootSegment
{
    /// <summary>Lying flat, held; it stops moving while off screen.</summary>
    Idle,
    /// <summary>Springing up, then held folded while it jumps; it moves off screen too.</summary>
    Jumping,
    /// <summary>Springing, then held spread out while it falls; it moves off screen too.</summary>
    Falling,
}
