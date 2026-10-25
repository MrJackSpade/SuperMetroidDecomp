namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Lossless view of the five-byte spritemap record's encoded Y byte: a signed vertical displacement that the OAM
/// packer adds to the origin's low byte, testing the unsigned carry to hide pieces that wrap across the screen edge.
/// </summary>
/// <param name="Raw">The encoded Y byte, the displacement modulo 256.</param>
[EncodingOwner("spritemap part Y byte")]
public readonly record struct SnesSpritemapYByte(byte Raw)
{
    /// <summary>The smallest displacement the byte encodes.</summary>
    public const int MinimumOffset = sbyte.MinValue;

    /// <summary>The largest displacement the byte encodes.</summary>
    public const int MaximumOffset = sbyte.MaxValue;

    /// <summary>The signed vertical displacement from the sprite origin, -128 through 127 pixels.</summary>
    public int SignedOffset => unchecked((sbyte)Raw);

    /// <summary>Whether the displacement points above the origin.</summary>
    public bool IsNegative => SignedOffset < 0;

    /// <summary>Encodes a signed vertical displacement.</summary>
    /// <param name="offset">The displacement from the sprite origin, from -128 through 127 pixels.</param>
    /// <exception cref="ArgumentOutOfRangeException">The displacement does not fit a signed byte.</exception>
    public static SnesSpritemapYByte Create(int offset) =>
        new(unchecked((byte)Ensure.BetweenInclusive(MinimumOffset, MaximumOffset, offset)));
}
