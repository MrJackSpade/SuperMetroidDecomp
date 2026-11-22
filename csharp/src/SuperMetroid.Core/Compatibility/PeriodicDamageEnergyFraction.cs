namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The fraction of one energy Samus holds beyond her whole energy, which periodic damage takes from first (#1763): heat,
/// lava, acid, spikes, a chewing plant and the other periodic damage take her energy a fraction at a time, and a fraction
/// that is more than the one she holds borrows a whole energy, the rest of that energy becoming the fraction she holds.
/// Her fraction persists from one periodic damage to the next, so how often the whole energy shown on the HUD drops
/// depends on what earlier damage left, not only on the rate she now takes. The arithmetic is the one a Metroid's drain
/// takes energy by (<see cref="FractionalEnergyDrain"/>), here applied to the fraction Samus herself holds.
/// </summary>
/// <remarks>
/// Consulted once an update, at the end of Samus's update, where the periodic damage the update accumulated is reduced
/// by her suit and applied (<c>SamusPeriodicDamage.Apply</c>); a fatal result empties it
/// (<see cref="None"/>). A value owned by Samus's state, starting empty with each new Samus. Evidence: the original
/// periodic damage subtracts the fraction from Samus's held fraction of one energy's
/// parts, the parts a <see cref="FractionalEnergyDrain"/> counts in, and takes one more energy when the subtraction borrows,
/// keeping the wrapped remainder; the atmospheric effects check (<c>VerifySamusAtmosphericEffects</c>) covers a covered
/// fraction, a borrowing one, and a fatal borrow and fatal whole damage that empty it.
/// </remarks>
public readonly partial record struct PeriodicDamageEnergyFraction
{

    /// <summary>The held fraction, kept as the drain the shared arithmetic runs on.</summary>
    private readonly FractionalEnergyDrain _held;

    /// <summary>Stores the held fraction; callers use <see cref="None"/> or <see cref="After"/>.</summary>
    private PeriodicDamageEnergyFraction(FractionalEnergyDrain held) => _held = held;

    /// <summary>The fraction held, 0 through one part short of an energy, in <c>PartsPerEnergy</c>ths.</summary>
    internal int Parts => _held.HeldOver;

    /// <summary>No fraction held: a new Samus's, and the one a fatal periodic damage leaves.</summary>
    public static PeriodicDamageEnergyFraction None => default;


    /// <summary>Whether taking <paramref name="damage"/>'s fraction borrows a whole energy: whether it is more than the fraction held.</summary>
    /// <param name="damage">The periodic damage, whose fraction of an energy is taken.</param>
    public bool Borrows(EnergyAmount damage) => _held.TakesEnergy(damage.FractionParts);

    /// <summary>
    /// The fraction held after <paramref name="damage"/>'s fraction is taken: the held fraction less it, or, when that
    /// borrows (<see cref="Borrows"/>), the rest of the energy borrowed.
    /// </summary>
    /// <param name="damage">The periodic damage, whose fraction of an energy is taken.</param>
    public PeriodicDamageEnergyFraction After(EnergyAmount damage) => new(_held.After(damage.FractionParts));

    /// <inheritdoc />
    public override string ToString() => Parts.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
