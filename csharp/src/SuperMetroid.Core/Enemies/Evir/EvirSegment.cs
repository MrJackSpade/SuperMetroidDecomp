namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The Evir body's and arms' facing loops and its projectile's spit and regeneration animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="EvirAnimations"/>.
/// </summary>
internal enum EvirSegment
{
    /// <summary>The body facing left animation.</summary>
    BodyFacingLeft,
    /// <summary>The arms facing left animation.</summary>
    ArmsFacingLeft,
    /// <summary>The body facing right animation.</summary>
    BodyFacingRight,
    /// <summary>The arms facing right animation.</summary>
    ArmsFacingRight,
    /// <summary>The projectile normal animation.</summary>
    ProjectileNormal,
    /// <summary>The projectile regenerating animation.</summary>
    ProjectileRegenerating,
    /// <summary>The projectile regeneration loop animation.</summary>
    ProjectileRegenerationLoop,
}
