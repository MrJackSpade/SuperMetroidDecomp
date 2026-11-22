namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A special attack particle that circles a point at an angle and a distance (#2346): an Ice particle and a Plasma ring
/// round Samus, a Spazer particle round Samus and then round a point above her.
/// </summary>
/// <param name="part">The part the particle plays in its attack.</param>
/// <param name="angle">The angle the particle starts at round its circle.</param>
/// <param name="radius">The distance, in pixels, the particle starts at from the point it circles.</param>
/// <param name="turnPerUpdate">The turn the particle starts making round its circle each update.</param>
internal abstract partial class OrbitingParticleState(SpecialAttackParticle part, TrigonometryAngle angle, int radius,
    TrigonometryAngle turnPerUpdate) : SpecialAttackParticleState(part)
{
    /// <summary>The particle's angle round its circle.</summary>
    internal TrigonometryAngle Angle { get; set; } = angle;

    /// <summary>The distance, in pixels, from the point the particle circles that its angle places it at.</summary>
    internal int Radius { get; set => field = Ensure.AtLeastZero(value); } = Ensure.AtLeastZero(radius);

    /// <summary>
    /// The turn the particle makes round its circle each update: for an Ice or Plasma particle its steps on, or as many
    /// steps back (a full circle less them) when a left-facing Samus starts the attack; for a Spazer particle its steps
    /// on when it turns clockwise, a full circle less them when it turns counterclockwise.
    /// </summary>
    internal TrigonometryAngle TurnPerUpdate { get; set; } = turnPerUpdate;

    /// <summary>Turns the particle on round its circle by its turn per update.</summary>
    internal void Turn() => Angle += TurnPerUpdate;
}
