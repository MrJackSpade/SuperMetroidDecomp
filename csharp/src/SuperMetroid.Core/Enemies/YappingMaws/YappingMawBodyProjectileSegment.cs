namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's body-link animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="YappingMawBodyProjectileAnimations"/>.
/// </summary>
internal enum YappingMawBodyProjectileSegment
{
    /// <summary>A link of its neck facing down, held.</summary>
    FacingDown,
    /// <summary>A link of its neck facing up, held.</summary>
    FacingUp,
}
