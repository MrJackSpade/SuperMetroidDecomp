namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's body-link programs (#641), one per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="YappingMawBodyProjectileAnimations"/>.
/// </summary>
internal enum YappingMawBodyProjectileProgram
{
    /// <summary>The facing down animation.</summary>
    FacingDown,
    /// <summary>The facing up animation.</summary>
    FacingUp,
}
