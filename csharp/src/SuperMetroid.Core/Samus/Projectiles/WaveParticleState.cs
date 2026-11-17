namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Wave special attack particle's homing on Samus (#2346): its horizontal speed is the shot's velocity, its vertical
/// speed its own.
/// </summary>
/// <param name="part">The part the particle plays in its attack.</param>
internal sealed class WaveParticleState(SpecialAttackParticle part) : SpecialAttackParticleState(part)
{
    /// <summary>
    /// The updates the particle has left: <see cref="BeamComboDefinitions.WaveLifetime"/> when the attack starts, counted
    /// down each update. The particle ends when it reaches zero, so it never passes it.
    /// </summary>
    internal int Lifetime { get; set => field = Ensure.AtLeastZero(value); } = BeamComboDefinitions.WaveLifetime;

    /// <summary>The particle's vertical homing speed.</summary>
    internal SignedEightEight VerticalSpeed { get; set; }
}
