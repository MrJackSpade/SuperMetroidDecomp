namespace SuperMetroid.Core.Spatial;

/// <summary>
/// The original program's arithmetic on raw 8.8 fixed-point words (#2428): whole units above eight fraction bits of
/// 1/256th each, as checks state the arithmetic a typed operation replaced. The game holds typed fixed-point values and
/// their named operations instead (<see cref="SignedEightEight"/>, <see cref="UnitComponent"/>,
/// <see cref="UnsignedSixteenSixteen"/>); only verification composes, splits and multiplies the raw words.
/// </summary>
internal static class EightEightOracle
{
    /// <summary>Eight low bits hold one unit's fraction.</summary>
    private const int FractionBits = 8;

    /// <summary>Sixteen low bits hold a 16.16 value's fraction.</summary>
    private const int SixteenSixteenFractionBits = 16;

    /// <summary>The raw 8.8 value with whole part <paramref name="whole"/> and no fraction.</summary>
    public static int FromWhole(int whole) => whole << FractionBits;

    /// <summary><paramref name="value"/> times an 8.8 factor, the product's whole units rounded toward negative infinity.</summary>
    public static int Multiply(int value, int factor) => value * factor >> FractionBits;

    /// <summary>The whole part of a raw 8.8 value, rounded toward negative infinity.</summary>
    public static int WholeOf(int raw) => raw >> FractionBits;

    /// <inheritdoc cref="WholeOf(int)"/>
    public static uint WholeOf(uint raw) => raw >> FractionBits;

    /// <summary>The fraction of a raw 8.8 value.</summary>
    public static int FractionOf(int raw) => raw & byte.MaxValue;

    /// <summary>The 8.8 value of a 16.16 whole and fraction pair, the fraction truncated to 256ths.</summary>
    public static int FromSixteenSixteen(int whole, int fraction) =>
        (whole << FractionBits) | (fraction >> (SixteenSixteenFractionBits - FractionBits));

    /// <summary>A raw 8.8 value widened to 16.16: whole units move to the whole part, 1/256ths to the fraction's upper half.</summary>
    public static int ToSixteenSixteen(int raw) => raw << (SixteenSixteenFractionBits - FractionBits);

    /// <summary>The whole units of an unsigned 8.8 word.</summary>
    public static byte UnsignedWhole(ushort value) => unchecked((byte)(value >> FractionBits));

    /// <summary>The whole units of an unsigned 8.8 word held as an int, 0 through 65535; other values are rejected.</summary>
    public static byte UnsignedWhole(int value) =>
        UnsignedWhole(checked((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, value)));
}
