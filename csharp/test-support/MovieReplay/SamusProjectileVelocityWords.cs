using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// Rebuilds the original game's projectile velocity words from a slot's typed state, for comparison with native memory.
/// The game keeps a special-attack particle's orbit radius, lifetime, ring phase and angle step, and a Shinespark crash
/// echo's radius, in its projectile's X and Y velocity words; the port keeps each in its kind's own record
/// (<see cref="SamusShotKindState"/>), so a particle's unused velocity holds what the slot's previous projectile left, as
/// the native word would without the reuse.
/// </summary>
internal static class SamusProjectileVelocityWords
{
    /// <summary>The native X velocity word of <paramref name="slot"/>.</summary>
    internal static ushort X(SamusProjectile slot)
    {
        if (slot.Behavior is IceSpecialAttackSpread or PlasmaSpecialAttackRing or SpazerSpecialAttackFlight)
            return checked((ushort)slot.KindStateAs<OrbitingParticleState>().Radius);
        if (slot.Behavior is ShinesparkEchoFlight)
            return (ushort)(slot.KindStateAs<ShinesparkEchoShotState>().Radius % 65536); // The game kept the growing radius in a 16-bit word.
        return slot.Velocity.X.RawValue;
    }

    /// <summary>The native Y velocity word of <paramref name="slot"/>.</summary>
    internal static ushort Y(SamusProjectile slot)
    {
        if (slot.Behavior is IceSpecialAttackOrbit or IceSpecialAttackSpread)
            return checked((ushort)slot.KindStateAs<IceParticleState>().Lifetime.UpdatesLeft);
        if (slot.Behavior is WaveSpecialAttackHoming)
            return checked((ushort)slot.KindStateAs<WaveParticleState>().Lifetime.UpdatesLeft);
        if (slot.Behavior is PlasmaSpecialAttackRing)
            return PlasmaPhaseWord(slot.KindStateAs<PlasmaParticleState>().Phase);
        if (slot.Behavior is SpazerSpecialAttackFlight)
            return SignedStepWord(global::SuperMetroid.Core.PrivateState.Get<global::SuperMetroid.Core.Samus.Projectiles.OrbitingParticleState, global::SuperMetroid.Core.Spatial.TrigonometryAngle>(slot.KindStateAs<SpazerParticleState>(), "TurnPerUpdate"));
        return slot.Velocity.Y.RawValue;
    }

    /// <summary>
    /// The signed word the game keeps a turn per update in: the turn's steps on, or, for a turn of more than a half turn,
    /// the steps back it makes, negative.
    /// </summary>
    private static ushort SignedStepWord(TrigonometryAngle turn) => unchecked((ushort)turn.StepsFrom(TrigonometryAngle.Zero));

    /// <summary>The number the game counts a Plasma ring's phase by: zero, one and two in the order the ring runs them.</summary>
    private static ushort PlasmaPhaseWord(PlasmaRingPhase phase) => phase switch
    {
        PlasmaRingPhase.Expanding => 0,
        PlasmaRingPhase.Contracting => 1,
        PlasmaRingPhase.FinalExpansion => 2,
        _ => throw new InvalidDataException($"Undefined {nameof(PlasmaRingPhase)} {phase}."),
    };
}
