namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of 16.16 fixed-point numbers that only verification reads.</summary>
internal static class SixteenSixteenConstants2
{
    /// <summary>Sixteen low bits hold the fraction.</summary>
    private const int FractionBits = 16;

    /// <summary>The raw 16.16 value with whole part <paramref name="whole"/> and no fraction.</summary>
    public static int FromWhole(int whole) => whole << FractionBits;
}
