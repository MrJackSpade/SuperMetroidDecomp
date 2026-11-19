namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of <see cref="SignedEightEight"/> that only verification reads.</summary>
internal static class SignedEightEightConstants
{
    /// <summary>A speed of <paramref name="sixteenths"/> sixteenths of a pixel an update, -2048 through 2047.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed lies outside the type's range.</exception>
    public static SignedEightEight FromSixteenthsOfPixel(int sixteenths) =>
        SignedEightEight.FromTwoHundredFiftySixthsOfPixel(Ensure.BetweenInclusive(-2048, 2047, sixteenths) * 16);
}
