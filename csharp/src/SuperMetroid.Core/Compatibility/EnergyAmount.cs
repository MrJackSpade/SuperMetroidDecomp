namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// An amount of Samus's energy that periodic damage counts (#2555): whole energy and a fraction of one energy, never
/// negative. Liquids, spikes, heat, the grapple's spikes and the room objects that chew her each add such an amount to an
/// update's damage, her suit divides it, and the fraction comes off the fraction of an energy she holds before the whole
/// energy comes off her health (<see cref="PeriodicDamageEnergyFraction"/>).
/// </summary>
/// <remarks>
/// Added to on every update a periodic damage source touches Samus, divided by her suit and applied once an update.
/// Evidence: the original keeps an update's periodic damage as a whole word and a fraction word that its sources add to
/// with a carry from the fraction into the whole; the periodic damage and suit damage checks compare every amount with
/// that arithmetic.
/// </remarks>
[EncodingOwner("periodic damage energy amount: whole energy and a sixteen-bit fraction of one energy")]
[CompatibilityService("periodic damage counts Samus's energy in whole energy and a fraction of one, which fractions together passing a whole energy add to")]
public readonly record struct EnergyAmount
{
    /// <summary>Keeps the amount at the precision periodic damage counts it, whole energy above its fraction.</summary>
    private EnergyAmount(UnsignedSixteenSixteen amount) => _amount = amount;

    /// <summary>The amount, whole energy above a sixteen-bit fraction of one energy.</summary>
    private readonly UnsignedSixteenSixteen _amount;

    /// <summary>No energy at all.</summary>
    public static EnergyAmount Zero => default;

    /// <summary>An amount of <paramref name="energy"/> whole energy, 0 through 65535, with no fraction.</summary>
    public static EnergyAmount FromEnergy(int energy) => new(UnsignedSixteenSixteen.FromWholePixels(energy));

    /// <summary>
    /// An amount of <paramref name="numerator"/> <paramref name="denominator"/>ths of an energy, the denominator a power of
    /// two from 1 through 65536 so the fraction is kept exactly: a quarter of an energy, or one and a half.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The numerator is negative, or the denominator is no power of two from 1 through 65536.
    /// </exception>
    public static EnergyAmount FromEnergyFraction(int numerator, int denominator) =>
        new(UnsignedSixteenSixteen.FromPixelFraction(numerator, denominator));

    /// <summary>The amount's whole energy, its fraction of an energy dropped.</summary>
    [IntegerQuantity("whole energy of a periodic damage amount")]
    public int WholeEnergy => _amount.Whole;

    /// <summary>The amount's fraction of an energy, in the parts <see cref="PeriodicDamageEnergyFraction"/> counts.</summary>
    internal int FractionParts => _amount.Fraction;

    /// <summary>Two amounts added, their fractions together passing a whole energy adding one to the whole energy.</summary>
    public static EnergyAmount operator +(EnergyAmount left, EnergyAmount right) => new(left._amount + right._amount);

    /// <summary>
    /// The amount divided by <paramref name="divisor"/>, one or more, as a suit divides periodic damage: counted to the
    /// 256th of an energy, the finer part of the fraction dropped, and the quotient rounded down.
    /// </summary>
    public EnergyAmount DividedAsSuitsDivide(int divisor) => new(_amount.DividedInTwoFiftySixths(divisor));
}
