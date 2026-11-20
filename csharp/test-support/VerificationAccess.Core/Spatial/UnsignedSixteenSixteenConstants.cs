namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of <see cref="UnsignedSixteenSixteen"/> that only verification reads.</summary>
internal static class UnsignedSixteenSixteenConstants
{
    /// <summary>
    /// The speed that is the product of two non-negative quantities each counted in 256ths, as the grapple release
    /// scales the swing's speed by a trigonometric component: 256ths times 256ths are 65536ths of a pixel. The product
    /// must fit the type.
    /// </summary>
    /// <param name="twoFiftySixths">The first factor in 256ths, zero or more.</param>
    /// <param name="otherTwoFiftySixths">The second factor in 256ths, zero or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">A factor is negative.</exception>
    /// <exception cref="OverflowException">The product lies outside the type's range.</exception>
    public static UnsignedSixteenSixteen ProductOfTwoFiftySixths(int twoFiftySixths, int otherTwoFiftySixths) =>
        UnsignedSixteenSixteen.FromRaw(checked((uint)Ensure.BetweenInclusive(0, int.MaxValue, twoFiftySixths) *
            (uint)Ensure.BetweenInclusive(0, int.MaxValue, otherTwoFiftySixths)));
}
