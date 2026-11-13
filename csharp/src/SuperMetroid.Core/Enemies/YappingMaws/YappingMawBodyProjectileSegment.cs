namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's body-link animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="YappingMawBodyProjectileAnimations"/>.
/// </summary>
internal enum YappingMawBodyProjectileSegment
{
    /// <summary>The facing down animation.</summary>
    FacingDown,
    /// <summary>The facing up animation.</summary>
    FacingUp,
}
