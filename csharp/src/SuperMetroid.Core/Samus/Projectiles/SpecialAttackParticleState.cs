namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What a special attack particle keeps (#2346): the part it plays, fixed when the attack starts (#1683), and its own
/// attack's flight: an Ice particle's (<see cref="IceParticleState"/>), a Plasma ring's
/// (<see cref="PlasmaParticleState"/>), a Spazer particle's (<see cref="SpazerParticleState"/>) or a Wave particle's
/// (<see cref="WaveParticleState"/>).
/// </summary>
/// <param name="part">The part the particle plays in its attack.</param>
internal abstract class SpecialAttackParticleState(SpecialAttackParticle part) : SamusShotKindState
{
    /// <summary>The part the particle plays in its attack.</summary>
    internal SpecialAttackParticle Part { get; } = Ensure.NotNull(part);

    /// <summary>
    /// A particle leaves no count a beam fired over it could start from (<see cref="BeamFiredOverLastShot"/>): an attack's
    /// particles hold only the first four places (<see cref="SpecialAttackReplacesFirstShots"/>), and a beam fired with
    /// every place held takes the last.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always: a beam is being fired over a particle.</exception>
    internal override int FuseLeftForBeamFiredOver => throw new InvalidOperationException(
        "A beam fired with every place held takes the last place, which no special attack particle holds, yet a particle was replaced.");
}
