namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which particle of a running special attack holds off a new Ice or Plasma attack (#1683, was <c>BeamCombos</c>' test of
/// the first projectile place): the lead particle, the first the attack set off. A new Ice attack is refused while the
/// last attack's lead particle still orbits as an Ice particle, inward or flying out, and a new Plasma attack while it
/// still orbits as a Plasma ring; once the lead has ended, struck by an enemy or gone from the view, a new attack of
/// the kind starts although the rest of its particles still orbit, and replaces them. The Wave and Spazer attacks are
/// never refused.
/// </summary>
/// <remarks>
/// Consulted once per special-attack start, after its Power Bomb cost, before its particles are set off. Evidence: the
/// Ice and Plasma starts (<c>$90:CD9B</c>, <c>$90:CE98</c>) compare only the first projectile place's update with the
/// Ice particle's two updates or the Plasma ring's, the attack sets its lead particle off in that place, and ending a
/// projectile clears its update (<c>$90:ADB7</c>). Every live particle clears the charge on its update, so a start is
/// reached while particles still orbit only through frozen time: while time is frozen the HUD handler still charges
/// the beam and starts special attacks when the run button is not held (<c>$90:DD4D</c>, <c>$90:DDC8</c>), but the
/// projectiles are not updated (<c>$90:DCFB</c>). The special-attack check compares the rule with the first place's
/// test in every state an attack leaves.
/// </remarks>
[CompatibilityService("an Ice or Plasma special attack holds off another of its kind only while its lead particle still orbits")]
internal static class LeadParticleHoldsOffOrbitingAttack
{
    /// <summary>The particle of a starting attack that holds off the next Ice or Plasma attack: its lead.</summary>
    /// <typeparam name="TParticle">The particle type.</typeparam>
    /// <param name="particlesInStartOrder">The attack's particles, the lead first.</param>
    internal static TParticle HoldingParticle<TParticle>(IReadOnlyList<TParticle> particlesInStartOrder) =>
        Ensure.NotNull(particlesInStartOrder)[0];
}
