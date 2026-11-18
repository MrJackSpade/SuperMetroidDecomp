using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The periodic damage Samus takes: liquids, spikes, heat, the grapple's spikes and Draygon's turrets, and enemies and
/// room objects add to one amount through an update, and Samus's movement applies it once, divided by her suit.
/// </summary>
public sealed class SamusPeriodicDamage
{
    /// <summary>
    /// The periodic damage accumulated and not yet applied, in energy and its 65536ths: <see cref="Apply"/> applies and
    /// clears it once an update, so it holds no more than one update's contributions.
    /// </summary>
    public UnsignedSixteenSixteen Pending { get; private set; }

    /// <summary>Adds one periodic-damage contribution, in energy and its 65536ths.</summary>
    /// <remarks>
    /// Liquid, block and enemy damage all add to one amount, so fractions that together pass a whole energy add one to
    /// the whole damage.
    /// </remarks>
    public void Accumulate(UnsignedSixteenSixteen damage) => Pending += damage;

    /// <summary>
    /// Applies the accumulated damage to Samus: the suit's periodic division (<see cref="SamusSuitDamage.ReducePeriodic"/>),
    /// the fraction taken first, then the whole energy, a fatal result clamped below zero, then clears it. Frozen time
    /// clears it without applying it.
    /// </summary>
    public void Apply(SamusState samus, bool timeIsFrozen)
    {
        Ensure.NotNull(samus);
        if (timeIsFrozen)
        {
            Pending = default;
            return;
        }

        UnsignedSixteenSixteen applied = SamusSuitDamage.ReducePeriodic(Pending, samus.EquippedItems);

        // The game stops with a crash when the whole damage is so large it reads as below zero.
        // Reachable environmental rates never do this; throwing retains the overflow guard instead of converting it
        // into plausible damage.
        if (WrappingCounter.ReadsBelowZero(applied.Whole))
            throw new InvalidDataException("Periodic whole damage overflowed into the signed-negative range.");

        samus.TakeFractionalEnergyDamage(applied.Fraction, applied.Whole);

        Pending = default;
    }

    /// <summary>Drops the accumulated damage unapplied, as entering a room without a liquid does (<see cref="SamusLiquidPhysicsState.Clear"/>).</summary>
    internal void Clear() => Pending = default;
}
