namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What a special attack particle keeps (#2346): the part it plays, fixed when the attack starts (#1683), and its own
/// attack's flight: an Ice particle's (<see cref="IceParticleState"/>), a Plasma ring's
/// (<see cref="PlasmaParticleState"/>), a Spazer particle's (<see cref="SpazerParticleState"/>) or a Wave particle's
/// (<see cref="WaveParticleState"/>).
/// </summary>
/// <param name="part">The part the particle plays in its attack.</param>
internal abstract partial class SpecialAttackParticleState(SpecialAttackParticle part) : SamusShotKindState
{
    /// <summary>The part the particle plays in its attack.</summary>
    internal SpecialAttackParticle Part { get; } = Ensure.NotNull(part);

    /// <summary>A special attack particle.</summary>
    internal override BeamFiredOverLastShot.ReplacedShot ReplacedByBeam => new BeamFiredOverLastShot.ReplacedShot.SpecialAttackParticle();
}
