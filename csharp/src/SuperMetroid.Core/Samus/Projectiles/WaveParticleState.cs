namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Wave special attack particle's homing on Samus (#2346, #2397): how long it has left; its homing velocity, on both
/// axes, is the shot's.
/// </summary>
/// <param name="part">The part the particle plays in its attack.</param>
internal sealed class WaveParticleState(SpecialAttackParticle part) : SpecialAttackParticleState(part)
{
    /// <summary>
    /// The updates the particle has left: <see cref="BeamComboDefinitions.WaveLifetime"/> when the attack starts, counted
    /// down each update it homes. The particle ends on its last.
    /// </summary>
    internal SpecialAttackLifetime Lifetime { get; } = new(BeamComboDefinitions.WaveLifetime);
}
