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

    /// <summary>The whole units of an unsigned 8.8 value.</summary>
    public static byte Whole(ushort value) => unchecked((byte)(value >> FractionBits));

    /// <summary>The whole units of an unsigned 8.8 value held as an int, 0 through 65535; other values are rejected.</summary>
    public static byte Whole(int value) => Whole(RequireValue(value));

    /// <summary>A value held as an int, checked to lie in 0 through 65535.</summary>
    private static ushort RequireValue(int value) => checked((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, value));
}
