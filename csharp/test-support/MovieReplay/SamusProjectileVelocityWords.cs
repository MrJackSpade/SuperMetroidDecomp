using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// Rebuilds the original game's projectile velocity words from a slot's typed state, for comparison with native memory.
/// The game keeps a special-attack particle's orbit radius, lifetime, ring phase and angle step, and a Shinespark crash
/// echo's radius, in its projectile's X and Y velocity words; the port keeps each in its own field, so a particle's
/// unused velocity holds what the slot's previous projectile left, as the native word would without the reuse.
/// </summary>
internal static class SamusProjectileVelocityWords
{
    /// <summary>The native X velocity word of <paramref name="slot"/>.</summary>
    internal static ushort X(SamusProjectile slot)
    {
        if (slot.FrameBehavior is SamusProjectileFrameBehavior.IceComboOutward or SamusProjectileFrameBehavior.PlasmaCombo or
            SamusProjectileFrameBehavior.SpazerCombo or SamusProjectileFrameBehavior.SpazerComboFalling)
            return checked((ushort)slot.ComboOrbitRadius);
        if (slot.FrameBehavior == SamusProjectileFrameBehavior.ShinesparkEcho)
            return checked((ushort)slot.EchoRadius);
        return slot.Velocity.X.RawValue;
    }

    /// <summary>The native Y velocity word of <paramref name="slot"/>.</summary>
    internal static ushort Y(SamusProjectile slot)
    {
        if (slot.FrameBehavior is SamusProjectileFrameBehavior.IceCombo or SamusProjectileFrameBehavior.IceComboOutward or
            SamusProjectileFrameBehavior.WaveCombo)
            return checked((ushort)slot.ComboLifetime);
        if (slot.FrameBehavior == SamusProjectileFrameBehavior.PlasmaCombo)
            return PlasmaPhaseWord(slot.PlasmaPhase);
        if (slot.FrameBehavior is SamusProjectileFrameBehavior.SpazerCombo or SamusProjectileFrameBehavior.SpazerComboFalling)
            return SignedStepWord(slot.SpazerTurnPerUpdate);
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
