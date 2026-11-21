namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Spazer special attack particle's flight (#2346): it circles Samus at
/// <see cref="BeamComboDefinitions.SpazerCirclingRadius"/>, then flies up towards a point above her and away from it,
/// then falls (<see cref="SpazerComboPhase"/>).
/// </summary>
/// <param name="part">The part the particle plays in its attack.</param>
/// <param name="turnPerUpdate">The particle's turn round Samus each update while it circles, its own way round.</param>
/// <param name="partner">The particle this one pairs with: a beam particle and the trailing particle that turn the same way.</param>
internal sealed class SpazerParticleState(SpecialAttackParticle part, TrigonometryAngle turnPerUpdate, SamusProjectile partner)
    : OrbitingParticleState(part, TrigonometryAngle.Zero, BeamComboDefinitions.SpazerCirclingRadius, turnPerUpdate)
{
    /// <summary>The particle's phase.</summary>
    internal SpazerComboPhase Phase { get; set; } = SpazerComboPhase.Circling;

    /// <summary>
    /// Whether the particle leaves trails: the trailing pair from the start, the beam pair only once it falls, when its
    /// trail countdown starts.
    /// </summary>
    internal bool LeavesTrails => Part.SpazerTrail || Phase == SpazerComboPhase.Falling;

    /// <summary>
    /// The particle this one is paired with from the attack's start (#1683, #2554): a beam particle and the trailing particle
    /// that turns the same way. A spent particle ends its partner's place with itself
    /// (<see cref="SamusShotPool.HolderOfPlaceOf"/>).
    /// </summary>
    internal SamusProjectile Partner { get; } = Ensure.NotNull(partner);
}
