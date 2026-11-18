namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of 16.16 fixed-point numbers that only verification reads.</summary>
internal static class SixteenSixteenConstants
{
    /// <summary>Sixteen low bits hold the fraction.</summary>
    private const int FractionBits = 16;

    /// <summary>The whole part of a raw 16.16 value.</summary>
    public static int WholeOf(int raw) => raw >> FractionBits;
}
