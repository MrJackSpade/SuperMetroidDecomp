namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The two-bit high-OAM pair belonging to one sprite: X coordinate bit eight followed by
/// the size-select bit. Four independently encoded pairs share each physical table byte.
/// </summary>
[SnesEncodingOwner]
public readonly record struct SnesOamHighTablePair
{
    /// <summary>Each sprite occupies two bits of a shared high-OAM byte.</summary>
    private const byte PairMask = 0x03;
    /// <summary>The first bit extends the low-OAM X coordinate to nine bits.</summary>
    private const byte XHighMask = 0x01;
    /// <summary>The second bit selects the larger configured OBJ size.</summary>
    private const byte LargeObjectMask = 0x02;

    /// <summary>Gets the unshifted two-bit pair, from 0 through 3, before placement in a shared high-OAM byte.</summary>
    public byte Raw { get; }

    /// <summary>Gets bit eight of the sprite's nine-bit screen X coordinate.</summary>
    public bool XHigh => (Raw & XHighMask) != 0;
    /// <summary>Gets whether the sprite selects the larger of the PPU's configured OBJ sizes.</summary>
    public bool IsLarge => (Raw & LargeObjectMask) != 0;

    /// <summary>Encodes the sprite's X high bit and size selector as an unshifted high-OAM pair.</summary>
    public static SnesOamHighTablePair Create(bool xHigh, bool isLarge) =>
        new(unchecked((byte)((xHigh ? XHighMask : 0) | (isLarge ? LargeObjectMask : 0))));

    /// <summary>Builds the pair from a nine-bit screen X and a spritemap size selector.</summary>
    public static SnesOamHighTablePair FromSprite(ushort screenX, bool isLarge) =>
        Create((screenX & 0x0100) != 0, isLarge);

    /// <summary>Wraps an unshifted high-OAM pair, rejecting bits belonging to other sprites.</summary>
    /// <param name="raw">The two-bit value, from 0 through 3.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value has any bit set outside the low two bits.</exception>
    public SnesOamHighTablePair(byte raw) : this()
    {
        if ((raw & ~PairMask) != 0)
            throw new ArgumentOutOfRangeException(nameof(raw));
        Raw = raw;
    }
}
