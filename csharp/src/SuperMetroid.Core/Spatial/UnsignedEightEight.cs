namespace SuperMetroid.Core.Spatial;

/// <summary>
/// Unsigned 8.8 fixed-point numbers (#223, #1420): whole units above eight fraction bits, the form of the radii,
/// speeds and accelerations the game advances by a fraction each frame.
/// </summary>
[EncodingOwner("unsigned 8.8 fixed-point number")]
public static class UnsignedEightEight
{
    /// <summary>The bits of the fraction, below the whole units.</summary>
    private const int FractionBits = 8;

    /// <summary>The whole units of an unsigned 8.8 word.</summary>
    public static byte Whole(ushort value) => unchecked((byte)(value >> FractionBits));

    /// <summary>The whole units of an unsigned 8.8 word held as an int, 0 through 65535; other values are rejected.</summary>
    public static byte Whole(int value) => Whole(RequireWord(value));

    /// <summary>An unsigned 8.8 word with its fraction dropped: the whole units kept in place.</summary>
    public static ushort WithoutFraction(ushort value) => (ushort)(Whole(value) << FractionBits);

    /// <summary>An unsigned 8.8 word held as an int, 0 through 65535, with its fraction dropped.</summary>
    public static ushort WithoutFraction(int value) => WithoutFraction(RequireWord(value));

    /// <summary>The unsigned 8.8 word with <paramref name="whole"/> units and <paramref name="fraction"/> 1/256ths.</summary>
    /// <param name="whole">The whole units, 0 through 255; other values are rejected.</param>
    /// <param name="fraction">The 1/256ths, 0 through 255; other values are rejected.</param>
    public static int FromParts(int whole, int fraction) => RequirePart(whole) << FractionBits | RequirePart(fraction);

    /// <summary>A whole or fraction part held as an int, checked to lie in 0 through 255.</summary>
    private static int RequirePart(int value) => Ensure.BetweenInclusive(0, byte.MaxValue, value);

    /// <summary>A word held as an int, checked to lie in 0 through 65535.</summary>
    private static ushort RequireWord(int value) => checked((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, value));
}
