namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Cartridge values and helpers of <see cref="ListArithmetic"/> that only verification reads.</summary>
internal static class ListArithmeticConstants
{
    /// <summary>Four times <paramref name="value"/>, which only checks compute.</summary>
    internal static int Quadruple(int value) => ListArithmetic.Twice(ListArithmetic.Twice(value));
}
