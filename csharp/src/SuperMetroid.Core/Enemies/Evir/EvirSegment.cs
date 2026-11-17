namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The Evir body's and arms' facing loops and its projectile's spit and regeneration animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
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
    /// <summary>A regenerating spit's start at the mouth, its entry cue.</summary>
    ProjectileRegenerating,
    /// <summary>A regenerating spit's sound, after its regrowth count is set.</summary>
    ProjectileRegeneratingSound,
    /// <summary>One step of a regenerating spit's regrowth out from the mouth, which its regeneration repeats.</summary>
    ProjectileRegrowing,
    /// <summary>The regenerated spit, whole again.</summary>
    ProjectileRegrown,
}
