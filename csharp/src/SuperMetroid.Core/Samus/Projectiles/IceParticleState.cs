namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// An Ice special attack particle's flight (#2346): it orbits Samus at <see cref="BeamComboDefinitions.IceOrbitRadius"/>
/// until its lifetime ends, then spreads outward from <see cref="BeamComboDefinitions.IceSpreadStartRadius"/>.
/// </summary>
/// <param name="part">The part the particle plays in its attack.</param>
/// <param name="turnPerUpdate">The attack's turn round Samus each update, set by the way Samus faces as it starts.</param>
internal sealed class IceParticleState(SpecialAttackParticle part, TrigonometryAngle turnPerUpdate)
    : OrbitingParticleState(part, part.OrbitStart, BeamComboDefinitions.IceOrbitRadius, turnPerUpdate)
{
    /// <summary>
    /// The updates the particle orbits before it turns outward: <see cref="BeamComboDefinitions.IceOrbitLifetime"/> when
    /// the attack starts, counted down each update while it orbits. It turns outward on its last.
    /// </summary>
    internal UpdateCountdown Lifetime { get; } = UpdateCountdown.Lasting(BeamComboDefinitions.IceOrbitLifetime);
}
