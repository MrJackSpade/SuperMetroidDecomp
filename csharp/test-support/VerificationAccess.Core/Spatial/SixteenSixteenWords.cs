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

    /// <summary>
    /// The signed displacement a recorded whole word and fraction word store, each 0 through 65535: the whole word read
    /// as signed whole pixels above the fraction.
    /// </summary>
    internal static SignedSixteenSixteen ToDisplacement(int whole, int fraction) =>
        SignedSixteenSixteen.FromParts(unchecked((short)Ensure.BetweenInclusive(0, ushort.MaxValue, whole)), fraction);

    /// <summary>
    /// The signed displacement a check's 16.16 word pair, held as one signed 32-bit word, stores: its high half read as
    /// signed whole pixels above its low half's fraction.
    /// </summary>
    internal static SignedSixteenSixteen ToDisplacement(int word) =>
        SignedSixteenSixteen.FromParts(word >> FractionBits, word & ushort.MaxValue);

    /// <summary>The whole word of a 16.16 word pair.</summary>
    internal static uint WholeOf(uint raw) => raw >> FractionBits;

    /// <summary>The fraction word of a 16.16 word pair.</summary>
    internal static uint FractionOf(uint raw) => raw & ushort.MaxValue;
}
