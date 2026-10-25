namespace SuperMetroid.Core.Spatial;

/// <summary>
/// Unsigned 8.8 fixed-point numbers (#223, #1420): whole units above eight fraction bits, the form of the radii,
/// speeds and accelerations the game advances by a fraction each frame.
/// </summary>
[EncodingOwner("unsigned 8.8 fixed-point number")]
public static class UnsignedEightEight
{
    /// <summary>The whole units of an unsigned 8.8 word.</summary>
    public static byte Whole(ushort value) => unchecked((byte)(value >> 8));
}
