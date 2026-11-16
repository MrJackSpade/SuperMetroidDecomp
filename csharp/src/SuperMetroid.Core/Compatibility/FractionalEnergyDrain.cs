namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// A drain that takes Samus's energy a fraction at a time (#1752): each update it drains a fraction of one energy, and it
/// takes a whole energy from her whenever that fraction is more than what is held over from the last energy it took, the
/// rest of that energy being held over for the updates after. A drain begins with nothing held over, so its first update
/// takes an energy at once, and a fraction that exactly uses up what is held over takes none, leaving nothing held over
/// for the next update to take an energy again. The cadence of the energy taken therefore depends on where the drain
/// began, not only on the fraction: a Metroid's grip takes an energy on the update it starts draining, and then once every
/// so many updates as its suit's fraction gives. Samus's periodic damage takes from the fraction of an energy she holds by
/// the same arithmetic (<see cref="PeriodicDamageEnergyFraction"/>).
/// </summary>
/// <remarks>
/// Consulted on every update a Metroid stays attached to Samus, during the enemy pass, as its drain takes its suit's
/// fraction (<c>MetroidBehavior</c>); started empty when the Metroid spawns. A value owned by the Metroid's state.
/// Evidence: the original drain subtracts the fraction from a held-over count of one energy's 65536 parts and takes one
/// energy when the subtraction borrows, keeping the wrapped remainder; the Metroid drain counter check
/// (<c>MetroidDrainCountersVerification</c>) exercises a fraction that uses up what is held over exactly and one that
/// exceeds it, and the wrapped remainder it leaves.
/// </remarks>
[CompatibilityService("a Metroid's drain takes one energy whenever its suit's fraction exceeds what is held over from the last energy taken, keeping the rest of that energy for later updates")]
public readonly record struct FractionalEnergyDrain
{
    /// <summary>The parts one energy is drained in: a fraction and what is held over are counted in these parts.</summary>
    public const int PartsPerEnergy = WrappingCounter.LargestCount + 1;

    /// <summary>Stores what is held over, 0 through one part short of an energy; callers use <see cref="HoldingOver"/>.</summary>
    private FractionalEnergyDrain(int heldOver) => HeldOver = Ensure.BetweenInclusive(0, PartsPerEnergy - 1, heldOver);

    /// <summary>What is held over from the last energy taken, 0 through one part short of an energy, in <see cref="PartsPerEnergy"/>ths.</summary>
    public int HeldOver { get; }

    /// <summary>A drain that has taken nothing yet: nothing is held over, so its first fraction takes an energy.</summary>
    public static FractionalEnergyDrain Start => default;

    /// <summary>A drain holding <paramref name="heldOver"/> parts over, 0 through one part short of an energy, as a parity check states one.</summary>
    public static FractionalEnergyDrain HoldingOver(int heldOver) => new(heldOver);

    /// <summary>Whether draining <paramref name="fraction"/> takes an energy: whether it is more than what is held over.</summary>
    /// <param name="fraction">One update's fraction, 0 through one part short of an energy.</param>
    public bool TakesEnergy(int fraction) =>
        Ensure.BetweenInclusive(0, PartsPerEnergy - 1, fraction) > HeldOver;

    /// <summary>
    /// The drain after <paramref name="fraction"/> is drained: what is held over less the fraction, or, when that takes an
    /// energy (<see cref="TakesEnergy"/>), the rest of the energy taken.
    /// </summary>
    /// <param name="fraction">One update's fraction, 0 through one part short of an energy.</param>
    public FractionalEnergyDrain After(int fraction) => new(WrappingCounter.SubtractCount(HeldOver, fraction));

    /// <inheritdoc />
    public override string ToString() => HeldOver.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
