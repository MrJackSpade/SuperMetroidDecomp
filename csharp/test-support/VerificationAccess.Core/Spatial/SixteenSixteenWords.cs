namespace SuperMetroid.Core.Spatial;

/// <summary>
/// Unsigned 16.16 words as the cartridge stores them (#1547): checks and native importers compose and split the recorded
/// whole and fraction words; the game holds typed fixed-point values instead.
/// </summary>
internal static class SixteenSixteenWords
{
    /// <summary>Sixteen low bits hold the fraction word.</summary>
    private const int FractionBits = 16;

    /// <summary>The 16.16 word pair with whole part <paramref name="whole"/> and no fraction.</summary>
    internal static uint FromWhole(uint whole) => whole << FractionBits;

    /// <summary>The 16.16 word pair of a whole word and a fraction word, each 0 through 65535.</summary>
    internal static uint Compose(int whole, int fraction) =>
        (uint)Ensure.BetweenInclusive(0, ushort.MaxValue, whole) << FractionBits |
        (uint)Ensure.BetweenInclusive(0, ushort.MaxValue, fraction);

    /// <summary>The fraction word of a 16.16 word pair.</summary>
    internal static uint FractionOf(uint raw) => raw & ushort.MaxValue;
}
