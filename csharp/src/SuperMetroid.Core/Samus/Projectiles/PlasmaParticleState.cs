namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Plasma special attack ring's flight (#2346): it circles Samus from <see cref="BeamComboDefinitions.PlasmaStartRadius"/>,
/// growing, shrinking and growing again by its phase.
/// </summary>
/// <param name="part">The part the particle plays in its attack.</param>
/// <param name="turnPerUpdate">The attack's turn round Samus each update, set by the way Samus faces as it starts.</param>
internal sealed partial class PlasmaParticleState(SpecialAttackParticle part, TrigonometryAngle turnPerUpdate)
    : OrbitingParticleState(part, part.OrbitStart, BeamComboDefinitions.PlasmaStartRadius, turnPerUpdate)
{
    /// <summary>The ring's phase.</summary>
    internal PlasmaRingPhase Phase { get; set; } = PlasmaRingPhase.Expanding;
}
