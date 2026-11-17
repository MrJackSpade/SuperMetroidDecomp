namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of <see cref="SixteenSixteen"/> that only verification reads.</summary>
internal static class SixteenSixteenConstants3
{
    /// <summary>Extracts the fraction without changing the signed whole half.</summary>
    internal const int FractionMask = 0xffff;
    /// <summary>The fraction of a raw 16.16 value.</summary>
    public static int FractionOf(int raw) => raw & FractionMask;
}
